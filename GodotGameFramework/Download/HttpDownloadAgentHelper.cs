using GameFramework.Download;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GodotGameFramework
{
    /// <summary>
    /// 下载代理辅助器。基于 HttpClient 实现，支持断点续传（Range 请求）。
    /// 每个实例对应 DownloadManager 中的一个下载代理槽位。
    /// </summary>
    public sealed class HttpDownloadAgentHelper : IDownloadAgentHelper
    {
        private static readonly HttpClient s_HttpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        private CancellationTokenSource m_Cts;

        // ── IDownloadAgentHelper 事件 ──────────────────────────────────────────

        public event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes;
        public event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength;
        public event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete;
        public event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError;

        // ── IDownloadAgentHelper 方法 ──────────────────────────────────────────

        public void Download(string downloadUri, object userData)
            => StartDownload(downloadUri, -1L, -1L, userData);

        public void Download(string downloadUri, long fromPosition, object userData)
            => StartDownload(downloadUri, fromPosition, -1L, userData);

        public void Download(string downloadUri, long fromPosition, long toPosition, object userData)
            => StartDownload(downloadUri, fromPosition, toPosition, userData);

        public void Reset()
        {
            m_Cts?.Cancel();
            m_Cts?.Dispose();
            m_Cts = null;
        }

        // ── 私有下载逻辑 ───────────────────────────────────────────────────────

        private void StartDownload(string uri, long from, long to, object userData)
        {
            Reset();
            m_Cts = new CancellationTokenSource();
            var token = m_Cts.Token;

            Task.Run(async () =>
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, uri);

                    if (from >= 0 && to > from)
                        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(from, to);
                    else if (from >= 0)
                        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(from, null);

                    using var response = await s_HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(token);
                    var buffer = new byte[65536];
                    long totalRead = 0L;
                    int read;

                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        totalRead += read;

                        // 通知字节块（框架用于写入磁盘）
                        DownloadAgentHelperUpdateBytes?.Invoke(this,
                            DownloadAgentHelperUpdateBytesEventArgs.Create(buffer, 0, read));

                        // 通知长度（框架用于统计下载速度）
                        DownloadAgentHelperUpdateLength?.Invoke(this,
                            DownloadAgentHelperUpdateLengthEventArgs.Create(read));
                    }

                    DownloadAgentHelperComplete?.Invoke(this,
                        DownloadAgentHelperCompleteEventArgs.Create(totalRead));
                }
                catch (OperationCanceledException)
                {
                    // 主动取消，不报错
                }
                catch (Exception ex)
                {
                    DownloadAgentHelperError?.Invoke(this,
                        DownloadAgentHelperErrorEventArgs.Create(false, ex.Message));
                }
            }, token);
        }
    }
}
