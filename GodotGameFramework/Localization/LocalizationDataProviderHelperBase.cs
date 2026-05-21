//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;
using GameFramework.Localization;

namespace GodotGameFramework
{
    /// <summary>
    /// 本地化数据提供者辅助器基类。继承此类实现自定义本地化数据解析，在 LocalizationComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class LocalizationDataProviderHelperBase : IDataProviderHelper<ILocalizationManager>
    {
        public abstract bool ReadData(ILocalizationManager dataProvider, string dataAssetName, object dataAsset, object userData);
        public abstract bool ReadData(ILocalizationManager dataProvider, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData);
        public abstract bool ParseData(ILocalizationManager dataProvider, string dataString, object userData);
        public abstract bool ParseData(ILocalizationManager dataProvider, byte[] dataBytes, int startIndex, int length, object userData);
        public abstract void ReleaseDataAsset(ILocalizationManager dataProvider, object dataAsset);
    }
}
