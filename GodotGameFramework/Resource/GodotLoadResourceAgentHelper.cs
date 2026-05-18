//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.FileSystem;
using GameFramework.Resource;
using Godot;
using System;
using System.Threading.Tasks;

namespace GodotGameFramework
{
    /// <summary>
    /// 加载资源代理辅助器：驱动单条资源的异步加载流水线。
    ///
    /// Godot 加载流程（对应原版 Agent 流水线）：
    ///   ReadFile(path)
    ///     → Godot LoadThreadedRequest 发起后台加载
    ///     → 轮询 LoadThreadedGetStatus（由 ResourceComponent._Process 驱动）
    ///     → 完成时触发 LoadResourceAgentHelperReadFileComplete（Resource = GodotResource）
    ///
    ///   ReadBytes(path)  / ParseBytes(bytes)
    ///     → 用于二进制资源，直接用 FileAccess 读字节
    ///     → 触发 LoadResourceAgentHelperReadBytesComplete / ParseBytesComplete
    ///
    ///   LoadAsset(resource, assetName, assetType, isScene)
    ///     → 在 Godot 中 resource 即是最终资源，直接触发 LoadComplete
    /// </summary>
    public sealed class GodotLoadResourceAgentHelper : ILoadResourceAgentHelper
    {
        // ── 状态机 ─────────────────────────────────────────────────────────────

        private enum AgentState { Idle, LoadingThreaded, LoadingBytes }

        private AgentState m_State = AgentState.Idle;
        private string m_LoadingPath = null;

        // ── 事件 ───────────────────────────────────────────────────────────────

        public event EventHandler<LoadResourceAgentHelperUpdateEventArgs> LoadResourceAgentHelperUpdate;
        public event EventHandler<LoadResourceAgentHelperReadFileCompleteEventArgs> LoadResourceAgentHelperReadFileComplete;
        public event EventHandler<LoadResourceAgentHelperReadBytesCompleteEventArgs> LoadResourceAgentHelperReadBytesComplete;
        public event EventHandler<LoadResourceAgentHelperParseBytesCompleteEventArgs> LoadResourceAgentHelperParseBytesComplete;
        public event EventHandler<LoadResourceAgentHelperLoadCompleteEventArgs> LoadResourceAgentHelperLoadComplete;
        public event EventHandler<LoadResourceAgentHelperErrorEventArgs> LoadResourceAgentHelperError;

        // ── ILoadResourceAgentHelper ───────────────────────────────────────────

        /// <summary>
        /// 发起 Godot 后台线程加载（对应 AB 体系的 ReadFile 阶段）。
        /// Godot 的 ResourceLoader 本身管理依赖，无需手动处理。
        /// </summary>
        public void ReadFile(string fullPath)
        {
            if (m_State != AgentState.Idle)
            {
                FireError(LoadResourceStatus.AssetError, "Agent is not idle.");
                return;
            }

            var err = ResourceLoader.LoadThreadedRequest(fullPath, "", true);
            if (err != Error.Ok)
            {
                FireError(LoadResourceStatus.NotExist, $"LoadThreadedRequest failed: {err} — {fullPath}");
                return;
            }

            m_LoadingPath = fullPath;
            m_State = AgentState.LoadingThreaded;
        }

        /// <summary>
        /// FileSystem 版本 ReadFile：读出字节再交 ParseBytes 处理。
        /// Godot 的 FileSystem 虚文件系统不适用，退化为按名字读磁盘文件。
        /// </summary>
        public void ReadFile(IFileSystem fileSystem, string name)
        {
            // 框架 FileSystem 在 Godot 侧未实现，按磁盘路径降级处理
            ReadFile(name);
        }

        /// <summary>
        /// 异步读取原始字节（供 DataTable 等二进制资源使用）。
        /// </summary>
        public void ReadBytes(string fullPath)
        {
            if (m_State != AgentState.Idle)
            {
                FireError(LoadResourceStatus.AssetError, "Agent is not idle.");
                return;
            }

            m_State = AgentState.LoadingBytes;
            Task.Run(() =>
            {
                try
                {
                    byte[] bytes = GodotResourceHelper.ReadAllBytes(fullPath);
                    if (bytes == null)
                    {
                        FireError(LoadResourceStatus.NotExist, $"File not found: {fullPath}");
                        return;
                    }
                    var args = LoadResourceAgentHelperReadBytesCompleteEventArgs.Create(bytes);
                    LoadResourceAgentHelperReadBytesComplete?.Invoke(this, args);
                    ReferencePool.Release(args);
                    m_State = AgentState.Idle;
                }
                catch (Exception ex)
                {
                    FireError(LoadResourceStatus.AssetError, ex.Message);
                }
            });
        }

