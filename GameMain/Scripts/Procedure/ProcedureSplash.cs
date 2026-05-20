//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;

namespace GameMain
{
    /// <summary>
    /// 流程：启动画面（Splash）。
    /// Godot 版本仅使用 Package 模式，资源由 ResourceComponent 在 _Ready 时自动初始化，
    /// 因此直接跳转到预加载流程。
    /// 可在此流程中播放 Splash 动画或 Logo 展示。
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 这里可以播放一个 Splash 动画
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // Splash 动画结束后跳转预加载
            // 当前直接跳转，如需等待动画完成，在 OnEnter 中设置标志位再于此处判断
            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}
