using GameFramework.Sound;
using Godot;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 默认声音代理辅助器。将 ISoundAgentHelper 映射到 Godot AudioStreamPlayer。
    /// 每个实例对应场景树中一个 AudioStreamPlayer 节点（2D 音效）或 AudioStreamPlayer3D 节点（3D 空间音效）。
    /// </summary>
    public sealed partial class SoundAgentHelper : SoundAgentHelperBase
    {
        private AudioStreamPlayer m_Player2D;
        private AudioStreamPlayer3D m_Player3D;

        private bool m_Is3D;
        private bool m_Mute;
        private bool m_Loop;
        private int m_Priority;
        private float m_Volume = 1f;
        private float m_Pitch = 1f;
        private float m_PanStereo;
        private float m_SpatialBlend;
        private float m_MaxDistance = 100f;
        private float m_DopplerLevel = 1f;

        private Tween m_FadeTween;

        public override event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent;

        public override bool IsPlaying => m_Is3D ? (m_Player3D?.Playing ?? false) : (m_Player2D?.Playing ?? false);

        public override float Length
        {
            get
            {
                AudioStream stream = m_Is3D ? m_Player3D?.Stream : m_Player2D?.Stream;
                return stream is AudioStream s ? (float)s.GetLength() : 0f;
            }
        }

        public override float Time
        {
            get => m_Is3D ? (float)(m_Player3D?.GetPlaybackPosition() ?? 0f) : (float)(m_Player2D?.GetPlaybackPosition() ?? 0f);
            set
            {
                if (m_Is3D) m_Player3D?.Seek(value);
                else m_Player2D?.Seek(value);
            }
        }

        public override bool Mute
        {
            get => m_Mute;
            set { m_Mute = value; ApplyVolume(); }
        }

        public override bool Loop
        {
            get => m_Loop;
            set { m_Loop = value; ApplyLoopToStream(); }
        }

        public override int Priority { get => m_Priority; set => m_Priority = value; }

        public override float Volume
        {
            get => m_Volume;
            set { m_Volume = value; ApplyVolume(); }
        }

        public override float Pitch
        {
            get => m_Pitch;
            set
            {
                m_Pitch = value;
                if (m_Is3D && m_Player3D != null) m_Player3D.PitchScale = value;
                else if (m_Player2D != null) m_Player2D.PitchScale = value;
            }
        }

        public override float PanStereo
        {
            get => m_PanStereo;
            set => m_PanStereo = value;
        }

        public override float SpatialBlend
        {
            get => m_SpatialBlend;
            set => m_SpatialBlend = value;
        }

        public override float MaxDistance
        {
            get => m_MaxDistance;
            set
            {
                m_MaxDistance = value;
                if (m_Player3D != null) m_Player3D.MaxDistance = value;
            }
        }

        public override float DopplerLevel
        {
            get => m_DopplerLevel;
            set
            {
                m_DopplerLevel = value;
                if (m_Player3D != null) m_Player3D.DopplerTracking =
                    value > 0f ? AudioStreamPlayer3D.DopplerTrackingEnum.PhysicsStep
                               : AudioStreamPlayer3D.DopplerTrackingEnum.Disabled;
            }
        }

        public override void Play(float fadeInSeconds)
        {
            EnsurePlayer();
            ApplyAllParams();

            if (m_Is3D) m_Player3D.Play();
            else m_Player2D.Play();

            if (fadeInSeconds > 0f)
            {
                SetVolumeImmediate(0f);
                FadeVolume(m_Volume, fadeInSeconds);
            }
        }

        public override void Stop(float fadeOutSeconds)
        {
            if (fadeOutSeconds > 0f)
                FadeVolume(0f, fadeOutSeconds, stopAfter: true);
            else
                StopPlayer();
        }

        public override void Pause(float fadeOutSeconds)
        {
            if (fadeOutSeconds > 0f)
            {
                FadeVolume(0f, fadeOutSeconds, pauseAfter: true);
            }
            else
            {
                if (m_Is3D) m_Player3D.StreamPaused = true;
                else m_Player2D.StreamPaused = true;
            }
        }

        public override void Resume(float fadeInSeconds)
        {
            if (m_Is3D) m_Player3D.StreamPaused = false;
            else m_Player2D.StreamPaused = false;

            if (fadeInSeconds > 0f)
            {
                SetVolumeImmediate(0f);
                FadeVolume(m_Volume, fadeInSeconds);
            }
        }

        public override void Reset()
        {
            StopPlayer();
            if (m_Player2D != null) m_Player2D.Stream = null;
            if (m_Player3D != null) m_Player3D.Stream = null;
            m_Mute = false;
            m_Loop = false;
            m_Priority = 0;
            m_Volume = 1f;
            m_Pitch = 1f;
            m_PanStereo = 0f;
            m_SpatialBlend = 0f;
            m_MaxDistance = 100f;
            m_DopplerLevel = 1f;
        }

        public override bool SetSoundAsset(object soundAsset)
        {
            if (soundAsset is not AudioStream stream) return false;

            bool need3D = m_SpatialBlend > 0f;
            EnsurePlayer(need3D);

            if (m_Is3D) m_Player3D.Stream = stream;
            else m_Player2D.Stream = stream;

            ApplyLoopToStream();
            return true;
        }

        private void EnsurePlayer(bool? force3D = null)
        {
            bool use3D = force3D ?? m_SpatialBlend > 0f;

            if (use3D && m_Player3D == null)
            {
                m_Player3D = new AudioStreamPlayer3D();
                AddChild(m_Player3D);
                m_Is3D = true;
            }
            else if (!use3D && m_Player2D == null)
            {
                m_Player2D = new AudioStreamPlayer();
                m_Player2D.Finished += OnFinished;
                AddChild(m_Player2D);
                m_Is3D = false;
            }
        }

        private void OnFinished()
        {
            if (m_Loop) m_Player2D?.Play();
        }

        private void StopPlayer()
        {
            m_FadeTween?.Kill();
            if (m_Is3D) m_Player3D?.Stop();
            else m_Player2D?.Stop();
        }

        private void ApplyVolume()
        {
            float effective = m_Mute ? 0f : m_Volume;
            SetVolumeImmediate(effective);
        }

        private void SetVolumeImmediate(float linear)
        {
            float db = linear <= 0f ? -80f : Mathf.LinearToDb(linear);
            if (m_Is3D && m_Player3D != null) m_Player3D.VolumeDb = db;
            else if (m_Player2D != null) m_Player2D.VolumeDb = db;
        }

        private void FadeVolume(float targetLinear, float seconds, bool stopAfter = false, bool pauseAfter = false)
        {
            m_FadeTween?.Kill();
            m_FadeTween = CreateTween();
            Node player = m_Is3D ? (Node)m_Player3D : m_Player2D;
            if (player == null) return;

            float targetDb = targetLinear <= 0f ? -80f : Mathf.LinearToDb(targetLinear);
            m_FadeTween.TweenProperty(player, "volume_db", targetDb, seconds);

            if (stopAfter)
                m_FadeTween.TweenCallback(Callable.From(StopPlayer));
            else if (pauseAfter)
                m_FadeTween.TweenCallback(Callable.From(() =>
                {
                    if (m_Is3D) m_Player3D.StreamPaused = true;
                    else m_Player2D.StreamPaused = true;
                }));
        }

        private void ApplyAllParams()
        {
            ApplyVolume();
            Pitch = m_Pitch;
            MaxDistance = m_MaxDistance;
            DopplerLevel = m_DopplerLevel;
        }

        private void ApplyLoopToStream()
        {
            AudioStream stream = m_Is3D ? m_Player3D?.Stream : m_Player2D?.Stream;
            if (stream is AudioStreamWav wav) wav.LoopMode = m_Loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;
            else if (stream is AudioStreamOggVorbis ogg) ogg.Loop = m_Loop;
            else if (stream is AudioStreamMP3 mp3) mp3.Loop = m_Loop;
        }
    }
}
