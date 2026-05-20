//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

namespace GodotGameFramework
{
    /// <summary>
    /// 引用池强制检查类型。
    /// </summary>
    public enum ReferenceStrictCheckType : byte
    {
        /// <summary>始终开启。</summary>
        AlwaysEnable = 0,

        /// <summary>仅在 Debug 构建时开启。</summary>
        OnlyEnableWhenDevelopment,

        /// <summary>始终关闭。</summary>
        AlwaysDisable,
    }
}
