//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;
using GameFramework.Config;

namespace GodotGameFramework
{
    /// <summary>
    /// 全局配置辅助器基类。继承此类实现自定义配置辅助器，在 ConfigComponent 的 Inspector 中填写完整类名即可生效。
    /// 实现类同时充当 IConfigHelper 和 IDataProviderHelper&lt;IConfigManager&gt;，与框架双接口注入模式对应。
    /// </summary>
    public abstract class ConfigHelperBase : IConfigHelper, IDataProviderHelper<IConfigManager>
    {
        public abstract bool ReadData(IConfigManager owner, string dataAssetName, object dataAsset, object userData);
        public abstract bool ReadData(IConfigManager owner, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData);
        public abstract bool ParseData(IConfigManager owner, string dataText, object userData);
        public abstract bool ParseData(IConfigManager owner, byte[] dataBytes, int startIndex, int length, object userData);
        public abstract void ReleaseDataAsset(IConfigManager owner, object dataAsset);
    }
}
