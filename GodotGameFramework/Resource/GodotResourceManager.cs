//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Download;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// Godot 资源管理器，直接实现 IResourceManager。
    ///
    /// 设计说明：
    ///   - 不继承原版 ResourceManager（AssetBundle 体系），完全从 Godot API 重写。
    ///   - PackageMode  : 所有资源通过 res:// 路径加载。
    ///   - UpdatableMode: 优先从 user:// (ReadWritePath) 查找，缺失时回退 res://；
    ///                    热更包通过 ProjectSettings.LoadResourcePack() 挂载。
    ///   - 异步加载由 GodotLoadResourceAgentHelper 通过 LoadThreadedRequest 实现，
    ///     ResourceComponent._Process 每帧轮询并驱动所有 Agent。
    ///   - 场景采用 additive 模式（SceneTree.ChangeSceneToFile 为 exclusive，
    ///     LoadScene 将场景实例作为子节点挂到 SceneRoot）。
    ///   - 二进制资源（LoadBinary）直接使用 FileAccess 同步或 Task 异步读取。
    ///   - 资源组（ResourceGroup）在 Godot 中退化为纯元数据跟踪，不参与加载决策。
    /// </summary>
    public sealed class GodotResourceManager : IResourceManager
    {
        // ── 内部任务记录 ────────────────────────────────────────────────────────

        private sealed class AssetLoadTask
        {
            public int Id;
            public string AssetName;
            public Type AssetType;
            public int Priority;
            public LoadAssetCallbacks Callbacks;
            public object UserData;
            public float StartTime;
            public GodotLoadResourceAgentHelper Agent; // null = 等待分配
        }

        private sealed class SceneLoadTask
        {
            public string SceneAssetName;
            public int Priority;
            public LoadSceneCallbacks Callbacks;
            public object UserData;
            public float StartTime;
            public GodotLoadResourceAgentHelper Agent;
        }

        // ── additive 场景注册表（供 GodotResourceHelper.UnloadScene 使用） ──────

        private static readonly Dictionary<string, Node> s_AdditiveScenes = new();

        internal static bool TryGetAdditiveScene(string name, out Node node) =>
            s_AdditiveScenes.TryGetValue(name, out node);

        internal static void RemoveAdditiveScene(string name) =>
            s_AdditiveScenes.Remove(name);

        // ── 路径与模式 ─────────────────────────────────────────────────────────

        private string m_ReadOnlyPath = "res://";
        private string m_ReadWritePath = "user://";
        private ResourceMode m_ResourceMode = ResourceMode.Package;
        private string m_CurrentVariant = null;

        // ── Agent 池 ───────────────────────────────────────────────────────────

        private readonly List<GodotLoadResourceAgentHelper> m_Agents = new();
        private readonly Queue<AssetLoadTask> m_WaitingAssetTasks = new();
        private readonly List<AssetLoadTask> m_WorkingAssetTasks = new();
        private readonly List<SceneLoadTask> m_WorkingSceneTasks = new();

        // ── Resource 缓存（简单引用缓存，避免重复加载同一 res 路径） ────────────

        private readonly Dictionary<string, WeakReference<GodotObject>> m_ResourceCache = new();

        // ── 资源组 ─────────────────────────────────────────────────────────────

        private readonly Dictionary<string, GodotResourceGroup> m_ResourceGroups = new();

        // ── 统计 ───────────────────────────────────────────────────────────────

        private int m_AssetCount = 0;

        // ── 版本/更新相关（UpdatableMode 下才有意义）──────────────────────────

        private string m_UpdatePrefixUri = string.Empty;
        private int m_UpdateRetryCount = 3;
        private int m_GenerateReadWriteVersionListLength = 0;

        // ── 未使用（Godot 无 ObjectPool 需求，接口保留） ────────────────────────

        private float m_AssetAutoReleaseInterval = 60f;
        private int m_AssetCapacity = 64;
        private float m_AssetExpireTime = 60f;
        private int m_AssetPriority = 0;
        private float m_ResourceAutoReleaseInterval = 60f;
        private int m_ResourceCapacity = 16;
        private float m_ResourceExpireTime = 60f;
        private int m_ResourcePriority = 0;

        // ── Scene root（由 ResourceComponent 注入）─────────────────────────────

        private Node m_SceneRoot = null;

        internal void SetSceneRoot(Node root) => m_SceneRoot = root;

        // ── IResourceManager 属性 ──────────────────────────────────────────────

        public string ReadOnlyPath => m_ReadOnlyPath;
        public string ReadWritePath => m_ReadWritePath;
        public ResourceMode ResourceMode => m_ResourceMode;
        public string CurrentVariant => m_CurrentVariant;

        public PackageVersionListSerializer PackageVersionListSerializer { get; } = new PackageVersionListSerializer();
        public UpdatableVersionListSerializer UpdatableVersionListSerializer { get; } = new UpdatableVersionListSerializer();
        public ReadOnlyVersionListSerializer ReadOnlyVersionListSerializer { get; } = new ReadOnlyVersionListSerializer();
        public ReadWriteVersionListSerializer ReadWriteVersionListSerializer { get; } = new ReadWriteVersionListSerializer();
        public ResourcePackVersionListSerializer ResourcePackVersionListSerializer { get; } = new ResourcePackVersionListSerializer();

        public string ApplicableGameVersion => ProjectSettings.GetSetting("application/config/version", "1.0").AsString();
        public int InternalResourceVersion => 0;

        public int AssetCount => m_AssetCount;
        public int ResourceCount => m_ResourceCache.Count;
        public int ResourceGroupCount => m_ResourceGroups.Count;

        public string UpdatePrefixUri { get => m_UpdatePrefixUri; set => m_UpdatePrefixUri = value; }
        public int GenerateReadWriteVersionListLength { get => m_GenerateReadWriteVersionListLength; set => m_GenerateReadWriteVersionListLength = value; }
        public string ApplyingResourcePackPath => string.Empty;
        public int ApplyWaitingCount => 0;
        public int UpdateRetryCount { get => m_UpdateRetryCount; set => m_UpdateRetryCount = value; }
        public IResourceGroup UpdatingResourceGroup => null;
        public int UpdateWaitingCount => 0;
        public int UpdateWaitingWhilePlayingCount => 0;
        public int UpdateCandidateCount => 0;

        public int LoadTotalAgentCount => m_Agents.Count;
        public int LoadFreeAgentCount => m_Agents.Count - m_WorkingAssetTasks.Count - m_WorkingSceneTasks.Count;
        public int LoadWorkingAgentCount => m_WorkingAssetTasks.Count + m_WorkingSceneTasks.Count;
        public int LoadWaitingTaskCount => m_WaitingAssetTasks.Count;

        public float AssetAutoReleaseInterval { get => m_AssetAutoReleaseInterval; set => m_AssetAutoReleaseInterval = value; }
        public int AssetCapacity { get => m_AssetCapacity; set => m_AssetCapacity = value; }
        public float AssetExpireTime { get => m_AssetExpireTime; set => m_AssetExpireTime = value; }
        public int AssetPriority { get => m_AssetPriority; set => m_AssetPriority = value; }
        public float ResourceAutoReleaseInterval { get => m_ResourceAutoReleaseInterval; set => m_ResourceAutoReleaseInterval = value; }
        public int ResourceCapacity { get => m_ResourceCapacity; set => m_ResourceCapacity = value; }
        public float ResourceExpireTime { get => m_ResourceExpireTime; set => m_ResourceExpireTime = value; }
        public int ResourcePriority { get => m_ResourcePriority; set => m_ResourcePriority = value; }

        // ── 事件（Verify / Apply / Update 系列，PackageMode 下不会触发） ────────

        public event EventHandler<ResourceVerifyStartEventArgs> ResourceVerifyStart;
        public event EventHandler<ResourceVerifySuccessEventArgs> ResourceVerifySuccess;
        public event EventHandler<ResourceVerifyFailureEventArgs> ResourceVerifyFailure;
        public event EventHandler<ResourceApplyStartEventArgs> ResourceApplyStart;
        public event EventHandler<ResourceApplySuccessEventArgs> ResourceApplySuccess;
        public event EventHandler<ResourceApplyFailureEventArgs> ResourceApplyFailure;
        public event EventHandler<ResourceUpdateStartEventArgs> ResourceUpdateStart;
        public event EventHandler<ResourceUpdateChangedEventArgs> ResourceUpdateChanged;
        public event EventHandler<ResourceUpdateSuccessEventArgs> ResourceUpdateSuccess;
        public event EventHandler<ResourceUpdateFailureEventArgs> ResourceUpdateFailure;
        public event EventHandler<ResourceUpdateAllCompleteEventArgs> ResourceUpdateAllComplete;

        // ── IResourceManager 配置方法 ──────────────────────────────────────────

        public void SetReadOnlyPath(string readOnlyPath) => m_ReadOnlyPath = readOnlyPath;
        public void SetReadWritePath(string readWritePath) => m_ReadWritePath = readWritePath;
        public void SetResourceMode(ResourceMode resourceMode) => m_ResourceMode = resourceMode;
        public void SetCurrentVariant(string currentVariant) => m_CurrentVariant = currentVariant;
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager) { /* Godot 不依赖外部对象池 */ }
        public void SetFileSystemManager(IFileSystemManager fileSystemManager) { /* Godot FileAccess 替代 */ }
        public void SetDownloadManager(IDownloadManager downloadManager) { /* UpdatableMode 扩展点 */ }
        public void SetDecryptResourceCallback(DecryptResourceCallback decryptResourceCallback) { /* 暂不支持加密 */ }
        public void SetResourceHelper(IResourceHelper resourceHelper) { /* 由组件注入，此处忽略 */ }

        public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper)
        {
            if (loadResourceAgentHelper is GodotLoadResourceAgentHelper helper)
                m_Agents.Add(helper);
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 单机模式初始化：Godot res:// 内置资源始终可用，直接回调完成。
        /// </summary>
        public void InitResources(InitResourcesCompleteCallback initResourcesCompleteCallback)
        {
            initResourcesCompleteCallback?.Invoke();
        }

        // ── 版本检查 / 更新（UpdatableMode 接口，PackageMode 下不实现） ──────────

        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion) =>
            CheckVersionListResult.Updated;

        public void UpdateVersionList(int versionListLength, int versionListHashCode,
            int versionListCompressedLength, int versionListCompressedHashCode,
            UpdateVersionListCallbacks updateVersionListCallbacks)
        {
            updateVersionListCallbacks.UpdateVersionListSuccessCallback?.Invoke(string.Empty, string.Empty);
        }

        public void VerifyResources(int verifyResourceLengthPerFrame,
            VerifyResourcesCompleteCallback verifyResourcesCompleteCallback)
        {
            verifyResourcesCompleteCallback?.Invoke(true);
        }

        public void CheckResources(bool ignoreOtherVariant,
            CheckResourcesCompleteCallback checkResourcesCompleteCallback)
        {
            checkResourcesCompleteCallback?.Invoke(0, 0, 0, 0L, 0L);
        }

        public void ApplyResources(string resourcePackPath,
            ApplyResourcesCompleteCallback applyResourcesCompleteCallback)
        {
            // 挂载 .pck 热更包，Godot 4 支持运行时加载
            bool ok = ProjectSettings.LoadResourcePack(resourcePackPath);
            applyResourcesCompleteCallback?.Invoke(resourcePackPath, ok);
        }

        public void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback) =>
            updateResourcesCompleteCallback?.Invoke(GetResourceGroup(), true);

        public void UpdateResources(string resourceGroupName,
            UpdateResourcesCompleteCallback updateResourcesCompleteCallback) =>
            updateResourcesCompleteCallback?.Invoke(GetResourceGroup(resourceGroupName), true);

        public void StopUpdateResources() { }

        public bool VerifyResourcePack(string resourcePackPath) =>
            FileAccess.FileExists(resourcePackPath);

        // ── 任务查询 ───────────────────────────────────────────────────────────

        public TaskInfo[] GetAllLoadAssetInfos()
        {
            var list = new List<TaskInfo>();
            GetAllLoadAssetInfos(list);
            return list.ToArray();
        }

        public void GetAllLoadAssetInfos(List<TaskInfo> results)
        {
            results.Clear();
            foreach (var t in m_WorkingAssetTasks)
                results.Add(new TaskInfo(t.Id, t.AssetName, t.Priority, t.UserData, TaskStatus.Doing, t.AssetName));
            foreach (var t in m_WaitingAssetTasks)
                results.Add(new TaskInfo(t.Id, t.AssetName, t.Priority, t.UserData, TaskStatus.Todo, t.AssetName));
        }

        // ── HasAsset ───────────────────────────────────────────────────────────

        public HasAssetResult HasAsset(string assetName)
        {
            string path = ResolvePath(assetName);
            if (string.IsNullOrEmpty(path))
                return HasAssetResult.NotExist;
            if (ResourceLoader.Exists(path))
                return HasAssetResult.AssetOnDisk;
            if (FileAccess.FileExists(path))
                return HasAssetResult.BinaryOnDisk;
            return HasAssetResult.NotExist;
        }

        // ── LoadAsset 系列重载 ─────────────────────────────────────────────────

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, null, 0, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, assetType, 0, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, null, priority, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            LoadAsset(assetName, null, 0, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks) =>
            LoadAsset(assetName, assetType, priority, loadAssetCallbacks, null);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            LoadAsset(assetName, assetType, 0, loadAssetCallbacks, userData);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData) =>
            LoadAsset(assetName, null, priority, loadAssetCallbacks, userData);

        /// <summary>核心加载方法：将任务入队，由 Update 驱动分配 Agent。</summary>
        public void LoadAsset(string assetName, Type assetType, int priority,
            LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(
                    assetName, LoadResourceStatus.NotExist, "Asset name is invalid.", userData);
                return;
            }

            string path = ResolvePath(assetName);
            if (string.IsNullOrEmpty(path))
            {
                loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(
                    assetName, LoadResourceStatus.NotExist, $"Cannot resolve path for: {assetName}", userData);
                return;
            }

            // 缓存命中：直接成功回调
            if (m_ResourceCache.TryGetValue(path, out var weakRef) &&
                weakRef.TryGetTarget(out var cached))
            {
                m_AssetCount++;
                loadAssetCallbacks.LoadAssetSuccessCallback(assetName, cached, 0f, userData);
                return;
            }

            var task = new AssetLoadTask
            {
                Id = GenerateTaskId(),
                AssetName = path,        // 存解析后的路径，供 Agent 使用
                AssetType = assetType,
                Priority = priority,
                Callbacks = loadAssetCallbacks,
                UserData = userData,
                StartTime = Time.GetTicksMsec() / 1000f,
                Agent = null
            };

            // 按优先级插入等待队列（简单线性排序，任务量通常较小）
            EnqueueTask(task);
        }

        public void UnloadAsset(object asset)
        {
            // Godot 资源引用计数管理，移除缓存中的弱引用即可
            string toRemove = null;
            foreach (var kv in m_ResourceCache)
            {
                if (kv.Value.TryGetTarget(out var target) && ReferenceEquals(target, asset))
                {
                    toRemove = kv.Key;
                    break;
                }
            }
            if (toRemove != null)
            {
                m_ResourceCache.Remove(toRemove);
                m_AssetCount = Math.Max(0, m_AssetCount - 1);
            }
        }

        // ── LoadScene 系列 ─────────────────────────────────────────────────────

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks) =>
            LoadScene(sceneAssetName, 0, loadSceneCallbacks, null);

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks) =>
            LoadScene(sceneAssetName, priority, loadSceneCallbacks, null);

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData) =>
            LoadScene(sceneAssetName, 0, loadSceneCallbacks, userData);

        /// <summary>
        /// 加载场景（additive）：将场景实例作为子节点挂到 SceneRoot，支持多场景并存。
        /// </summary>
        public void LoadScene(string sceneAssetName, int priority,
            LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                loadSceneCallbacks.LoadSceneFailureCallback?.Invoke(
                    sceneAssetName, LoadResourceStatus.NotExist, "Scene name is invalid.", userData);
                return;
            }

            string path = ResolvePath(sceneAssetName);

            // 发起后台加载
            var err = ResourceLoader.LoadThreadedRequest(path, "PackedScene", true);
            if (err != Error.Ok)
            {
                loadSceneCallbacks.LoadSceneFailureCallback?.Invoke(
                    sceneAssetName, LoadResourceStatus.NotExist,
                    $"LoadThreadedRequest failed: {err}", userData);
                return;
            }

            var agent = AcquireAgent();
            var task = new SceneLoadTask
            {
                SceneAssetName = path,
                Priority = priority,
                Callbacks = loadSceneCallbacks,
                UserData = userData,
                StartTime = Time.GetTicksMsec() / 1000f,
                Agent = agent
            };
            m_WorkingSceneTasks.Add(task);
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks) =>
            UnloadScene(sceneAssetName, unloadSceneCallbacks, null);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (TryGetAdditiveScene(sceneAssetName, out var node))
            {
                node.QueueFree();
                RemoveAdditiveScene(sceneAssetName);
                unloadSceneCallbacks.UnloadSceneSuccessCallback(sceneAssetName, userData);
            }
            else
            {
                unloadSceneCallbacks.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
            }
        }

        // ── 二进制资源 ─────────────────────────────────────────────────────────

        public string GetBinaryPath(string binaryAssetName) => ResolvePath(binaryAssetName);

        public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly,
            out bool storageInFileSystem, out string relativePath, out string fileName)
        {
            storageInFileSystem = false;
            fileName = null;
            string full = ResolvePath(binaryAssetName);
            if (string.IsNullOrEmpty(full))
            {
                storageInReadOnly = false;
                relativePath = null;
                return false;
            }
            storageInReadOnly = full.StartsWith("res://");
            relativePath = full;
            return true;
        }

        public int GetBinaryLength(string binaryAssetName)
        {
            string path = ResolvePath(binaryAssetName);
            if (string.IsNullOrEmpty(path)) return -1;
            using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            return f != null ? (int)f.GetLength() : -1;
        }

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks) =>
            LoadBinary(binaryAssetName, loadBinaryCallbacks, null);

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            string path = ResolvePath(binaryAssetName);
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    byte[] bytes = GodotResourceHelper.ReadAllBytes(path);
                    if (bytes == null)
                    {
                        loadBinaryCallbacks.LoadBinaryFailureCallback?.Invoke(
                            binaryAssetName, LoadResourceStatus.NotExist,
                            $"File not found: {path}", userData);
                        return;
                    }
                    loadBinaryCallbacks.LoadBinarySuccessCallback(binaryAssetName, bytes, 0f, userData);
                }
                catch (Exception ex)
                {
                    loadBinaryCallbacks.LoadBinaryFailureCallback?.Invoke(
                        binaryAssetName, LoadResourceStatus.AssetError, ex.Message, userData);
                }
            });
        }

        public byte[] LoadBinaryFromFileSystem(string binaryAssetName) =>
            GodotResourceHelper.ReadAllBytes(ResolvePath(binaryAssetName));

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer) =>
            LoadBinaryFromFileSystem(binaryAssetName, buffer, 0, buffer.Length);

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex) =>
            LoadBinaryFromFileSystem(binaryAssetName, buffer, startIndex, buffer.Length - startIndex);

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            byte[] bytes = LoadBinaryFromFileSystem(binaryAssetName);
            if (bytes == null) return 0;
            int count = Math.Min(length, bytes.Length);
            Array.Copy(bytes, 0, buffer, startIndex, count);
            return count;
        }

        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length) =>
            LoadBinarySegmentFromFileSystem(binaryAssetName, 0, length);

        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length)
        {
            byte[] bytes = LoadBinaryFromFileSystem(binaryAssetName);
            if (bytes == null) return null;
            int count = Math.Min(length, bytes.Length - offset);
            if (count <= 0) return Array.Empty<byte>();
            byte[] result = new byte[count];
            Array.Copy(bytes, offset, result, 0, count);
            return result;
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer) =>
            LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, 0, buffer.Length);

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int length) =>
            LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, 0, length);

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer) =>
            LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, 0, buffer.Length);

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int length) =>
            LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, 0, length);

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length) =>
            LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, startIndex, length);

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex, int length)
        {
            byte[] bytes = LoadBinaryFromFileSystem(binaryAssetName);
            if (bytes == null) return 0;
            int count = Math.Min(length, bytes.Length - offset);
            if (count <= 0) return 0;
            Array.Copy(bytes, offset, buffer, startIndex, count);
            return count;
        }

        // ── 资源组 ─────────────────────────────────────────────────────────────

        public bool HasResourceGroup(string resourceGroupName) =>
            m_ResourceGroups.ContainsKey(resourceGroupName ?? string.Empty);

        public IResourceGroup GetResourceGroup() => GetResourceGroup(string.Empty);

        public IResourceGroup GetResourceGroup(string resourceGroupName)
        {
            string key = resourceGroupName ?? string.Empty;
            if (!m_ResourceGroups.TryGetValue(key, out var group))
            {
                group = new GodotResourceGroup(key);
                m_ResourceGroups[key] = group;
            }
            return group;
        }

        public IResourceGroup[] GetAllResourceGroups()
        {
            var list = new List<IResourceGroup>();
            GetAllResourceGroups(list);
            return list.ToArray();
        }

        public void GetAllResourceGroups(List<IResourceGroup> results)
        {
            results.Clear();
            foreach (var g in m_ResourceGroups.Values)
                results.Add(g);
        }

        public IResourceGroupCollection GetResourceGroupCollection(params string[] resourceGroupNames)
        {
            var groups = new List<IResourceGroup>();
            foreach (var name in resourceGroupNames)
                groups.Add(GetResourceGroup(name));
            return new GodotResourceGroupCollection(groups);
        }

        public IResourceGroupCollection GetResourceGroupCollection(List<string> resourceGroupNames)
        {
            var groups = new List<IResourceGroup>();
            foreach (var name in resourceGroupNames)
                groups.Add(GetResourceGroup(name));
            return new GodotResourceGroupCollection(groups);
        }

        // ── 每帧驱动（由 ResourceComponent._Process 调用） ─────────────────────

        internal void Update(float elapseSeconds)
        {
            DispatchWaitingTasks();
            PollWorkingTasks(elapseSeconds);
        }

        // ── 私有实现 ───────────────────────────────────────────────────────────

        private void DispatchWaitingTasks()
        {
            while (m_WaitingAssetTasks.Count > 0 && LoadFreeAgentCount > 0)
            {
                var task = m_WaitingAssetTasks.Dequeue();
                task.Agent = AcquireAgent();
                m_WorkingAssetTasks.Add(task);

                // Agent 发起后台加载
                task.Agent.LoadResourceAgentHelperReadFileComplete += OnAgentReadFileComplete;
                task.Agent.LoadResourceAgentHelperError += OnAgentError;
                task.Agent.LoadResourceAgentHelperUpdate += (_, e) =>
                {
                    task.Callbacks.LoadAssetUpdateCallback?.Invoke(
                        task.AssetName, e.Progress, task.UserData);
                };
                task.Agent.ReadFile(task.AssetName);
            }
        }

        private void PollWorkingTasks(float elapseSeconds)
        {
            // 轮询 asset 任务
            for (int i = m_WorkingAssetTasks.Count - 1; i >= 0; i--)
                m_WorkingAssetTasks[i].Agent.PollThreadedLoad();

            // 轮询 scene 任务
            for (int i = m_WorkingSceneTasks.Count - 1; i >= 0; i--)
            {
                var task = m_WorkingSceneTasks[i];
                var arr = new Godot.Collections.Array();
                var status = ResourceLoader.LoadThreadedGetStatus(task.SceneAssetName, arr);
                float progress = arr.Count > 0 ? (float)arr[0] : 0f;

                if (status == ResourceLoader.ThreadLoadStatus.InProgress)
                {
                    task.Callbacks.LoadSceneUpdateCallback?.Invoke(task.SceneAssetName, progress, task.UserData);
                }
                else if (status == ResourceLoader.ThreadLoadStatus.Loaded)
                {
                    var packed = ResourceLoader.LoadThreadedGet(task.SceneAssetName) as PackedScene;
                    if (packed != null && m_SceneRoot != null)
                    {
                        var instance = packed.Instantiate();
                        m_SceneRoot.AddChild(instance);
                        s_AdditiveScenes[task.SceneAssetName] = instance;
                        float duration = Time.GetTicksMsec() / 1000f - task.StartTime;
                        task.Callbacks.LoadSceneSuccessCallback(task.SceneAssetName, duration, task.UserData);
                    }
                    else
                    {
                        task.Callbacks.LoadSceneFailureCallback?.Invoke(
                            task.SceneAssetName, LoadResourceStatus.AssetError,
                            "PackedScene is null or SceneRoot is not set.", task.UserData);
                    }
                    ReleaseAgent(task.Agent);
                    m_WorkingSceneTasks.RemoveAt(i);
                }
                else
                {
                    task.Callbacks.LoadSceneFailureCallback?.Invoke(
                        task.SceneAssetName, LoadResourceStatus.AssetError,
                        $"Scene load failed: {status}", task.UserData);
                    ReleaseAgent(task.Agent);
                    m_WorkingSceneTasks.RemoveAt(i);
                }
            }
        }

        private void OnAgentReadFileComplete(object sender, LoadResourceAgentHelperReadFileCompleteEventArgs e)
        {
            var agent = (GodotLoadResourceAgentHelper)sender;
            var task = FindTaskByAgent(agent);
            if (task == null) return;

            // ReadFile 完成 = Godot 资源已加载完毕，直接进入 LoadAsset 阶段
            agent.LoadResourceAgentHelperReadFileComplete -= OnAgentReadFileComplete;
            agent.LoadResourceAgentHelperError -= OnAgentError;

            var resource = e.Resource as GodotObject;
            if (resource != null)
                m_ResourceCache[task.AssetName] = new WeakReference<GodotObject>(resource);

            // 触发 LoadAsset（在 Godot 中此步骤为透传）
            agent.LoadAsset(e.Resource, task.AssetName, task.AssetType, false);

            float duration = Time.GetTicksMsec() / 1000f - task.StartTime;
            m_AssetCount++;
            task.Callbacks.LoadAssetSuccessCallback(task.AssetName, e.Resource, duration, task.UserData);

            ReleaseAgent(agent);
            m_WorkingAssetTasks.Remove(task);
        }

        private void OnAgentError(object sender, LoadResourceAgentHelperErrorEventArgs e)
        {
            var agent = (GodotLoadResourceAgentHelper)sender;
            var task = FindTaskByAgent(agent);
            if (task == null) return;

            agent.LoadResourceAgentHelperReadFileComplete -= OnAgentReadFileComplete;
            agent.LoadResourceAgentHelperError -= OnAgentError;

            task.Callbacks.LoadAssetFailureCallback?.Invoke(
                task.AssetName, e.Status, e.ErrorMessage, task.UserData);

            ReleaseAgent(agent);
            m_WorkingAssetTasks.Remove(task);
        }

        private AssetLoadTask FindTaskByAgent(GodotLoadResourceAgentHelper agent)
        {
            foreach (var t in m_WorkingAssetTasks)
                if (ReferenceEquals(t.Agent, agent)) return t;
            return null;
        }

        private GodotLoadResourceAgentHelper AcquireAgent()
        {
            // 优先找空闲的 Agent；全忙时动态新增
            foreach (var a in m_Agents)
            {
                bool inUse = false;
                foreach (var t in m_WorkingAssetTasks)
                    if (ReferenceEquals(t.Agent, a)) { inUse = true; break; }
                if (!inUse)
                    foreach (var t in m_WorkingSceneTasks)
                        if (ReferenceEquals(t.Agent, a)) { inUse = true; break; }
                if (!inUse) return a;
            }
            var newAgent = new GodotLoadResourceAgentHelper();
            m_Agents.Add(newAgent);
            return newAgent;
        }

        private void ReleaseAgent(GodotLoadResourceAgentHelper agent) => agent.Reset();

        private void EnqueueTask(AssetLoadTask task)
        {
            // 转为 List 以便按优先级排序后重建 Queue
            var list = new List<AssetLoadTask>(m_WaitingAssetTasks) { task };
            list.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            m_WaitingAssetTasks.Clear();
            foreach (var t in list) m_WaitingAssetTasks.Enqueue(t);
        }

        private static int s_TaskIdSeed = 0;
        private static int GenerateTaskId() => ++s_TaskIdSeed;

        /// <summary>
        /// 解析资源路径：UpdatableMode 先查 user://，找不到退回 res://。
        /// </summary>
        private string ResolvePath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 已经是完整路径
            if (assetName.StartsWith("res://") || assetName.StartsWith("user://"))
                return assetName;

            if (m_ResourceMode == ResourceMode.Updatable ||
                m_ResourceMode == ResourceMode.UpdatableWhilePlaying)
            {
                string rwPath = m_ReadWritePath.TrimEnd('/') + "/" + assetName;
                if (FileAccess.FileExists(rwPath) || ResourceLoader.Exists(rwPath))
                    return rwPath;
            }

            return m_ReadOnlyPath.TrimEnd('/') + "/" + assetName;
        }
    }
}
