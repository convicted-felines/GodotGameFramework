//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GodotGameFramework;

namespace GameMain
{
    /// <summary>
    /// 游戏入口 - 内置框架组件。
    /// </summary>
    public partial class GameEntry
    {
        public static BaseComponent Base { get; private set; }

        public static GodotGameFramework.EventComponent Event { get; private set; }

        public static GodotGameFramework.ProcedureComponent Procedure { get; private set; }

        public static GodotGameFramework.ResourceComponent Resource { get; private set; }

        public static GodotGameFramework.DataTableComponent DataTable { get; private set; }

        public static GodotGameFramework.ConfigComponent Config { get; private set; }

        public static GodotGameFramework.SettingComponent Setting { get; private set; }

        public static GodotGameFramework.EntityComponent Entity { get; private set; }

        public static GodotGameFramework.UIComponent UI { get; private set; }

        public static GodotGameFramework.SceneComponent Scene { get; private set; }

        public static GodotGameFramework.LocalizationComponent Localization { get; private set; }

        public static GodotGameFramework.SoundComponent Sound { get; private set; }

        public static GodotGameFramework.NetworkComponent Network { get; private set; }

        public static GodotGameFramework.DownloadComponent Download { get; private set; }

        public static GodotGameFramework.WebRequestComponent WebRequest { get; private set; }

        public static GodotGameFramework.FileSystemComponent FileSystem { get; private set; }

        public static GodotGameFramework.ObjectPoolComponent ObjectPool { get; private set; }

        public static GodotGameFramework.ReferencePoolComponent ReferencePool { get; private set; }

        private static void InitBuiltinComponents()
        {
            Base = GodotGameFramework.GameEntry.GetComponent<BaseComponent>();
            Event = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.EventComponent>();
            Procedure = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.ProcedureComponent>();
            Resource = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.ResourceComponent>();
            DataTable = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.DataTableComponent>();
            Config = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.ConfigComponent>();
            Setting = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.SettingComponent>();
            Entity = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.EntityComponent>();
            UI = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.UIComponent>();
            Scene = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.SceneComponent>();
            Localization = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.LocalizationComponent>();
            Sound = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.SoundComponent>();
            Network = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.NetworkComponent>();
            Download = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.DownloadComponent>();
            WebRequest = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.WebRequestComponent>();
            FileSystem = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.FileSystemComponent>();
            ObjectPool = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.ObjectPoolComponent>();
            ReferencePool = GodotGameFramework.GameEntry.GetComponent<GodotGameFramework.ReferencePoolComponent>();
        }
    }
}
