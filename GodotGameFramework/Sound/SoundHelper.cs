using GameFramework.Sound;

namespace GodotGameFramework
{
    /// <summary>
    /// 声音辅助器。负责释放声音资源（Godot 中资源由 GC 管理，无需显式释放）。
    /// </summary>
    public sealed class SoundHelper : SoundHelperBase
    {
        public override void ReleaseSoundAsset(object soundAsset)
        {
            // Godot AudioStream 由 GC 管理，不需要显式释放。
        }
    }
}
