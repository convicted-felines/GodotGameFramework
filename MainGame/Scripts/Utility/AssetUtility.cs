//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;

namespace GameMain
{
    /// <summary>
    /// MainGame 资源路径约定。业务代码应通过此类获取路径，禁止硬编码 res:// 前缀。
    /// </summary>
    public static class AssetUtility
    {
        public static string GetByteDataTableAsset(string assetName)
        {
            return Utility.Text.Format("res://MainGame/DataTables/Bytes/{0}.bytes", assetName);
        }

        public static string GetTextDataTableAsset(string assetName)
        {
            return Utility.Text.Format("res://MainGame/DataTables/Text/{0}.txt", assetName);
        }
    }
}
