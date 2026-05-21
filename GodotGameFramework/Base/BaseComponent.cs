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

        /// <summary>日志辅助器完整类名。</summary>
        [Export] public string LogHelperTypeName = "GodotGameFramework.Utility.GodotLogHelper";

        /// <summary>文本辅助器完整类名。</summary>
        [Export] public string TextHelperTypeName = "GodotGameFramework.Utility.DefaultTextHelper";

        /// <summary>压缩辅助器完整类名。</summary>
        [Export] public string CompressionHelperTypeName = "GodotGameFramework.Utility.DefaultCompressionHelper";

        /// <summary>JSON 辅助器完整类名。</summary>
        [Export] public string JsonHelperTypeName = "GodotGameFramework.Utility.DefaultJsonHelper";

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
            var type = GameFramework.Utility.Assembly.GetType(TextHelperTypeName);
            if (type == null || Activator.CreateInstance(type) is not TextHelperBase helper)
            {
                GD.PrintErr($"Can not create text helper '{TextHelperTypeName}'.");
                return;
            }
            GameFramework.Utility.Text.SetTextHelper(helper);
        }

        private static void InitVersionHelper()
        {
            // 可通过 Export 属性注入自定义 VersionHelper；默认不设置
        }

        private void InitLogHelper()
        {
            var type = GameFramework.Utility.Assembly.GetType(LogHelperTypeName);
            if (type == null || Activator.CreateInstance(type) is not LogHelperBase helper)
            {
                GD.PrintErr($"Can not create log helper '{LogHelperTypeName}'.");
                return;
            }
            GameFrameworkLog.SetLogHelper(helper);
        }

        private void InitCompressionHelper()
        {
            var type = GameFramework.Utility.Assembly.GetType(CompressionHelperTypeName);
            if (type == null || Activator.CreateInstance(type) is not CompressionHelperBase helper)
            {
                GameFrameworkLog.Fatal($"Can not create compression helper '{CompressionHelperTypeName}'.");
                return;
            }
            GameFramework.Utility.Compression.SetCompressionHelper(helper);
        }

        private void InitJsonHelper()
        {
            var type = GameFramework.Utility.Assembly.GetType(JsonHelperTypeName);
            if (type == null || Activator.CreateInstance(type) is not JsonHelperBase helper)
            {
                GameFrameworkLog.Fatal($"Can not create JSON helper '{JsonHelperTypeName}'.");
                return;
            }
            GameFramework.Utility.Json.SetJsonHelper(helper);
        }
    }
}
