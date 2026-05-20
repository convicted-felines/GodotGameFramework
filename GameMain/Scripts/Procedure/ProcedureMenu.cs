//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace GameMain
{
    /// <summary>
    /// 流程：菜单。
    /// 打开主菜单 UI，等待玩家选择开始游戏后跳转到场景切换流程。
    /// </summary>
    public class ProcedureMenu : ProcedureBase
    {
        private bool m_StartGame = false;

        public override bool UseNativeDialog => false;

        /// <summary>由菜单 UI 调用，触发开始游戏。</summary>
        public void StartGame()
        {
            m_StartGame = true;
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_StartGame = false;

            // TODO: 打开主菜单 UI
            // GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this);
            GameFrameworkLog.Info("ProcedureMenu: entered, waiting for player to start game.");
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            // TODO: 关闭主菜单 UI（如未被场景切换自动卸载）
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_StartGame)
                return;

            // 设置目标场景 ID 后切换到 ChangeScene 流程
            // procedureOwner.SetData<GameFramework.Variable.VarInt32>("NextSceneId", GameSceneId);
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }
    }
}
