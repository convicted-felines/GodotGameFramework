namespace GameMain
{
    public abstract class EffectBase : IEffect
    {
        public string EffectId { get; protected set; }
        public EffectType EffectType { get; protected set; }
        public virtual bool IsExpired => false;

        public virtual void OnApply(EffectContext context) { }
        public virtual void OnRemove(EffectContext context) { }
        public virtual void OnTick(EffectContext context, float delta) { }
    }
}
