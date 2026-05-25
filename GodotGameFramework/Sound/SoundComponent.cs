using GameFramework;
using GameFramework.Resource;
using GameFramework.Sound;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 声音组件。封装 ISoundManager，提供多声音组、播放/暂停/停止等能力。
    ///
    /// 在 Inspector 中配置声音组后，每组自动创建若干 AudioStreamPlayer 代理节点。
    ///
    /// 使用示例：
    ///   var sound = GameEntry.GetComponent＜SoundComponent＞();
    ///   int id = sound.PlaySound("res://Audio/bgm.ogg", "Music");
    ///   sound.StopSound(id, fadeOutSeconds: 1f);
    /// </summary>
    public sealed partial class SoundComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>声音组名称列表。</summary>
        [Export] public string[] SoundGroupNames = Array.Empty<string>();

        /// <summary>声音辅助器。</summary>
        [Export] public SoundHelperType SoundHelper = SoundHelperType.SoundHelper;

        /// <summary>声音组辅助器。</summary>
        [Export] public SoundGroupHelperType SoundGroupHelper = SoundGroupHelperType.SoundGroupHelper;

        /// <summary>声音代理辅助器。</summary>
        [Export] public SoundAgentHelperType SoundAgentHelper = SoundAgentHelperType.SoundAgentHelper;

        /// <summary>各声音组代理数（每组可并发播放的声音数），与 SoundGroupNames 对应。</summary>
        [Export] public int[] SoundGroupAgentCounts = Array.Empty<int>();

        /// <summary>各声音组是否避免被同优先级声音替换（0=否, 1=是）。</summary>
        [Export] public int[] SoundGroupAvoidBeingReplaced = Array.Empty<int>();

        /// <summary>各声音组默认静音状态（0=否, 1=是）。</summary>
        [Export] public int[] SoundGroupMutes = Array.Empty<int>();

        /// <summary>各声音组默认音量（0~1）。</summary>
        [Export] public float[] SoundGroupVolumes = Array.Empty<float>();

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private ISoundManager m_SoundManager = null;

        private string SoundHelperTypeName => $"GodotGameFramework.{SoundHelper}";
        private string SoundGroupHelperTypeName => $"GodotGameFramework.{SoundGroupHelper}";
        private string SoundAgentHelperTypeName => $"GodotGameFramework.{SoundAgentHelper}";

        // ── 属性 ───────────────────────────────────────────────────────────────

        public int SoundGroupCount => m_SoundManager.SoundGroupCount;

        // ── 事件透传 ───────────────────────────────────────────────────────────

        public event EventHandler<PlaySoundSuccessEventArgs> PlaySoundSuccess
        {
            add => m_SoundManager.PlaySoundSuccess += value;
            remove => m_SoundManager.PlaySoundSuccess -= value;
        }

        public event EventHandler<PlaySoundFailureEventArgs> PlaySoundFailure
        {
            add => m_SoundManager.PlaySoundFailure += value;
            remove => m_SoundManager.PlaySoundFailure -= value;
        }

        public event EventHandler<PlaySoundUpdateEventArgs> PlaySoundUpdate
        {
            add => m_SoundManager.PlaySoundUpdate += value;
            remove => m_SoundManager.PlaySoundUpdate -= value;
        }

        public event EventHandler<PlaySoundDependencyAssetEventArgs> PlaySoundDependencyAsset
        {
            add => m_SoundManager.PlaySoundDependencyAsset += value;
            remove => m_SoundManager.PlaySoundDependencyAsset -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_SoundManager = GameFrameworkEntry.GetModule<ISoundManager>();
            if (m_SoundManager == null)
            {
                GameFrameworkLog.Fatal("Sound manager is invalid.");
                return;
            }

            // 注入资源管理器
            var resourceManager = ResourceComponent.Instance;
            if (resourceManager != null)
                m_SoundManager.SetResourceManager(resourceManager);

            // 注入声音辅助器（反射实例化，支持自定义扩展）
            var soundHelperType = GameFramework.Utility.Assembly.GetType(SoundHelperTypeName);
            if (soundHelperType == null || Activator.CreateInstance(soundHelperType) is not SoundHelperBase soundHelper)
            {
                GameFrameworkLog.Fatal($"Can not create sound helper '{SoundHelperTypeName}'.");
                return;
            }
            m_SoundManager.SetSoundHelper(soundHelper);

            // 注册 Inspector 中配置的声音组
            RegisterSoundGroupsFromExport();
        }

        // ── 声音组管理 ─────────────────────────────────────────────────────────

        public bool HasSoundGroup(string soundGroupName) =>
            m_SoundManager.HasSoundGroup(soundGroupName);

        public ISoundGroup GetSoundGroup(string soundGroupName) =>
            m_SoundManager.GetSoundGroup(soundGroupName);

        public ISoundGroup[] GetAllSoundGroups() =>
            m_SoundManager.GetAllSoundGroups();

        public void GetAllSoundGroups(List<ISoundGroup> results) =>
            m_SoundManager.GetAllSoundGroups(results);

        /// <summary>动态添加声音组（运行时调用）。</summary>
        public bool AddSoundGroup(string soundGroupName, int agentCount = 4,
            bool avoidBeingReplaced = false, bool mute = false, float volume = 1f)
        {
            if (m_SoundManager.HasSoundGroup(soundGroupName)) return false;

            var groupHelperType = GameFramework.Utility.Assembly.GetType(SoundGroupHelperTypeName);
            if (groupHelperType == null || Activator.CreateInstance(groupHelperType) is not SoundGroupHelperBase groupHelper)
            {
                GameFrameworkLog.Fatal($"Can not create sound group helper '{SoundGroupHelperTypeName}'.");
                return false;
            }
            bool added = m_SoundManager.AddSoundGroup(soundGroupName, avoidBeingReplaced, mute, volume, groupHelper);
            if (added) AddAgentsToGroup(soundGroupName, agentCount);
            return added;
        }

        // ── 播放控制 ───────────────────────────────────────────────────────────

        public int PlaySound(string soundAssetName, string soundGroupName) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName);

        public int PlaySound(string soundAssetName, string soundGroupName, int priority) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, priority);

        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, playSoundParams);

        public int PlaySound(string soundAssetName, string soundGroupName, object userData) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, userData);

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams);

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, object userData) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, priority, userData);

        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams, object userData) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, playSoundParams, userData);

        public int PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, object userData) =>
            m_SoundManager.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, userData);

        public bool StopSound(int serialId) => m_SoundManager.StopSound(serialId);
        public bool StopSound(int serialId, float fadeOutSeconds) => m_SoundManager.StopSound(serialId, fadeOutSeconds);

        public void StopAllLoadedSounds() => m_SoundManager.StopAllLoadedSounds();
        public void StopAllLoadedSounds(float fadeOutSeconds) => m_SoundManager.StopAllLoadedSounds(fadeOutSeconds);
        public void StopAllLoadingSounds() => m_SoundManager.StopAllLoadingSounds();

        public void PauseSound(int serialId) => m_SoundManager.PauseSound(serialId);
        public void PauseSound(int serialId, float fadeOutSeconds) => m_SoundManager.PauseSound(serialId, fadeOutSeconds);

        public void ResumeSound(int serialId) => m_SoundManager.ResumeSound(serialId);
        public void ResumeSound(int serialId, float fadeInSeconds) => m_SoundManager.ResumeSound(serialId, fadeInSeconds);

        // ── 查询 ───────────────────────────────────────────────────────────────

        public int[] GetAllLoadingSoundSerialIds() => m_SoundManager.GetAllLoadingSoundSerialIds();
        public void GetAllLoadingSoundSerialIds(List<int> results) => m_SoundManager.GetAllLoadingSoundSerialIds(results);
        public bool IsLoadingSound(int serialId) => m_SoundManager.IsLoadingSound(serialId);

        // ── 私有辅助 ───────────────────────────────────────────────────────────

        private void RegisterSoundGroupsFromExport()
        {
            int count = SoundGroupNames?.Length ?? 0;
            for (int i = 0; i < count; i++)
            {
                string groupName = SoundGroupNames[i];
                if (string.IsNullOrEmpty(groupName) || m_SoundManager.HasSoundGroup(groupName))
                    continue;

                int agentCount = i < SoundGroupAgentCounts.Length ? SoundGroupAgentCounts[i] : 4;
                bool avoidReplaced = i < SoundGroupAvoidBeingReplaced.Length && SoundGroupAvoidBeingReplaced[i] != 0;
                bool mute = i < SoundGroupMutes.Length && SoundGroupMutes[i] != 0;
                float volume = i < SoundGroupVolumes.Length ? SoundGroupVolumes[i] : 1f;

                var groupHelperType = GameFramework.Utility.Assembly.GetType(SoundGroupHelperTypeName);
                if (groupHelperType == null || Activator.CreateInstance(groupHelperType) is not SoundGroupHelperBase groupHelper)
                {
                    GameFrameworkLog.Fatal($"Can not create sound group helper '{SoundGroupHelperTypeName}'.");
                    continue;
                }
                bool added = m_SoundManager.AddSoundGroup(groupName, avoidReplaced, mute, volume, groupHelper);
                if (added) AddAgentsToGroup(groupName, agentCount);
            }
        }

        private void AddAgentsToGroup(string groupName, int agentCount)
        {
            var agentHelperType = GameFramework.Utility.Assembly.GetType(SoundAgentHelperTypeName);
            for (int j = 0; j < agentCount; j++)
            {
                if (agentHelperType == null || Activator.CreateInstance(agentHelperType) is not SoundAgentHelperBase agentHelper)
                {
                    GameFrameworkLog.Fatal($"Can not create sound agent helper '{SoundAgentHelperTypeName}'.");
                    return;
                }
                agentHelper.Name = $"SoundAgent_{groupName}_{j}";
                AddChild(agentHelper);
                m_SoundManager.AddSoundAgentHelper(groupName, agentHelper);
            }
        }
    }
}
