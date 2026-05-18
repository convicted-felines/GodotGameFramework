using GameFramework.Config;
using GameFramework;
using Godot;
using System;
using System.Text;

namespace GodotGameFramework
{
    /// <summary>
    /// 全局配置辅助器。支持两种格式：
    ///   • TSV（制表符分隔）：每行 "Key\tValue"，# 开头为注释行。
    ///   • JSON 对象：{"Key": "Value", ...}，值统一作为字符串写入 ConfigManager.AddConfig。
    /// </summary>
    public sealed class DefaultConfigHelper : IConfigHelper, IDataProviderHelper<IConfigManager>
    {
        public bool ReadData(IConfigManager owner, string dataAssetName, object dataAsset, object userData)
        {
            // dataAsset 由 ResourceManager 加载，Godot 文本资源为 string，二进制为 byte[]
            if (dataAsset is byte[] bytes)
            {
                return owner.ParseData(bytes, userData);
            }

            if (dataAsset is string text)
            {
                return owner.ParseData(text, userData);
            }

            GameFrameworkLog.Warning($"[Config] Unsupported asset type: {dataAsset?.GetType()}");
            return false;
        }

        public bool ReadData(IConfigManager owner, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData)
        {
            return owner.ParseData(dataBytes, startIndex, length, userData);
        }

        public bool ParseData(IConfigManager owner, string dataText, object userData)
        {
            if (string.IsNullOrEmpty(dataText))
            {
                return false;
            }

            // JSON 检测：去掉 BOM 后首字符为 '{'
            string trimmed = dataText.TrimStart('﻿', ' ', '\t', '\r', '\n');
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return ParseJson(owner, trimmed);
            }

            return ParseTsv(owner, dataText);
        }

        public bool ParseData(IConfigManager owner, byte[] dataBytes, int startIndex, int length, object userData)
        {
            string text = Encoding.UTF8.GetString(dataBytes, startIndex, length);
            return ParseData(owner, text, userData);
        }

        public void ReleaseDataAsset(IConfigManager owner, object dataAsset)
        {
            // Godot 资源由 GC 管理，无需手动释放
        }

        // ── TSV 解析 ────────────────────────────────────────────────────────────
        // 格式：每行 "ConfigName\tValue"，# 开头为注释，空行跳过。
        private static bool ParseTsv(IConfigManager owner, string text)
        {
            try
            {
                string[] lines = text.Split('\n');
                foreach (string rawLine in lines)
                {
                    string line = rawLine.TrimEnd('\r');
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int tab = line.IndexOf('\t');
                    if (tab < 0)
                    {
                        continue;
                    }

                    string key = line.Substring(0, tab).Trim();
                    string value = line.Substring(tab + 1).Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        owner.AddConfig(key, value);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                GameFrameworkLog.Warning($"[Config] TSV parse failed: {ex.Message}");
                return false;
            }
        }

        // ── JSON 解析 ────────────────────────────────────────────────────────────
        // 只支持扁平 JSON 对象，值类型可为 string/number/bool/null。
        // 依赖 System.Text.Json（Godot .NET 项目已包含）。
        private static bool ParseJson(IConfigManager owner, string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
                {
                    GameFrameworkLog.Warning("[Config] JSON root must be an object.");
                    return false;
                }

                foreach (var prop in root.EnumerateObject())
                {
                    string value = prop.Value.ValueKind switch
                    {
                        System.Text.Json.JsonValueKind.String => prop.Value.GetString(),
                        System.Text.Json.JsonValueKind.True => "true",
                        System.Text.Json.JsonValueKind.False => "false",
                        System.Text.Json.JsonValueKind.Null => string.Empty,
                        _ => prop.Value.GetRawText()
                    };
                    owner.AddConfig(prop.Name, value);
                }

                return true;
            }
            catch (Exception ex)
            {
                GameFrameworkLog.Warning($"[Config] JSON parse failed: {ex.Message}");
                return false;
            }
        }
    }
}
