//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Resource;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 资源组件：封装 GodotResourceManager，作为场景树 Node 存在。
    ///
    /// 使用步骤：
    ///   1. 在场景 Node 树中添加 ResourceComponent（顺序在 BaseComponent 之后）。
    ///   2. 在 Inspector 中配置 ResourceMode、AgentCount 等。
    ///   3. 在 Procedure 或其他组件中通过 GameEntry.GetComponent&lt;ResourceComponent&gt;()
    ///      访问此组件，或直接通过 GameEntry.GetModule&lt;IResourceManager&gt;() 获取接口。
    ///
    /// 场景树示例：
    ///   GameFramework (Node)
    ///   ├── BaseComponent
    ///   ├── EventComponent
    ///   ├── ResourceComponent   ← 此节点
    ///   └── ProcedureComponent
    ///
    /// 加载示例：
    ///   var res = GameEntry.GetComponent&lt;ResourceComponent&gt;();
    ///   res.LoadAsset("res://Prefabs/Hero.tscn", new LoadAssetCallbacks(
    ///       (name, asset, duration, userData) => {
    ///           var packed = asset as PackedScene;
    ///           AddChild(packed.Instantiate());
    ///       }
    ///   ));
    /// </summary>
    public sealed partial class ResourceComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>资源模式。</summary>
        [Export]
        public ResourceMode ResourceMode = ResourceMode.Package;

        /// <summary>加载代理数量（并发异步加载数）。</summary>
        [Export(PropertyHint.Range, "1,16")]
        public int AgentCount = 3;

        /// <summary>
        /// 资源只读区路径（PackageMode 下即 res://）。
        /// </summary>
        [Export]
        public string ReadOnlyPath = "res://";

        /// <summary>
        /// 资源读写区路径（UpdatableMode 下为 user:// 或指定目录）。
        /// </summary>
        [Export]
        public string ReadWritePath = "user://";

        /// <summary>
        /// UpdatableMode 热更下载地址前缀，例如 "https://cdn.example.com/res/"。
        /// </summary>
        [Export]
        public string UpdatePrefixUri = string.Empty;

        /// <summary>热更失败最大重试次数。</summary>
        [Export(PropertyHint.Range, "0,10")]
        public int UpdateRetryCount = 3;

        /// <summary>
        /// additive 场景挂载父节点（留空则挂到 ResourceComponent 自身）。
        /// </summary>
        [Export]
        public NodePath SceneRootPath = new NodePath();

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private GodotResourceManager m_ResourceManager = null;

        /// <summary>获取当前活跃的资源管理器实例（供其他组件内部使用）。</summary>
        internal static IResourceManager Instance { get; private set; }

        // ── 属性代理 ───────────────────────────────────────────────────────────

        public int AssetCount => m_ResourceManager.AssetCount;
        public int ResourceCount => m_ResourceManager.ResourceCount;
        public int LoadTotalAgentCount => m_ResourceManager.LoadTotalAgentCount;
        public int LoadFreeAgentCount => m_ResourceManager.LoadFreeAgentCount;
        public int LoadWorkingAgentCount => m_ResourceManager.LoadWorkingAgentCount;
        public int LoadWaitingTaskCount => m_ResourceManager.LoadWaitingTaskCount;

        // ── 事件透传 ───────────────────────────────────────────────────────────

        public event EventHandler<ResourceVerifyStartEventArgs> ResourceVerifyStart
        {
            add => m_ResourceManager.ResourceVerifyStart += value;
            remove => m_ResourceManager.ResourceVerifyStart -= value;
        }

        public event EventHandler<ResourceVerifySuccessEventArgs> ResourceVerifySuccess
        {
            add => m_ResourceManager.ResourceVerifySuccess += value;
            remove => m_ResourceManager.ResourceVerifySuccess -= value;
        }

        public event EventHandler<ResourceVerifyFailureEventArgs> ResourceVerifyFailure
        {
            add => m_ResourceManager.ResourceVerifyFailure += value;
            remove => m_ResourceManager.ResourceVerifyFailure -= value;
        }

        public event EventHandler<ResourceApplyStartEventArgs> ResourceApplyStart
        {
            add => m_ResourceManager.ResourceApplyStart += value;
            remove => m_ResourceManager.ResourceApplyStart -= value;
        }

        public event EventHandler<ResourceApplySuccessEventArgs> ResourceApplySuccess
        {
            add => m_ResourceManager.ResourceApplySuccess += value;
            remove => m_ResourceManager.ResourceApplySuccess -= value;
        }

        public event EventHandler<ResourceApplyFailureEventArgs> ResourceApplyFailure
        {
            add => m_ResourceManager.ResourceApplyFailure += value;
            remove => m_ResourceManager.ResourceApplyFailure -= value;
        }

        public event EventHandler<ResourceUpdateStartEventArgs> ResourceUpdateStart
        {
            add => m_ResourceManager.ResourceUpdateStart += value;
            remove => m_ResourceManager.ResourceUpdateStart -= value;
        }

        public event EventHandler<ResourceUpdateChangedEventArgs> ResourceUpdateChanged
        {
            add => m_ResourceManager.ResourceUpdateChanged += value;
            remove => m_ResourceManager.ResourceUpdateChanged -= value;
        }

        public event EventHandler<ResourceUpdateSuccessEventArgs> ResourceUpdateSuccess
        {
            add => m_ResourceManager.ResourceUpdateSuccess += value;
            remove => m_ResourceManager.ResourceUpdateSuccess -= value;
        }

        public event EventHandler<ResourceUpdateFailureEventArgs> ResourceUpdateFailure
        {
            add => m_ResourceManager.ResourceUpdateFailure += value;
            remove => m_ResourceManager.ResourceUpdateFailure -= value;
        }

        public event EventHandler<ResourceUpdateAllCompleteEventArgs> ResourceUpdateAllComplete
        {
            add => m_ResourceManager.ResourceUpdateAllComplete += value;
            remove => m_ResourceManager.ResourceUpdateAllComplete -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            // 从框架模块系统获取（GodotResourceManager 需要预先注册）
            // 由于 IResourceManager 的实现是 GodotResourceManager（非原版 ResourceManager），
            // 这里直接 new 并手动注册到框架模块系统。
            m_ResourceManager = new GodotResourceManager();
            Instance = m_ResourceManager;

            // 配置路径与模式
            m_ResourceManager.SetReadOnlyPath(ReadOnlyPath);
            m_ResourceManager.SetReadWritePath(ReadWritePath);
            m_ResourceManager.SetResourceMode(ResourceMode);
            m_ResourceManager.UpdatePrefixUri = UpdatePrefixUri;
            m_ResourceManager.UpdateRetryCount = UpdateRetryCount;

            // 配置 additive 场景挂载节点
            Node sceneRoot = SceneRootPath.IsEmpty ? this : GetNode(SceneRootPath);
            m_ResourceManager.SetSceneRoot(sceneRoot ?? this);

            // 注册辅助器
            m_ResourceManager.SetResourceHelper(new GodotResourceHelper());

            // 创建并注册加载 Agent
            for (int i = 0; i < Math.Max(1, AgentCount); i++)
                m_ResourceManager.AddLoadResourceAgentHelper(new GodotLoadResourceAgentHelper());

            // PackageMode 自动初始化
            if (ResourceMode == ResourceMode.Package)
                m_ResourceManager.InitResources(null);
        }

        /// <summary>每帧驱动所有 Agent 的异步轮询。</summary>
        public override void _Process(double delta)
        {
            m_ResourceManager?.Update((float)delta);
        }

        // ── 公开 API（透传 IResourceManager，方便直接引用 ResourceComponent 的场景） ──

        public HasAssetResult HasAsset(string assetName) => m_ResourceManager.HasAsset(assetName);

        public void LoadAsset(string assetName, LoadAssetCallbacks callbacks) =>
            m_ResourceManager.LoadAsset(assetName, callbacks);

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks callbacks) =>
            m_ResourceManager.LoadAsset(assetName, assetType, callbacks);

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks callbacks) =>
            m_ResourceManager.LoadAsset(assetName, priority, callbacks);

        public void LoadAsset(string assetName, LoadAssetCallbacks callbacks, object userData) =>
            m_ResourceManager.LoadAsset(assetName, callbacks, userData);

        public void LoadAsset(string assetName, Type assetType, int priority,
            LoadAssetCallbacks callbacks, object userData) =>
            m_ResourceManager.LoadAsset(assetName, assetType, priority, callbacks, userData);

        public void UnloadAsset(object asset) => m_ResourceManager.UnloadAsset(asset);

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks callbacks) =>
            m_ResourceManager.LoadScene(sceneAssetName, callbacks);

        public void LoadScene(string sceneAssetName, int priority,
            LoadSceneCallbacks callbacks, object userData) =>
            m_ResourceManager.LoadScene(sceneAssetName, priority, callbacks, userData);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks callbacks) =>
            m_ResourceManager.UnloadScene(sceneAssetName, callbacks);

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks callbacks, object userData) =>
            m_ResourceManager.UnloadScene(sceneAssetName, callbacks, userData);

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks callbacks) =>
            m_ResourceManager.LoadBinary(binaryAssetName, callbacks);

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks callbacks, object userData) =>
            m_ResourceManager.LoadBinary(binaryAssetName, callbacks, userData);

        public byte[] LoadBinaryFromFileSystem(string binaryAssetName) =>
            m_ResourceManager.LoadBinaryFromFileSystem(binaryAssetName);

        public void InitResources(InitResourcesCompleteCallback callback) =>
            m_ResourceManager.InitResources(callback);

        public void ApplyResources(string packPath, ApplyResourcesCompleteCallback callback) =>
            m_ResourceManager.ApplyResources(packPath, callback);

        public void UpdateResources(UpdateResourcesCompleteCallback callback) =>
            m_ResourceManager.UpdateResources(callback);

        public void StopUpdateResources() => m_ResourceManager.StopUpdateResources();

        public TaskInfo[] GetAllLoadAssetInfos() => m_ResourceManager.GetAllLoadAssetInfos();

        public bool HasResourceGroup(string name) => m_ResourceManager.HasResourceGroup(name);

        public IResourceGroup GetResourceGroup(string name) => m_ResourceManager.GetResourceGroup(name);

        public IResourceGroup[] GetAllResourceGroups() => m_ResourceManager.GetAllResourceGroups();
    }
}
