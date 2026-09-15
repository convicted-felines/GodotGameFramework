//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework;

namespace GameMain
{
    /// <summary>
    /// 流程：启动。
    /// 负责游戏初始化工作：语言设置、音量恢复等，完成后立即跳转到 Splash 流程。
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 初始化语言设置
            InitLanguage();

            // 初始化音量设置
            InitSoundSettings();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 下一帧立即跳转到 Splash 流程
            ChangeState<ProcedureSplash>(procedureOwner);
        }

        private void InitLanguage()
        {
            // 读取已保存的语言设置，未设置时使用系统语言
            // TODO: 根据项目语言配置扩展此方法
            GameFrameworkLog.Info("ProcedureLaunch: InitLanguage.");
        }

        private void InitSoundSettings()
        {
            // 恢复保存的音量和静音设置
            // TODO: 通过 GameEntry.Sound 和 GameEntry.Setting 恢复各音效组的音量
            GameFrameworkLog.Info("ProcedureLaunch: InitSoundSettings.");
        }
    }
}
