//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

namespace GameMain
{
    /// <summary>
    /// 游戏流程基类。
    /// </summary>
    public abstract class ProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        /// <summary>
        /// 获取流程是否使用原生对话框。
        /// 在资源更新完成之前的流程中，使用原生对话框进行消息提示。
        /// </summary>
        public abstract bool UseNativeDialog { get; }
    }
}
