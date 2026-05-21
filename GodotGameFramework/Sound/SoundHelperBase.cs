//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Sound;

namespace GodotGameFramework
{
    /// <summary>
    /// 声音辅助器基类。继承此类实现自定义声音辅助器，在 SoundComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class SoundHelperBase : ISoundHelper
    {
        public abstract void ReleaseSoundAsset(object soundAsset);
    }
}
