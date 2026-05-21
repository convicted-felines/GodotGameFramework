//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;
using GameFramework.DataTable;

namespace GodotGameFramework
{
    /// <summary>
    /// 数据提供者辅助器基类。继承此类实现自定义数据表解析，在 DataTableComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class DataProviderHelperBase : IDataProviderHelper<DataTableBase>
    {
        public abstract bool ReadData(DataTableBase dataProviderOwner, string dataAssetName, object dataAsset, object userData);
        public abstract bool ReadData(DataTableBase dataProviderOwner, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData);
        public abstract bool ParseData(DataTableBase dataProviderOwner, string dataString, object userData);
        public abstract bool ParseData(DataTableBase dataProviderOwner, byte[] dataBytes, int startIndex, int length, object userData);
        public abstract void ReleaseDataAsset(DataTableBase dataProviderOwner, object dataAsset);
    }
}
