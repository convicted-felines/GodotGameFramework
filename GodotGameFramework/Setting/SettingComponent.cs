using GameFramework;
using GameFramework.Setting;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 游戏配置组件（Setting）。封装 ISettingManager，管理玩家持久化数据。
    /// </summary>
    public sealed partial class SettingComponent : GameFrameworkComponent
    {
        private ISettingManager m_SettingManager = null;

        /// <summary>设置文件路径（支持 user:// 协议）。</summary>
        [Export]
        public string SettingFilePath { get; set; } = "user://settings.cfg";

        public int Count => m_SettingManager.Count;

        public override void _Ready()
        {
            base._Ready();

            m_SettingManager = GameFrameworkEntry.GetModule<ISettingManager>();
            if (m_SettingManager == null)
            {
                GameFrameworkLog.Fatal("Setting manager is invalid.");
                return;
            }

            var helper = new DefaultSettingHelper(SettingFilePath);
            m_SettingManager.SetSettingHelper(helper);

            // 启动时自动加载
            if (!m_SettingManager.Load())
            {
                GameFrameworkLog.Warning("[Setting] Load failed, starting with empty settings.");
            }
        }

        public override void _ExitTree()
        {
            // 场景退出时自动保存
            m_SettingManager?.Save();
            base._ExitTree();
        }

        // ── 持久化 ─────────────────────────────────────────────────────────────

        public bool Load() => m_SettingManager.Load();
        public bool Save() => m_SettingManager.Save();

        // ── 查询与修改 ─────────────────────────────────────────────────────────

        public string[] GetAllSettingNames() => m_SettingManager.GetAllSettingNames();
        public void GetAllSettingNames(List<string> results) => m_SettingManager.GetAllSettingNames(results);

        public bool HasSetting(string settingName) => m_SettingManager.HasSetting(settingName);
        public bool RemoveSetting(string settingName) => m_SettingManager.RemoveSetting(settingName);
        public void RemoveAllSettings() => m_SettingManager.RemoveAllSettings();

        public bool GetBool(string settingName) => m_SettingManager.GetBool(settingName);
        public bool GetBool(string settingName, bool defaultValue) => m_SettingManager.GetBool(settingName, defaultValue);
        public void SetBool(string settingName, bool value) => m_SettingManager.SetBool(settingName, value);

        public int GetInt(string settingName) => m_SettingManager.GetInt(settingName);
        public int GetInt(string settingName, int defaultValue) => m_SettingManager.GetInt(settingName, defaultValue);
        public void SetInt(string settingName, int value) => m_SettingManager.SetInt(settingName, value);

        public float GetFloat(string settingName) => m_SettingManager.GetFloat(settingName);
        public float GetFloat(string settingName, float defaultValue) => m_SettingManager.GetFloat(settingName, defaultValue);
        public void SetFloat(string settingName, float value) => m_SettingManager.SetFloat(settingName, value);

        public string GetString(string settingName) => m_SettingManager.GetString(settingName);
        public string GetString(string settingName, string defaultValue) => m_SettingManager.GetString(settingName, defaultValue);
        public void SetString(string settingName, string value) => m_SettingManager.SetString(settingName, value);

        public T GetObject<T>(string settingName) => m_SettingManager.GetObject<T>(settingName);
        public T GetObject<T>(string settingName, T defaultObj) => m_SettingManager.GetObject<T>(settingName, defaultObj);
        public void SetObject<T>(string settingName, T obj) => m_SettingManager.SetObject(settingName, obj);

        public object GetObject(Type objectType, string settingName) => m_SettingManager.GetObject(objectType, settingName);
        public object GetObject(Type objectType, string settingName, object defaultObj) => m_SettingManager.GetObject(objectType, settingName, defaultObj);
        public void SetObject(string settingName, object obj) => m_SettingManager.SetObject(settingName, obj);
    }
}
