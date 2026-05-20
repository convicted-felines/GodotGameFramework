using GameFramework;
using GameFramework.WebRequest;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// Web 请求组件。封装 IWebRequestManager，提供 HTTP GET/POST 能力。
    ///
    /// 使用示例：
    ///   var web = GameEntry.GetComponent＜WebRequestComponent＞();
    ///   web.AddWebRequest("https://api.example.com/user");
    ///   web.WebRequestSuccess += (s, e) => {
    ///       string json = System.Text.Encoding.UTF8.GetString(e.GetWebResponseBytes());
    ///   };
    /// </summary>
    public sealed partial class WebRequestComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>并发请求代理数。</summary>
        [Export] public int WebRequestAgentCount = 4;

        /// <summary>超时时间（秒），0 表示不超时。</summary>
        [Export] public float Timeout = 30f;

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private IWebRequestManager m_WebRequestManager = null;

        // ── 属性 ───────────────────────────────────────────────────────────────

        public int TotalAgentCount => m_WebRequestManager.TotalAgentCount;
        public int FreeAgentCount => m_WebRequestManager.FreeAgentCount;
        public int WorkingAgentCount => m_WebRequestManager.WorkingAgentCount;
        public int WaitingTaskCount => m_WebRequestManager.WaitingTaskCount;

        public float WebRequestTimeout
        {
            get => m_WebRequestManager.Timeout;
            set => m_WebRequestManager.Timeout = value;
        }

        // ── 事件透传 ───────────────────────────────────────────────────────────

        public event EventHandler<WebRequestStartEventArgs> WebRequestStart
        {
            add => m_WebRequestManager.WebRequestStart += value;
            remove => m_WebRequestManager.WebRequestStart -= value;
        }

        public event EventHandler<WebRequestSuccessEventArgs> WebRequestSuccess
        {
            add => m_WebRequestManager.WebRequestSuccess += value;
            remove => m_WebRequestManager.WebRequestSuccess -= value;
        }

        public event EventHandler<WebRequestFailureEventArgs> WebRequestFailure
        {
            add => m_WebRequestManager.WebRequestFailure += value;
            remove => m_WebRequestManager.WebRequestFailure -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_WebRequestManager = GameFrameworkEntry.GetModule<IWebRequestManager>();
            if (m_WebRequestManager == null)
            {
                GameFrameworkLog.Fatal("Web request manager is invalid.");
                return;
            }

            m_WebRequestManager.Timeout = Timeout;

            for (int i = 0; i < WebRequestAgentCount; i++)
                m_WebRequestManager.AddWebRequestAgentHelper(new HttpWebRequestAgentHelper());
        }

        // ── GET 请求 ───────────────────────────────────────────────────────────

        public int AddWebRequest(string webRequestUri) =>
            m_WebRequestManager.AddWebRequest(webRequestUri);

        public int AddWebRequest(string webRequestUri, int priority) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, priority);

        public int AddWebRequest(string webRequestUri, object userData) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, userData);

        public int AddWebRequest(string webRequestUri, int priority, object userData) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, priority, userData);

        // ── POST 请求 ──────────────────────────────────────────────────────────

        public int AddWebRequest(string webRequestUri, byte[] postData) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, postData);

        public int AddWebRequest(string webRequestUri, byte[] postData, int priority) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, postData, priority);

        public int AddWebRequest(string webRequestUri, byte[] postData, object userData) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, postData, userData);

        public int AddWebRequest(string webRequestUri, byte[] postData, int priority, object userData) =>
            m_WebRequestManager.AddWebRequest(webRequestUri, postData, priority, userData);

        // ── 移除 ───────────────────────────────────────────────────────────────

        public bool RemoveWebRequest(int serialId) => m_WebRequestManager.RemoveWebRequest(serialId);

        public int RemoveWebRequests(string tag) => m_WebRequestManager.RemoveWebRequests(tag);

        public int RemoveAllWebRequests() => m_WebRequestManager.RemoveAllWebRequests();

        // ── 查询 ───────────────────────────────────────────────────────────────

        public TaskInfo GetWebRequestInfo(int serialId) => m_WebRequestManager.GetWebRequestInfo(serialId);

        public TaskInfo[] GetAllWebRequestInfos() => m_WebRequestManager.GetAllWebRequestInfos();

        public void GetAllWebRequestInfos(List<TaskInfo> results) => m_WebRequestManager.GetAllWebRequestInfos(results);
    }
}
