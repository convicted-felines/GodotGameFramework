//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Scene;
using System;

namespace GameMain
{
    /// <summary>
    /// 流程：切换场景。
    /// 卸载当前所有场景并加载目标场景，完成后根据目标类型跳转到菜单或主游戏流程。
    /// 通过 IFsm SetData 传入 "NextSceneId"（int）来指定目标场景。
    /// </summary>
    public class ProcedureChangeScene : ProcedureBase
    {
        public const int MenuSceneId = 1;

        private bool m_ChangeToMenu = false;
        private bool m_IsChangeSceneComplete = false;

        public override bool UseNativeDialog => false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_IsChangeSceneComplete = false;

            GameEntry.Scene.LoadSceneSuccess += OnLoadSceneSuccess;
            GameEntry.Scene.LoadSceneFailure += OnLoadSceneFailure;
            GameEntry.Scene.LoadSceneUpdate += OnLoadSceneUpdate;

            // 卸载当前所有已加载场景
            string[] loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            foreach (string sceneName in loadedSceneAssetNames)
            {
                GameEntry.Scene.UnloadScene(sceneName);
            }

            // 读取目标场景 ID
            int nextSceneId = procedureOwner.GetData<GameFramework.Variable.VarInt32>("NextSceneId");
            m_ChangeToMenu = nextSceneId == MenuSceneId;

            // TODO: 从数据表中查找场景资源路径，此处先用占位路径
            string nextSceneAssetName = GetSceneAssetName(nextSceneId);
            if (string.IsNullOrEmpty(nextSceneAssetName))
            {
                GameFrameworkLog.Error("ProcedureChangeScene: scene asset name is invalid for id '{0}'.", nextSceneId);
                return;
            }

            GameEntry.Scene.LoadScene(nextSceneAssetName, this);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Scene.LoadSceneSuccess -= OnLoadSceneSuccess;
            GameEntry.Scene.LoadSceneFailure -= OnLoadSceneFailure;
            GameEntry.Scene.LoadSceneUpdate -= OnLoadSceneUpdate;

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_IsChangeSceneComplete)
                return;

            if (m_ChangeToMenu)
                ChangeState<ProcedureMenu>(procedureOwner);
            else
                ChangeState<ProcedureMain>(procedureOwner);
        }

        private static string GetSceneAssetName(int sceneId)
        {
            // TODO: 通过数据表 DRScene 查找对应路径
            // return GameEntry.DataTable.GetDataTable<DRScene>().GetDataRow(sceneId).AssetName;
            return string.Empty;
        }

        private void OnLoadSceneSuccess(object sender, GameFrameworkEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            if (ne.UserData != this)
                return;

            GameFrameworkLog.Info("Load scene '{0}' OK ({1:F1}s).", ne.SceneAssetName, ne.Duration);
            m_IsChangeSceneComplete = true;
        }

        private void OnLoadSceneFailure(object sender, GameFrameworkEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            if (ne.UserData != this)
                return;

            GameFrameworkLog.Error("Load scene '{0}' failure, error message: '{1}'.", ne.SceneAssetName, ne.ErrorMessage);
        }

        private void OnLoadSceneUpdate(object sender, GameFrameworkEventArgs e)
        {
            LoadSceneUpdateEventArgs ne = (LoadSceneUpdateEventArgs)e;
            if (ne.UserData != this)
                return;

            GameFrameworkLog.Info("Load scene '{0}' update, progress: {1:P0}.", ne.SceneAssetName, ne.Progress);
        }
    }
}
