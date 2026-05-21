using GameFramework;
using GameFramework.Localization;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 本地化数据提供者辅助器。支持 TSV（Key\tValue）和 JSON 扁平对象两种格式，
    /// 与 DefaultConfigHelper 的解析规则保持一致。
    /// </summary>
    public sealed class DefaultLocalizationDataProviderHelper : LocalizationDataProviderHelperBase
    {
        public override bool ReadData(ILocalizationManager dataProvider, string dataAssetName, object dataAsset, object userData)
        {
            // dataAsset 是已加载的资源对象（异步加载路径），此处按字符串路径同步加载
            string text = ReadText(dataAssetName);
            if (text == null) return false;
            return dataProvider.ParseData(text, userData);
        }

        public override bool ReadData(ILocalizationManager dataProvider, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData)
        {
            string text = System.Text.Encoding.UTF8.GetString(dataBytes, startIndex, length);
            return dataProvider.ParseData(text, userData);
        }

        public override bool ParseData(ILocalizationManager dataProvider, string dataText, object userData)
        {
            if (dataText.TrimStart().StartsWith("{"))
                return ParseJson(dataProvider, dataText);
            return ParseTsv(dataProvider, dataText);
        }

        public override bool ParseData(ILocalizationManager dataProvider, byte[] dataBytes, int startIndex, int length, object userData)
        {
            string text = System.Text.Encoding.UTF8.GetString(dataBytes, startIndex, length);
            return ParseData(dataProvider, text, userData);
        }

        public override void ReleaseDataAsset(ILocalizationManager dataProvider, object dataAsset) { }

        // ── 解析 ───────────────────────────────────────────────────────────────

        private static bool ParseTsv(ILocalizationManager manager, string text)
        {
            int added = 0;
            foreach (string line in text.Split('\n'))
            {
                string trimmed = line.TrimEnd('\r').Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                int tab = trimmed.IndexOf('\t');
                if (tab < 0) continue;

                string key = trimmed.Substring(0, tab).Trim();
                string value = trimmed.Substring(tab + 1);
                if (!string.IsNullOrEmpty(key) && manager.AddRawString(key, value)) added++;
            }
            return added > 0;
        }

        private static bool ParseJson(ILocalizationManager manager, string text)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(text);
                int added = 0;
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (manager.AddRawString(prop.Name, prop.Value.GetString() ?? string.Empty)) added++;
                }
                return added > 0;
            }
            catch
            {
                return false;
            }
        }

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
