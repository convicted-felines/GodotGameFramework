using GameFramework.Localization;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 本地化辅助器。将 Godot 系统语言映射到 GameFramework Language 枚举。
    /// </summary>
    public sealed class GodotLocalizationHelper : LocalizationHelperBase
    {
        public override Language SystemLanguage
        {
            get
            {
                // Godot 返回 IETF 语言标签，如 "zh_CN"、"en"、"ja"
                string locale = OS.GetLocaleLanguage();
                return locale switch
                {
                    "zh" => Language.ChineseSimplified,
                    "zh_TW" or "zh_HK" => Language.ChineseTraditional,
                    "en" => Language.English,
                    "fr" => Language.French,
                    "de" => Language.German,
                    "ja" => Language.Japanese,
                    "ko" => Language.Korean,
                    "ru" => Language.Russian,
                    "es" => Language.Spanish,
                    "pt" => Language.PortuguesePortugal,
                    "pt_BR" => Language.PortugueseBrazil,
                    "it" => Language.Italian,
                    "ar" => Language.Arabic,
                    "tr" => Language.Turkish,
                    "pl" => Language.Polish,
                    "nl" => Language.Dutch,
                    "sv" => Language.Swedish,
                    "da" => Language.Danish,
                    "fi" => Language.Finnish,
                    "nb" or "no" => Language.Norwegian,
                    "cs" => Language.Czech,
                    "sk" => Language.Slovak,
                    "hu" => Language.Hungarian,
                    "ro" => Language.Romanian,
                    "bg" => Language.Bulgarian,
                    "hr" => Language.Croatian,
                    "uk" => Language.Ukrainian,
                    "el" => Language.Greek,
                    "he" => Language.Hebrew,
                    "th" => Language.Thai,
                    "vi" => Language.Vietnamese,
                    "id" => Language.Indonesian,
                    "ml" => Language.Malayalam,
                    _ => Language.Unspecified
                };
            }
        }
    }
}
