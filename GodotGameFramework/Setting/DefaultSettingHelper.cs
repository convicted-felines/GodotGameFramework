using GameFramework.Setting;
using GameFramework;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 游戏配置辅助器。使用 Godot ConfigFile 实现持久化（存储于 user://settings.cfg）。
    /// 对象值通过 System.Text.Json 序列化为 JSON 字符串存储。
    /// </summary>
    public sealed class DefaultSettingHelper : ISettingHelper
    {
        private const string Section = "Settings";

        private readonly string m_FilePath;
        private readonly ConfigFile m_ConfigFile;

        public DefaultSettingHelper(string filePath = "user://settings.cfg")
        {
            m_FilePath = filePath;
            m_ConfigFile = new ConfigFile();
        }

        public int Count
        {
            get
            {
                string[] keys = m_ConfigFile.GetSectionKeys(Section);
                return keys == null ? 0 : keys.Length;
            }
        }

        public bool Load()
        {
            var err = m_ConfigFile.Load(m_FilePath);
            return err == Error.Ok || err == Error.FileNotFound;
        }

        public bool Save()
        {
            return m_ConfigFile.Save(m_FilePath) == Error.Ok;
        }

        public string[] GetAllSettingNames()
        {
            return m_ConfigFile.GetSectionKeys(Section) ?? Array.Empty<string>();
        }

        public void GetAllSettingNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            string[] keys = m_ConfigFile.GetSectionKeys(Section);
            if (keys != null)
            {
                results.AddRange(keys);
            }
        }

        public bool HasSetting(string settingName)
        {
            return m_ConfigFile.HasSectionKey(Section, settingName);
        }

        public bool RemoveSetting(string settingName)
        {
            if (!HasSetting(settingName))
            {
                return false;
            }

            m_ConfigFile.EraseSectionKey(Section, settingName);
            return true;
        }

        public void RemoveAllSettings()
        {
            m_ConfigFile.EraseSection(Section);
        }

        // ── Bool ──────────────────────────────────────────────────────────────

        public bool GetBool(string settingName)
        {
            RequireExists(settingName);
            return (bool)m_ConfigFile.GetValue(Section, settingName, false);
        }

        public bool GetBool(string settingName, bool defaultValue)
        {
            return (bool)m_ConfigFile.GetValue(Section, settingName, defaultValue);
        }

        public void SetBool(string settingName, bool value)
        {
            m_ConfigFile.SetValue(Section, settingName, value);
        }

        // ── Int ───────────────────────────────────────────────────────────────

        public int GetInt(string settingName)
        {
            RequireExists(settingName);
            return (int)m_ConfigFile.GetValue(Section, settingName, 0);
        }

        public int GetInt(string settingName, int defaultValue)
        {
            return (int)m_ConfigFile.GetValue(Section, settingName, defaultValue);
        }

        public void SetInt(string settingName, int value)
        {
            m_ConfigFile.SetValue(Section, settingName, value);
        }

        // ── Float ─────────────────────────────────────────────────────────────

        public float GetFloat(string settingName)
        {
            RequireExists(settingName);
            return (float)m_ConfigFile.GetValue(Section, settingName, 0f);
        }

        public float GetFloat(string settingName, float defaultValue)
        {
            return (float)m_ConfigFile.GetValue(Section, settingName, defaultValue);
        }

        public void SetFloat(string settingName, float value)
        {
            m_ConfigFile.SetValue(Section, settingName, value);
        }

        // ── String ────────────────────────────────────────────────────────────

        public string GetString(string settingName)
        {
            RequireExists(settingName);
            return (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
        }

        public string GetString(string settingName, string defaultValue)
        {
            return (string)m_ConfigFile.GetValue(Section, settingName, defaultValue);
        }

        public void SetString(string settingName, string value)
        {
            m_ConfigFile.SetValue(Section, settingName, value);
        }

        // ── Object（JSON 序列化）─────────────────────────────────────────────

        public T GetObject<T>(string settingName)
        {
            RequireExists(settingName);
            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        public object GetObject(Type objectType, string settingName)
        {
            RequireExists(settingName);
            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize(json, objectType);
        }

        public T GetObject<T>(string settingName, T defaultObj)
        {
            if (!HasSetting(settingName))
            {
                return defaultObj;
            }

            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            if (!HasSetting(settingName))
            {
                return defaultObj;
            }

            string json = (string)m_ConfigFile.GetValue(Section, settingName, string.Empty);
            return System.Text.Json.JsonSerializer.Deserialize(json, objectType);
        }

        public void SetObject<T>(string settingName, T obj)
        {
            m_ConfigFile.SetValue(Section, settingName, System.Text.Json.JsonSerializer.Serialize(obj));
        }

        public void SetObject(string settingName, object obj)
        {
            m_ConfigFile.SetValue(Section, settingName, System.Text.Json.JsonSerializer.Serialize(obj));
        }

        // ── 私有工具 ───────────────────────────────────────────────────────────

        private void RequireExists(string settingName)
        {
            if (!HasSetting(settingName))
            {
                throw new GameFrameworkException($"Setting '{settingName}' is not exist.");
            }
        }
    }
}
