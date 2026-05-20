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
    /// 流程：主游戏。
    /// 负责初始化并驱动游戏核心逻辑，游戏结束后跳回菜单场景。
    /// </summary>
    public class ProcedureMain : ProcedureBase
    {
        private const float GameOverDelayedSeconds = 2f;

        private bool m_GotoMenu = false;
        private float m_GotoMenuDelay = 0f;

        public override bool UseNativeDialog => false;

        /// <summary>由游戏逻辑调用，触发游戏结束并延迟返回菜单。</summary>
        public void GotoMenu()
        {
            m_GotoMenu = true;
            m_GotoMenuDelay = 0f;
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_GotoMenu = false;
            m_GotoMenuDelay = 0f;

            // TODO: 初始化游戏逻辑
            // m_CurrentGame = new SurvivalGame();
            // m_CurrentGame.Initialize();
            GameFrameworkLog.Info("ProcedureMain: game started.");
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            // TODO: 关闭游戏逻辑
            // m_CurrentGame?.Shutdown();
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // TODO: 驱动游戏逻辑
            // m_CurrentGame?.Update(elapseSeconds, realElapseSeconds);

            if (m_GotoMenu)
            {
                m_GotoMenuDelay += elapseSeconds;
                if (m_GotoMenuDelay >= GameOverDelayedSeconds)
                {
                    procedureOwner.SetData<GameFramework.Variable.VarInt32>("NextSceneId", ProcedureChangeScene.MenuSceneId);
                    ChangeState<ProcedureChangeScene>(procedureOwner);
                }
            }
        }
    }
}
