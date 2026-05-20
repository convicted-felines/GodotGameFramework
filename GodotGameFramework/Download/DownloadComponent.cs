using GameFramework;
using GameFramework.Download;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 下载组件。封装 IDownloadManager，支持断点续传、并发下载和速度统计。
    ///
    /// 使用示例：
    ///   var dl = GameEntry.GetComponent＜DownloadComponent＞();
    ///   dl.AddDownload("user://patch_v2.pck", "https://cdn.example.com/patch_v2.pck");
    ///   dl.DownloadSuccess += (s, e) => GD.Print($"Downloaded: {e.DownloadPath}");
    /// </summary>
    public sealed partial class DownloadComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>并发下载代理数。</summary>
        [Export] public int DownloadAgentCount = 3;

        /// <summary>超时时间（秒），0 表示不超时。</summary>
        [Export] public float Timeout = 30f;

        /// <summary>写磁盘缓冲大小（字节），达到此大小时写入文件。</summary>
        [Export] public int FlushSize = 1024 * 1024;

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private IDownloadManager m_DownloadManager = null;

        // ── 属性 ───────────────────────────────────────────────────────────────

        public bool Paused
        {
            get => m_DownloadManager.Paused;
            set => m_DownloadManager.Paused = value;
        }

        public int TotalAgentCount => m_DownloadManager.TotalAgentCount;
        public int FreeAgentCount => m_DownloadManager.FreeAgentCount;
        public int WorkingAgentCount => m_DownloadManager.WorkingAgentCount;
        public int WaitingTaskCount => m_DownloadManager.WaitingTaskCount;
        public float CurrentSpeed => m_DownloadManager.CurrentSpeed;

        // ── 事件透传 ───────────────────────────────────────────────────────────

        public event EventHandler<DownloadStartEventArgs> DownloadStart
        {
            add => m_DownloadManager.DownloadStart += value;
            remove => m_DownloadManager.DownloadStart -= value;
        }

        public event EventHandler<DownloadUpdateEventArgs> DownloadUpdate
        {
            add => m_DownloadManager.DownloadUpdate += value;
            remove => m_DownloadManager.DownloadUpdate -= value;
        }

        public event EventHandler<DownloadSuccessEventArgs> DownloadSuccess
        {
            add => m_DownloadManager.DownloadSuccess += value;
            remove => m_DownloadManager.DownloadSuccess -= value;
        }

        public event EventHandler<DownloadFailureEventArgs> DownloadFailure
        {
            add => m_DownloadManager.DownloadFailure += value;
            remove => m_DownloadManager.DownloadFailure -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_DownloadManager = GameFrameworkEntry.GetModule<IDownloadManager>();
            if (m_DownloadManager == null)
            {
                GameFrameworkLog.Fatal("Download manager is invalid.");
                return;
            }

            m_DownloadManager.Timeout = Timeout;
            m_DownloadManager.FlushSize = FlushSize;

            for (int i = 0; i < DownloadAgentCount; i++)
                m_DownloadManager.AddDownloadAgentHelper(new HttpDownloadAgentHelper());
        }

        // ── 添加下载任务 ───────────────────────────────────────────────────────

        public int AddDownload(string downloadPath, string downloadUri) =>
            m_DownloadManager.AddDownload(downloadPath, downloadUri);

        public int AddDownload(string downloadPath, string downloadUri, int priority) =>
            m_DownloadManager.AddDownload(downloadPath, downloadUri, priority);

        public int AddDownload(string downloadPath, string downloadUri, object userData) =>
            m_DownloadManager.AddDownload(downloadPath, downloadUri, userData);

        public int AddDownload(string downloadPath, string downloadUri, int priority, object userData) =>
            m_DownloadManager.AddDownload(downloadPath, downloadUri, priority, userData);

        // ── 移除下载任务 ───────────────────────────────────────────────────────

        public bool RemoveDownload(int serialId) => m_DownloadManager.RemoveDownload(serialId);

        public int RemoveDownloads(string tag) => m_DownloadManager.RemoveDownloads(tag);

        public int RemoveAllDownloads() => m_DownloadManager.RemoveAllDownloads();

        // ── 查询 ───────────────────────────────────────────────────────────────

        public TaskInfo GetDownloadInfo(int serialId) => m_DownloadManager.GetDownloadInfo(serialId);

        public TaskInfo[] GetAllDownloadInfos() => m_DownloadManager.GetAllDownloadInfos();

        public void GetAllDownloadInfos(List<TaskInfo> results) => m_DownloadManager.GetAllDownloadInfos(results);
    }
}
