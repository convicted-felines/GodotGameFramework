//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Sound;
using Godot;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 声音代理辅助器基类。继承此类实现自定义声音代理，在 SoundComponent 的 Inspector 中填写完整类名即可生效。
    /// 辅助器会被作为子节点挂载到 SoundComponent 下，可直接使用 Godot Node API（AddChild、CreateTween 等）。
    /// </summary>
    public abstract partial class SoundAgentHelperBase : Node, ISoundAgentHelper
    {
        public abstract event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent;

        public abstract bool IsPlaying { get; }
        public abstract float Length { get; }
        public abstract float Time { get; set; }
        public abstract bool Mute { get; set; }
        public abstract bool Loop { get; set; }
        public abstract int Priority { get; set; }
        public abstract float Volume { get; set; }
        public abstract float Pitch { get; set; }
        public abstract float PanStereo { get; set; }
        public abstract float SpatialBlend { get; set; }
        public abstract float MaxDistance { get; set; }
        public abstract float DopplerLevel { get; set; }

        public abstract void Play(float fadeInSeconds);
        public abstract void Stop(float fadeOutSeconds);
        public abstract void Pause(float fadeOutSeconds);
        public abstract void Resume(float fadeInSeconds);
        public abstract void Reset();
        public abstract bool SetSoundAsset(object soundAsset);
    }
}
