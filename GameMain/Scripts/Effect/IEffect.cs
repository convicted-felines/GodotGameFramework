namespace GameMain
{
    public enum EffectType
    {
        Passive,    // 装备/天赋等持续生效，跟随装备卸下
        Timed,      // 有持续时间，结束后自动移除
        Triggered,  // 由外部事件驱动触发（如 Proc、技能命中）
    }

    public interface IEffect
    {
        string EffectId { get; }
        EffectType EffectType { get; }
        bool IsExpired { get; }

        void OnApply(EffectContext context);
        void OnRemove(EffectContext context);
        void OnTick(EffectContext context, float delta);
    }
}
