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
    public sealed class DefaultJsonHelper : JsonHelperBase
    {
        private static readonly JsonSerializerOptions s_Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
        };

        public override string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, s_Options);
        }

        public override T ToObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, s_Options);
        }

        public override object ToObject(Type objectType, string json)
        {
            return JsonSerializer.Deserialize(json, objectType, s_Options);
        }
    }
}
