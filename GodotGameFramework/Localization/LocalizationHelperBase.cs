//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Localization;

namespace GodotGameFramework
{
    /// <summary>
    /// 本地化辅助器基类。继承此类实现自定义本地化辅助器，在 LocalizationComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class LocalizationHelperBase : ILocalizationHelper
    {
        public abstract Language SystemLanguage { get; }
    }
}
