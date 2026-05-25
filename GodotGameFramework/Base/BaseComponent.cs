using GameFramework;
using GodotGameFramework.Utility;
using Godot;
using System;
using System.Text.Json;

namespace GodotGameFramework
{
    /// <summary>
    /// 基础组件，框架必须最先挂载的 Node。
    /// 负责初始化 GameFrameworkEntry、日志、JSON、压缩等 Helper，并驱动每帧 Update。
    /// </summary>
    public sealed partial class BaseComponent : GameFrameworkComponent
    {
        [Export] private float m_GameSpeed = 1f;
        [Export] private bool m_RunInBackground = true;
        [Export] private bool m_NeverSleep = true;

        [Export] public LogHelperType LogHelper = LogHelperType.GodotLogHelper;

        [Export] public TextHelperType TextHelper = TextHelperType.DefaultTextHelper;

        [Export] public CompressionHelperType CompressionHelper = CompressionHelperType.DefaultCompressionHelper;

        [Export] public JsonHelperType JsonHelper = JsonHelperType.DefaultJsonHelper;

        private float m_GameSpeedBeforePause = 1f;

        public float GameSpeed
        {
            get => m_GameSpeed;
            set
            {
                m_GameSpeed = value >= 0f ? value : 0f;
                Engine.TimeScale = m_GameSpeed;
            }
        }

        public bool IsGamePaused => m_GameSpeed <= 0f;

        public bool IsNormalGameSpeed => Math.Abs(m_GameSpeed - 1f) < float.Epsilon;

        public override void _Ready()
        {
            base._Ready();

            InitTextHelper();
            InitVersionHelper();
            InitLogHelper();
            InitCompressionHelper();
            InitJsonHelper();

            GameFrameworkLog.Info("Game Framework version: {0}.", GameFramework.Version.GameFrameworkVersion);
            GameFrameworkLog.Info("Game version: {0} ({1}).", GameFramework.Version.GameVersion, GameFramework.Version.InternalGameVersion);

            GameSpeed = m_GameSpeed;
        }

        public override void _Process(double delta)
        {
            GameFrameworkEntry.Update((float)delta, (float)delta);
        }

        public override void _ExitTree()
        {
            GameFrameworkEntry.Shutdown();
        }

        public void PauseGame()
        {
            if (IsGamePaused) return;
            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        public void ResumeGame()
        {
            if (!IsGamePaused) return;
            GameSpeed = m_GameSpeedBeforePause;
        }

        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed) return;
            GameSpeed = 1f;
        }

        private void InitTextHelper()
        {
            string typeName = $"GodotGameFramework.Utility.{TextHelper}";
            var type = GameFramework.Utility.Assembly.GetType(typeName);
            if (type == null || Activator.CreateInstance(type) is not TextHelperBase helper)
            {
                GD.PrintErr($"Can not create text helper '{typeName}'.");
                return;
            }
            GameFramework.Utility.Text.SetTextHelper(helper);
        }

        private static void InitVersionHelper() { }

        private void InitLogHelper()
        {
            string typeName = $"GodotGameFramework.Utility.{LogHelper}";
            var type = GameFramework.Utility.Assembly.GetType(typeName);
            if (type == null || Activator.CreateInstance(type) is not LogHelperBase helper)
            {
                GD.PrintErr($"Can not create log helper '{typeName}'.");
                return;
            }
            GameFrameworkLog.SetLogHelper(helper);
        }

        private void InitCompressionHelper()
        {
            string typeName = $"GodotGameFramework.Utility.{CompressionHelper}";
            var type = GameFramework.Utility.Assembly.GetType(typeName);
            if (type == null || Activator.CreateInstance(type) is not CompressionHelperBase helper)
            {
                GameFrameworkLog.Fatal($"Can not create compression helper '{typeName}'.");
                return;
            }
            GameFramework.Utility.Compression.SetCompressionHelper(helper);
        }

        private void InitJsonHelper()
        {
            string typeName = $"GodotGameFramework.Utility.{JsonHelper}";
            var type = GameFramework.Utility.Assembly.GetType(typeName);
            if (type == null || Activator.CreateInstance(type) is not JsonHelperBase helper)
            {
                GameFrameworkLog.Fatal($"Can not create JSON helper '{typeName}'.");
                return;
            }
            GameFramework.Utility.Json.SetJsonHelper(helper);
        }
    }
}
