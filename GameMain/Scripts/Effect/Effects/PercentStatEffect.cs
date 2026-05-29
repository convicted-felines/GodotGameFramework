namespace GameMain
{
    /// <summary>百分比加成：给指定属性添加 +N% 的百分比修饰。</summary>
    public sealed class PercentStatEffect : EffectBase
    {
        private readonly AttributeType _target;
        private readonly float _percent;
        private AttributeModifier _modifier;

        public PercentStatEffect(string id, AttributeType target, float percent)
        {
            EffectId = id;
            EffectType = EffectType.Passive;
            _target = target;
            _percent = percent;
        }

        public override void OnApply(EffectContext context)
        {
            _modifier = new AttributeModifier(_percent, ModifierType.Percent, this);
            context.Attributes.AddModifier(_target, _modifier);
        }

        public override void OnRemove(EffectContext context)
        {
            context.Attributes.RemoveModifier(_target, _modifier);
        }
    }
}
