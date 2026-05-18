using GameFramework;
using GameFramework.Config;
using Godot;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 全局配置组件。封装 IConfigManager，提供同步加载快捷方法。
    /// </summary>
    public sealed partial class ConfigComponent : GameFrameworkComponent
    {
        private IConfigManager m_ConfigManager = null;

        [Export]
        public int CachedBytesSize { get; private set; } = 0;

        public int Count => m_ConfigManager.Count;

        public override void _Ready()
        {
            base._Ready();

            m_ConfigManager = GameFrameworkEntry.GetModule<IConfigManager>();
            if (m_ConfigManager == null)
            {
                GameFrameworkLog.Fatal("Config manager is invalid.");
                return;
            }

            var helper = new DefaultConfigHelper();
            m_ConfigManager.SetDataProviderHelper(helper);
            m_ConfigManager.SetConfigHelper(helper);

            if (CachedBytesSize > 0)
            {
                m_ConfigManager.EnsureCachedBytesSize(CachedBytesSize);
            }
        }

        public void SetResourceManager(GameFramework.Resource.IResourceManager resourceManager)
        {
            m_ConfigManager.SetResourceManager(resourceManager);
        }

        // ── 同步加载（直接从 Godot res:// / user:// / 绝对路径读取）──────────────

        /// <summary>
        /// 从路径同步读取并解析全局配置文件（TSV 或 JSON）。
        /// </summary>
        public bool LoadConfig(string path)
        {
            string text = ReadTextFromPath(path);
            if (text == null)
            {
                GameFrameworkLog.Warning($"[Config] Cannot read file: {path}");
                return false;
            }

            return m_ConfigManager.ParseData(text);
        }

        // ── 查询接口透传 ───────────────────────────────────────────────────────

        public bool HasConfig(string configName) => m_ConfigManager.HasConfig(configName);

        public bool GetBool(string configName) => m_ConfigManager.GetBool(configName);
        public bool GetBool(string configName, bool defaultValue) => m_ConfigManager.GetBool(configName, defaultValue);

        public int GetInt(string configName) => m_ConfigManager.GetInt(configName);
        public int GetInt(string configName, int defaultValue) => m_ConfigManager.GetInt(configName, defaultValue);

        public float GetFloat(string configName) => m_ConfigManager.GetFloat(configName);
        public float GetFloat(string configName, float defaultValue) => m_ConfigManager.GetFloat(configName, defaultValue);

        public string GetString(string configName) => m_ConfigManager.GetString(configName);
        public string GetString(string configName, string defaultValue) => m_ConfigManager.GetString(configName, defaultValue);

        public bool AddConfig(string configName, string configValue) => m_ConfigManager.AddConfig(configName, configValue);

        public bool AddConfig(string configName, bool boolValue, int intValue, float floatValue, string stringValue)
            => m_ConfigManager.AddConfig(configName, boolValue, intValue, floatValue, stringValue);

        public bool RemoveConfig(string configName) => m_ConfigManager.RemoveConfig(configName);

        public void RemoveAllConfigs() => m_ConfigManager.RemoveAllConfigs();

        // ── 私有工具 ───────────────────────────────────────────────────────────

        private static string ReadTextFromPath(string path)
        {
            if (path.StartsWith("res://", StringComparison.Ordinal) || path.StartsWith("user://", StringComparison.Ordinal))
            {
                using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    return null;
                }

                return file.GetAsText();
            }
            else
            {
                if (!System.IO.File.Exists(path))
                {
                    return null;
                }

                return System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
            }
        }
    }
}
