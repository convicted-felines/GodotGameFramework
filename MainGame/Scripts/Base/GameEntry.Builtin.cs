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

        public static EventComponent Event { get; private set; }

        public static ProcedureComponent Procedure { get; private set; }

        public static ResourceComponent Resource { get; private set; }

        public static DataTableComponent DataTable { get; private set; }

        public static ConfigComponent Config { get; private set; }

        public static SettingComponent Setting { get; private set; }

        public static EntityComponent Entity { get; private set; }

        public static UIComponent UI { get; private set; }

        public static SceneComponent Scene { get; private set; }

        public static LocalizationComponent Localization { get; private set; }

        public static SoundComponent Sound { get; private set; }

        public static NetworkComponent Network { get; private set; }

        public static DownloadComponent Download { get; private set; }

        public static WebRequestComponent WebRequest { get; private set; }

        public static FileSystemComponent FileSystem { get; private set; }

        public static ObjectPoolComponent ObjectPool { get; private set; }

        public static ReferencePoolComponent ReferencePool { get; private set; }

        private static void InitBuiltinComponents()
        {
            Base = GodotGameFramework.GameEntry.GetComponent<BaseComponent>();
            Event = GodotGameFramework.GameEntry.GetComponent<EventComponent>();
            Procedure = GodotGameFramework.GameEntry.GetComponent<ProcedureComponent>();
            Resource = GodotGameFramework.GameEntry.GetComponent<ResourceComponent>();
            DataTable = GodotGameFramework.GameEntry.GetComponent<DataTableComponent>();
            Config = GodotGameFramework.GameEntry.GetComponent<ConfigComponent>();
            Setting = GodotGameFramework.GameEntry.GetComponent<SettingComponent>();
            Entity = GodotGameFramework.GameEntry.GetComponent<EntityComponent>();
            UI = GodotGameFramework.GameEntry.GetComponent<UIComponent>();
            Scene = GodotGameFramework.GameEntry.GetComponent<SceneComponent>();
            Localization = GodotGameFramework.GameEntry.GetComponent<LocalizationComponent>();
            Sound = GodotGameFramework.GameEntry.GetComponent<SoundComponent>();
            Network = GodotGameFramework.GameEntry.GetComponent<NetworkComponent>();
            Download = GodotGameFramework.GameEntry.GetComponent<DownloadComponent>();
            WebRequest = GodotGameFramework.GameEntry.GetComponent<WebRequestComponent>();
            FileSystem = GodotGameFramework.GameEntry.GetComponent<FileSystemComponent>();
            ObjectPool = GodotGameFramework.GameEntry.GetComponent<ObjectPoolComponent>();
            ReferencePool = GodotGameFramework.GameEntry.GetComponent<ReferencePoolComponent>();
        }
    }
}
