//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Download;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 下载代理辅助器基类。继承此类实现自定义下载代理，在 DownloadComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class DownloadAgentHelperBase : IDownloadAgentHelper
    {
        public abstract event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes;
        public abstract event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength;
        public abstract event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete;
        public abstract event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError;

        public abstract void Download(string downloadUri, object userData);
        public abstract void Download(string downloadUri, long fromPosition, object userData);
        public abstract void Download(string downloadUri, long fromPosition, long toPosition, object userData);
        public abstract void Reset();
    }
}
