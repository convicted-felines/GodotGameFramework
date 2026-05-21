//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Setting;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 游戏配置辅助器基类。继承此类实现自定义配置持久化，在 SettingComponent 的 Inspector 中填写完整类名即可生效。
    /// 框架通过反射创建实例后，会调用 Initialize(filePath) 传入存储路径。
    /// </summary>
    public abstract class SettingHelperBase : ISettingHelper
    {
        /// <summary>由 SettingComponent 在创建实例后调用，传入 Inspector 配置的文件路径。</summary>
        public virtual void Initialize(string filePath) { }

        public abstract int Count { get; }
        public abstract bool Load();
        public abstract bool Save();
        public abstract string[] GetAllSettingNames();
        public abstract void GetAllSettingNames(List<string> results);
        public abstract bool HasSetting(string settingName);
        public abstract bool RemoveSetting(string settingName);
        public abstract void RemoveAllSettings();
        public abstract bool GetBool(string settingName);
        public abstract bool GetBool(string settingName, bool defaultValue);
        public abstract void SetBool(string settingName, bool value);
        public abstract int GetInt(string settingName);
        public abstract int GetInt(string settingName, int defaultValue);
        public abstract void SetInt(string settingName, int value);
        public abstract float GetFloat(string settingName);
        public abstract float GetFloat(string settingName, float defaultValue);
        public abstract void SetFloat(string settingName, float value);
        public abstract string GetString(string settingName);
        public abstract string GetString(string settingName, string defaultValue);
        public abstract void SetString(string settingName, string value);
        public abstract T GetObject<T>(string settingName);
        public abstract T GetObject<T>(string settingName, T defaultObj);
        public abstract void SetObject<T>(string settingName, T obj);
        public abstract object GetObject(Type objectType, string settingName);
        public abstract object GetObject(Type objectType, string settingName, object defaultObj);
        public abstract void SetObject(string settingName, object obj);
    }
}
