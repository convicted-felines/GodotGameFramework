using GameFramework;
using GameFramework.Localization;
using GameFramework.Resource;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 本地化组件。封装 ILocalizationManager，提供多语言字典查询能力。
    ///
    /// 语言表格式（TSV）：
    ///   # 注释
    ///   KEY\t翻译文本
    ///
    /// 语言表格式（JSON 扁平对象）：
    ///   {"KEY": "翻译文本"}
    ///
    /// 使用示例：
    ///   var loc = GameEntry.GetComponent＜LocalizationComponent＞();
    ///   loc.LoadDictionary("res://Localization/zh_CN.tsv");
    ///   string text = loc.GetString("UI_StartButton");
    /// </summary>
    public sealed partial class LocalizationComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>启动时自动加载的语言（Unspecified 则跟随系统语言）。</summary>
        [Export] public Language Language = Language.Unspecified;

        /// <summary>缓冲二进制流大小（字节）；0 表示不预分配。</summary>
        [Export] public int CachedBytesSize = 0;

        /// <summary>本地化辅助器。</summary>
        [Export] public LocalizationHelperType LocalizationHelper = LocalizationHelperType.GodotLocalizationHelper;

        /// <summary>本地化数据提供辅助器。</summary>
        [Export] public LocalizationDataProviderHelperType LocalizationDataProviderHelper = LocalizationDataProviderHelperType.DefaultLocalizationDataProviderHelper;

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private ILocalizationManager m_LocalizationManager = null;

        private string LocalizationHelperTypeName => $"GodotGameFramework.{LocalizationHelper}";
        private string LocalizationDataProviderHelperTypeName => $"GodotGameFramework.{LocalizationDataProviderHelper}";

        // ── 属性 ───────────────────────────────────────────────────────────────

        public Language CurrentLanguage
        {
            get => m_LocalizationManager.Language;
            set => m_LocalizationManager.Language = value;
        }

        public Language SystemLanguage => m_LocalizationManager.SystemLanguage;

        public int DictionaryCount => m_LocalizationManager.DictionaryCount;

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_LocalizationManager = GameFrameworkEntry.GetModule<ILocalizationManager>();
            if (m_LocalizationManager == null)
            {
                GameFrameworkLog.Fatal("Localization manager is invalid.");
                return;
            }

            var resourceManager = ResourceComponent.Instance;
            if (resourceManager != null)
                m_LocalizationManager.SetResourceManager(resourceManager);

            var dataProviderHelperType = GameFramework.Utility.Assembly.GetType(LocalizationDataProviderHelperTypeName);
            if (dataProviderHelperType == null || Activator.CreateInstance(dataProviderHelperType) is not LocalizationDataProviderHelperBase dataProviderHelper)
            {
                GameFrameworkLog.Fatal($"Can not create localization data provider helper '{LocalizationDataProviderHelperTypeName}'.");
                return;
            }
            m_LocalizationManager.SetDataProviderHelper(dataProviderHelper);

            var locHelperType = GameFramework.Utility.Assembly.GetType(LocalizationHelperTypeName);
            if (locHelperType == null || Activator.CreateInstance(locHelperType) is not LocalizationHelperBase locHelper)
            {
                GameFrameworkLog.Fatal($"Can not create localization helper '{LocalizationHelperTypeName}'.");
                return;
            }
            m_LocalizationManager.SetLocalizationHelper(locHelper);

            if (CachedBytesSize > 0)
                m_LocalizationManager.EnsureCachedBytesSize(CachedBytesSize);

            if (Language != Language.Unspecified)
                m_LocalizationManager.Language = Language;
        }

        // ── 字典加载 ───────────────────────────────────────────────────────────

        /// <summary>同步加载语言表文件（TSV 或 JSON）。</summary>
        public bool LoadDictionary(string path)
        {
            string text = ReadText(path);
            if (text == null)
            {
                GameFrameworkLog.Warning($"[Localization] Cannot read file: {path}");
                return false;
            }
            return m_LocalizationManager.ParseData(text);
        }

        // ── 字典查询 ───────────────────────────────────────────────────────────

        public bool HasRawString(string key) => m_LocalizationManager.HasRawString(key);
        public string GetRawString(string key) => m_LocalizationManager.GetRawString(key);

        public bool AddRawString(string key, string value) => m_LocalizationManager.AddRawString(key, value);
        public bool RemoveRawString(string key) => m_LocalizationManager.RemoveRawString(key);
        public void RemoveAllRawStrings() => m_LocalizationManager.RemoveAllRawStrings();

        public string GetString(string key) => m_LocalizationManager.GetString(key);
        public string GetString<T>(string key, T arg) => m_LocalizationManager.GetString(key, arg);
        public string GetString<T1, T2>(string key, T1 arg1, T2 arg2) => m_LocalizationManager.GetString(key, arg1, arg2);
        public string GetString<T1, T2, T3>(string key, T1 arg1, T2 arg2, T3 arg3) => m_LocalizationManager.GetString(key, arg1, arg2, arg3);
        public string GetString<T1, T2, T3, T4>(string key, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => m_LocalizationManager.GetString(key, arg1, arg2, arg3, arg4);

        public void FreeCachedBytes() => m_LocalizationManager.FreeCachedBytes();

        // ── 私有辅助 ───────────────────────────────────────────────────────────

        private static string ReadText(string path)
        {
            if (path.StartsWith("res://") || path.StartsWith("user://"))
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                return file?.GetAsText();
            }
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8) : null;
        }
    }
}