        public void ReadBytes(IFileSystem fileSystem, string name)
        {
            ReadBytes(name);
        }

        /// <summary>
        /// 将字节流解析为 Godot Resource（目前仅透传字节，实际资源已由 ReadFile 完成加载）。
        /// </summary>
        public void ParseBytes(byte[] bytes)
        {
            // Godot 侧的二进制资源（.bytes）通过 ReadBytes 直接交给上层使用，
            // ParseBytes 在 AB 体系里是把 AB 字节解码为 AssetBundle 对象，
            // Godot 不需要这一步，直接把 bytes 当作 resource 向上传递。
            var args = LoadResourceAgentHelperParseBytesCompleteEventArgs.Create(bytes);
            LoadResourceAgentHelperParseBytesComplete?.Invoke(this, args);
            ReferencePool.Release(args);
        }

        /// <summary>
        /// 加载资产（在 Godot 侧 resource 本身就是最终资源，直接触发完成）。
        /// </summary>
        public void LoadAsset(object resource, string assetName, Type assetType, bool isScene)
        {
            // resource 即 Godot.Resource（PackedScene / Texture2D / AudioStream …）
            var args = LoadResourceAgentHelperLoadCompleteEventArgs.Create(resource);
            LoadResourceAgentHelperLoadComplete?.Invoke(this, args);
            ReferencePool.Release(args);
        }

        /// <summary>
        /// 重置 Agent 状态（框架在每次任务结束后调用）。
        /// </summary>
        public void Reset()
        {
            m_State = AgentState.Idle;
            m_LoadingPath = null;
        }

        // ── 轮询（由 ResourceComponent._Process 驱动） ─────────────────────────

        /// <summary>
        /// 每帧查询 Godot 后台加载进度，完成时触发 ReadFileComplete。
        /// ResourceComponent 负责对所有 Helper 实例调用此方法。
        /// </summary>
        internal void PollThreadedLoad()
        {
            if (m_State != AgentState.LoadingThreaded)
                return;

            Godot.Collections.Array progressArr = new Godot.Collections.Array();
            var status = ResourceLoader.LoadThreadedGetStatus(m_LoadingPath, progressArr);
            float progress = progressArr.Count > 0 ? (float)progressArr[0] : 0f;

            switch (status)
            {
                case ResourceLoader.ThreadLoadStatus.InProgress:
                {
                    var upd = LoadResourceAgentHelperUpdateEventArgs.Create(LoadResourceProgress.LoadResource, progress);
                    LoadResourceAgentHelperUpdate?.Invoke(this, upd);
                    ReferencePool.Release(upd);
                    break;
                }

                case ResourceLoader.ThreadLoadStatus.Loaded:
                {
                    var resource = ResourceLoader.LoadThreadedGet(m_LoadingPath);
                    m_State = AgentState.Idle;
                    var args = LoadResourceAgentHelperReadFileCompleteEventArgs.Create(resource);
                    LoadResourceAgentHelperReadFileComplete?.Invoke(this, args);
                    ReferencePool.Release(args);
                    break;
                }

                case ResourceLoader.ThreadLoadStatus.Failed:
                    FireError(LoadResourceStatus.AssetError, $"Threaded load failed: {m_LoadingPath}");
                    break;

                case ResourceLoader.ThreadLoadStatus.InvalidResource:
                    FireError(LoadResourceStatus.NotExist, $"Invalid resource: {m_LoadingPath}");
                    break;
            }
        }

        // ── 私有辅助 ───────────────────────────────────────────────────────────

        private void FireError(LoadResourceStatus status, string message)
        {
            m_State = AgentState.Idle;
            m_LoadingPath = null;
            var args = LoadResourceAgentHelperErrorEventArgs.Create(status, message);
            LoadResourceAgentHelperError?.Invoke(this, args);
            ReferencePool.Release(args);
        }
    }
}
