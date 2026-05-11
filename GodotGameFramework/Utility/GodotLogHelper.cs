//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;
using Godot;

namespace GodotGameFramework.Utility
{
    /// <summary>
    /// 使用 GD.Print / GD.PushWarning / GD.PushError 实现的日志辅助器。
    /// </summary>
    public sealed class GodotLogHelper : GameFrameworkLog.ILogHelper
    {
        public void Log(GameFrameworkLogLevel level, object message)
        {
            switch (level)
            {
                case GameFrameworkLogLevel.Debug:
                    GD.Print($"[Debug] {message}");
                    break;
                case GameFrameworkLogLevel.Info:
                    GD.Print($"[Info] {message}");
                    break;
                case GameFrameworkLogLevel.Warning:
                    GD.PushWarning(message?.ToString());
                    break;
                case GameFrameworkLogLevel.Error:
                    GD.PushError(message?.ToString());
                    break;
                case GameFrameworkLogLevel.Fatal:
                    GD.PushError($"[Fatal] {message}");
                    break;
                default:
                    GD.Print(message);
                    break;
            }
        }
    }
}
