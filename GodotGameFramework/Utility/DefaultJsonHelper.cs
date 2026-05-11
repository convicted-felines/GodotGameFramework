//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;
using System;
using System.Text.Json;

namespace GodotGameFramework.Utility
{
    /// <summary>
    /// 基于 System.Text.Json 的 JSON 辅助器。
    /// </summary>
    public sealed class DefaultJsonHelper : global::GameFramework.Utility.Json.IJsonHelper
    {
        private static readonly JsonSerializerOptions s_Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
        };

        public string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, s_Options);
        }

        public T ToObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, s_Options);
        }

        public object ToObject(Type objectType, string json)
        {
            return JsonSerializer.Deserialize(json, objectType, s_Options);
        }
    }
}
