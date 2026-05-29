//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.UI;
using GodotGameFramework;

namespace GameMain
{
    /// <summary>
    /// 流程：菜单。
    /// 打开主菜单 UI，等待玩家选择开始游戏后跳转到主游戏流程。
    /// </summary>
    public class ProcedureMenu : ProcedureBase
    {
        public const string MenuFormAssetName = "res://GameMain/UI/MenuForm.tscn";
        public const string MenuUIGroupName = "Menu";

        private bool m_StartGame = false;
        private int m_MenuFormSerialId = -1;

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
            m_MenuFormSerialId = -1;

            GameEntry.UI.OpenUIFormSuccess += OnOpenUIFormSuccess;
            m_MenuFormSerialId = GameEntry.UI.OpenUIForm(MenuFormAssetName, MenuUIGroupName, this);

            GameFrameworkLog.Info("ProcedureMenu: entered, opening menu UI.");
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.UI.OpenUIFormSuccess -= OnOpenUIFormSuccess;

            if (GameEntry.UI.HasUIForm(m_MenuFormSerialId))
                GameEntry.UI.CloseUIForm(m_MenuFormSerialId);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_StartGame)
                return;

            ChangeState<ProcedureMain>(procedureOwner);
        }

        private void OnOpenUIFormSuccess(object sender, OpenUIFormSuccessEventArgs e)
        {
            if (e.UIForm.SerialId != m_MenuFormSerialId)
                return;

            GameFrameworkLog.Info("ProcedureMenu: menu UI opened.");
        }
    }
}
