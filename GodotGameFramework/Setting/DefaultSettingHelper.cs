using GameFramework.Setting;
using GameFramework;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 默认游戏配置辅助器。使用 Godot ConfigFile 实现持久化。
    /// SettingComponent 创建实例后会调用 Initialize(filePath) 传入存储路径。
    /// </summary>
    public sealed class DefaultSettingHelper : SettingHelperBase
    {
        private const string Section = "Settings";

        private string m_FilePath = "user://settings.cfg";
        private ConfigFile m_ConfigFile = new ConfigFile();

        public override void Initialize(string filePath)
        {
            m_FilePath = filePath;
            m_ConfigFile = new ConfigFile();
        }

        public override int Count
        {
            get
            {
                string[] keys = m_ConfigFile.GetSectionKeys(Section);
                return keys == null ? 0 : keys.Length;
            }
        }

        public override bool Load()
        {
            var err = m_ConfigFile.Load(m_FilePath);
            return err == Error.Ok || err == Error.FileNotFound;
        }

        public override bool Save()
        {
            return m_ConfigFile.Save(m_FilePath) == Error.Ok;
        }

        public override string[] GetAllSettingNames()
        {
            return m_ConfigFile.GetSectionKeys(Section) ?? Array.Empty<string>();
        }

        public override void GetAllSettingNames(List<string> results)
        {
            if (results == null)
                throw new GameFrameworkException("Results is invalid.");

            results.Clear();
            string[] keys = m_ConfigFile.GetSectionKeys(Section);
            if (keys != null)
                results.AddRange(keys);
        }

        public override bool HasSetting(string settingName)
            => m_ConfigFile.HasSectionKey(Section, settingName);

        public override bool RemoveSetting(string settingName)
        {
            if (!HasSetting(settingName)) return false;
            m_ConfigFile.EraseSectionKey(Section, settingName);
            return true;
        }

        public override void RemoveAllSettings()
            => m_ConfigFile.EraseSection(Section);

        public override bool GetBool(string settingName)
        {
            RequireExists(settingName);
            return (bool)m_ConfigFile.GetValue(Section, settingName, false);
        }

        public override bool GetBool(string settingName, bool defaultValue)
            => (bool)m_ConfigFile.GetValue(Section, settingName, defaultValue);

        public override void SetBool(string settingName, bool value)
            => m_ConfigFile.SetValue(Section, settingName, value);

        public override int GetInt(string settingName)
        {
            RequireExists(settingName);
            return (int)m_ConfigFile.GetValue(Section, settingName, 0);
        }

        public override int GetInt(string settingName, int defaultValue)
            => (int)m_ConfigFile.GetValue(Section, settingName, defaultValue);

        public override void SetInt(string settingName, int value)
            => m_ConfigFile.SetValue(Section, settingName, value);

        public override float GetFloat(string settingName)
        {
            RequireExists(settingName);
            return (float)m_ConfigFile.GetValue(Section, settingName, 0f);
        }

        public override float GetFloat(string settingName, float defaultValue)
            => (float)m_ConfigFile.GetValue(Section, settingName, defaultValue);

        public override void SetFloat(string settingName, float value)
            => m_ConfigFile.SetValue(Section, settingName, value);

        public override string GetString(string settingName)
        {
            RequireExists(settingName);
            return (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
        }

        public override string GetString(string settingName, string defaultValue)
            => (string)m_ConfigFile.GetValue(Section, settingName, defaultValue);

        public override void SetString(string settingName, string value)
            => m_ConfigFile.SetValue(Section, settingName, value);

        public override T GetObject<T>(string settingName)
        {
            RequireExists(settingName);
            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        public override object GetObject(Type objectType, string settingName)
        {
            RequireExists(settingName);
            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize(json, objectType);
        }

        public override T GetObject<T>(string settingName, T defaultObj)
        {
            if (!HasSetting(settingName)) return defaultObj;
            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        public override object GetObject(Type objectType, string settingName, object defaultObj)
        {
            if (!HasSetting(settingName)) return defaultObj;
            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize(json, objectType);
        }

        public override void SetObject<T>(string settingName, T obj)
            => m_ConfigFile.SetValue(Section, settingName, System.Text.Json.JsonSerializer.Serialize(obj));

        public override void SetObject(string settingName, object obj)
            => m_ConfigFile.SetValue(Section, settingName, System.Text.Json.JsonSerializer.Serialize(obj));

        private void RequireExists(string settingName)
        {
            if (!HasSetting(settingName))
                throw new GameFrameworkException($"Setting '{settingName}' is not exist.");
        }
    }
}
