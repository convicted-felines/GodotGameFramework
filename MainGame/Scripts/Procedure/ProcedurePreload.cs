//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GodotGameFramework;

namespace GameMain
{
    /// <summary>
    /// 流程：预加载。
    /// 加载游戏运行所需的基础资源，全部完成后跳转到场景切换流程。
    /// 运行时默认读取 .bytes 二进制文件，.txt 仅作为策划可读的中间文件。
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        /// <summary>
        /// 启动时需要加载的数据表名称，需与 MainGame/DataTables/DataTableNames.txt 保持一致。
        /// </summary>
        public static readonly string[] DataTableNames =
        {
            "Scene",
            "Entity",
            "Music",
            "Sound",
            "UIForm",
            "UISound",
            "UITranslation",
        };

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
                procedureOwner.SetData<VarInt32>("NextSceneId", ProcedureChangeScene.MenuSceneId);
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        private void PreloadResources()
        {
            foreach (string dataTableName in DataTableNames)
            {
                LoadDataTable(dataTableName);
            }

            GameFrameworkLog.Info("ProcedurePreload: PreloadResources done.");
        }

        private void LoadDataTable(string dataTableName)
        {
            string dataTableAssetPath = AssetUtility.GetByteDataTableAsset(dataTableName);
            GameEntry.DataTable.LoadDataTable(dataTableName);
            GameFrameworkLog.Info("Load data table '{0}' from '{1}' OK.", dataTableName, dataTableAssetPath);
        }
    }
}
