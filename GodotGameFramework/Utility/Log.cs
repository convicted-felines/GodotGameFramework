//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using System.Diagnostics;
using GameFramework;

namespace GodotGameFramework
{
    /// <summary>
    /// 日志类，提供与 UnityGameFramework 一致的静态日志接口。
    /// 条件编译符号层级：
    ///   Debug   — ENABLE_LOG | ENABLE_DEBUG_LOG | ENABLE_DEBUG_AND_ABOVE_LOG
    ///   Info    — ENABLE_LOG | ENABLE_INFO_LOG  | ENABLE_DEBUG_AND_ABOVE_LOG | ENABLE_INFO_AND_ABOVE_LOG
    ///   Warning — ENABLE_LOG | ENABLE_WARNING_LOG | ENABLE_DEBUG_AND_ABOVE_LOG | ENABLE_INFO_AND_ABOVE_LOG | ENABLE_WARNING_AND_ABOVE_LOG
    ///   Error   — ENABLE_LOG | ENABLE_ERROR_LOG | ENABLE_DEBUG_AND_ABOVE_LOG | ENABLE_INFO_AND_ABOVE_LOG | ENABLE_WARNING_AND_ABOVE_LOG | ENABLE_ERROR_AND_ABOVE_LOG
    ///   Fatal   — 始终输出
    /// </summary>
    public static class Log
    {
        // ----------------------------------------------------------------
        // Debug
        // ----------------------------------------------------------------

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug(object message)
            => GameFrameworkLog.Debug(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug(string message)
            => GameFrameworkLog.Debug(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug<T>(string message, T arg)
            => GameFrameworkLog.Debug(message, arg);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug<T1, T2>(string message, T1 arg1, T2 arg2)
            => GameFrameworkLog.Debug(message, arg1, arg2);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
            => GameFrameworkLog.Debug(message, arg1, arg2, arg3);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_DEBUG_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        public static void Debug(string message, params object[] args)
            => GameFrameworkLog.Debug(string.Format(message, args));

        // ----------------------------------------------------------------
        // Info
        // ----------------------------------------------------------------

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info(object message)
            => GameFrameworkLog.Info(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info(string message)
            => GameFrameworkLog.Info(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info<T>(string message, T arg)
            => GameFrameworkLog.Info(message, arg);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info<T1, T2>(string message, T1 arg1, T2 arg2)
            => GameFrameworkLog.Info(message, arg1, arg2);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
            => GameFrameworkLog.Info(message, arg1, arg2, arg3);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_INFO_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        public static void Info(string message, params object[] args)
            => GameFrameworkLog.Info(string.Format(message, args));

        // ----------------------------------------------------------------
        // Warning
        // ----------------------------------------------------------------

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning(object message)
            => GameFrameworkLog.Warning(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning(string message)
            => GameFrameworkLog.Warning(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning<T>(string message, T arg)
            => GameFrameworkLog.Warning(message, arg);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning<T1, T2>(string message, T1 arg1, T2 arg2)
            => GameFrameworkLog.Warning(message, arg1, arg2);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
            => GameFrameworkLog.Warning(message, arg1, arg2, arg3);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_WARNING_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        public static void Warning(string message, params object[] args)
            => GameFrameworkLog.Warning(string.Format(message, args));

        // ----------------------------------------------------------------
        // Error
        // ----------------------------------------------------------------

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error(object message)
            => GameFrameworkLog.Error(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error(string message)
            => GameFrameworkLog.Error(message);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error<T>(string message, T arg)
            => GameFrameworkLog.Error(message, arg);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error<T1, T2>(string message, T1 arg1, T2 arg2)
            => GameFrameworkLog.Error(message, arg1, arg2);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
            => GameFrameworkLog.Error(message, arg1, arg2, arg3);

        [Conditional("ENABLE_LOG")]
        [Conditional("ENABLE_ERROR_LOG")]
        [Conditional("ENABLE_DEBUG_AND_ABOVE_LOG")]
        [Conditional("ENABLE_INFO_AND_ABOVE_LOG")]
        [Conditional("ENABLE_WARNING_AND_ABOVE_LOG")]
        [Conditional("ENABLE_ERROR_AND_ABOVE_LOG")]
        public static void Error(string message, params object[] args)
            => GameFrameworkLog.Error(string.Format(message, args));

        // ----------------------------------------------------------------
        // Fatal — 始终输出，不受条件编译控制
        // ----------------------------------------------------------------

        public static void Fatal(object message)
            => GameFrameworkLog.Fatal(message);

        public static void Fatal(string message)
            => GameFrameworkLog.Fatal(message);

        public static void Fatal<T>(string message, T arg)
            => GameFrameworkLog.Fatal(message, arg);

        public static void Fatal<T1, T2>(string message, T1 arg1, T2 arg2)
            => GameFrameworkLog.Fatal(message, arg1, arg2);

        public static void Fatal<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
            => GameFrameworkLog.Fatal(message, arg1, arg2, arg3);

        public static void Fatal(string message, params object[] args)
            => GameFrameworkLog.Fatal(string.Format(message, args));
    }
}
