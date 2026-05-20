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
    /// 流程：预加载。
    /// 加载游戏运行所需的基础资源，全部完成后跳转到场景切换流程。
    /// Godot 版本的 DataTableComponent 采用同步加载，无需异步标志位等待。
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        private bool m_PreloadDone = false;

        public override bool UseNativeDialog => true;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_PreloadDone = false;
            PreloadResources();
            m_PreloadDone = true;
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_PreloadDone)
            {
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        private void PreloadResources()
        {
            // 在此处同步加载所有基础数据表，例如：
            // GameEntry.DataTable.LoadDataTable<DREntity>("res://GameMain/DataTables/Entity.bytes");
            // GameEntry.DataTable.LoadDataTable<DRUIForm>("res://GameMain/DataTables/UIForm.bytes");
            // GameEntry.DataTable.LoadDataTable<DRScene>("res://GameMain/DataTables/Scene.bytes");
            GameFrameworkLog.Info("ProcedurePreload: PreloadResources done.");
        }
    }
}
