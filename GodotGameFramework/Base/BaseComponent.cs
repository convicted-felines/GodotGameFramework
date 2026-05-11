//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

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

        private float m_GameSpeedBeforePause = 1f;

        /// <summary>游戏逻辑时间缩放（对应 Unity Time.timeScale）。</summary>
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

        /// <summary>暂停游戏。</summary>
        public void PauseGame()
        {
            if (IsGamePaused)
                return;
            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>恢复游戏。</summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
                return;
            GameSpeed = m_GameSpeedBeforePause;
        }

        /// <summary>重置游戏速度。</summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
                return;
            GameSpeed = 1f;
        }

        private static void InitTextHelper()
        {
            GameFramework.Utility.Text.SetTextHelper(new DefaultTextHelper());
        }

        private static void InitVersionHelper()
        {
            // 可通过 Export 属性注入自定义 VersionHelper；默认不设置
        }

        private static void InitLogHelper()
        {
            GameFrameworkLog.SetLogHelper(new GodotLogHelper());
        }

        private static void InitCompressionHelper()
        {
            GameFramework.Utility.Compression.SetCompressionHelper(new DefaultCompressionHelper());
        }

        private static void InitJsonHelper()
        {
            GameFramework.Utility.Json.SetJsonHelper(new DefaultJsonHelper());
        }
    }
}
