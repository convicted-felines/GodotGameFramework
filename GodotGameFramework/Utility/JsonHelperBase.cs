//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using System;

namespace GodotGameFramework.Utility
{
    /// <summary>
    /// JSON 辅助器基类。继承此类实现自定义 JSON 序列化，在 BaseComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class JsonHelperBase : global::GameFramework.Utility.Json.IJsonHelper
    {
        public abstract string ToJson(object obj);
        public abstract T ToObject<T>(string json);
        public abstract object ToObject(Type objectType, string json);
    }
}
