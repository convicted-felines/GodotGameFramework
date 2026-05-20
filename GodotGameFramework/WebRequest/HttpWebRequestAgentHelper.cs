using GameFramework.WebRequest;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GodotGameFramework
{
    /// <summary>
    /// Web 请求代理辅助器。基于 HttpClient 实现 GET / POST 请求。
    /// 每个实例对应 WebRequestManager 中的一个请求代理槽位。
    /// </summary>
    public sealed class HttpWebRequestAgentHelper : IWebRequestAgentHelper
    {
        private static readonly HttpClient s_HttpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        private CancellationTokenSource m_Cts;

        // ── IWebRequestAgentHelper 事件 ────────────────────────────────────────

        public event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete;
        public event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError;

        // ── IWebRequestAgentHelper 方法 ────────────────────────────────────────

        public void Request(string webRequestUri, object userData)
            => StartRequest(webRequestUri, null);

        public void Request(string webRequestUri, byte[] postData, object userData)
            => StartRequest(webRequestUri, postData);

        public void Reset()
        {
            m_Cts?.Cancel();
            m_Cts?.Dispose();
            m_Cts = null;
        }

        // ── 私有请求逻辑 ───────────────────────────────────────────────────────

        private void StartRequest(string uri, byte[] postData)
        {
            Reset();
            m_Cts = new CancellationTokenSource();
            var token = m_Cts.Token;

            Task.Run(async () =>
            {
                try
                {
                    HttpResponseMessage response;
                    if (postData != null)
                    {
                        using var content = new ByteArrayContent(postData);
                        response = await s_HttpClient.PostAsync(uri, content, token);
                    }
                    else
                    {
                        response = await s_HttpClient.GetAsync(uri, token);
                    }

                    response.EnsureSuccessStatusCode();
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync(token);

                    WebRequestAgentHelperComplete?.Invoke(this,
                        WebRequestAgentHelperCompleteEventArgs.Create(bytes));
                }
                catch (OperationCanceledException)
                {
                    // 主动取消，不报错
                }
                catch (Exception ex)
                {
                    WebRequestAgentHelperError?.Invoke(this,
                        WebRequestAgentHelperErrorEventArgs.Create(ex.Message));
                }
            }, token);
        }
    }
}
