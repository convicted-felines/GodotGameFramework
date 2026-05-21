//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;

namespace GodotGameFramework.Utility
{
    /// <summary>
    /// 日志辅助器基类。继承此类实现自定义日志输出，在 BaseComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class LogHelperBase : GameFrameworkLog.ILogHelper
    {
        public abstract void Log(GameFrameworkLogLevel level, object message);
    }
}
