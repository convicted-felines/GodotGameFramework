//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.WebRequest;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// Web 请求代理辅助器基类。继承此类实现自定义 Web 请求代理，在 WebRequestComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class WebRequestAgentHelperBase : IWebRequestAgentHelper
    {
        public abstract event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete;
        public abstract event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError;

        public abstract void Request(string webRequestUri, object userData);
        public abstract void Request(string webRequestUri, byte[] postData, object userData);
        public abstract void Reset();
    }
}
