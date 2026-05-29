namespace GameMain
{
    /// <summary>固定值加成：给指定属性添加 +N 的平坦修饰。</summary>
    public sealed class FlatStatEffect : EffectBase
    {
        private readonly AttributeType _target;
        private readonly float _value;
        private AttributeModifier _modifier;

        public FlatStatEffect(string id, AttributeType target, float value)
        {
            EffectId = id;
            EffectType = EffectType.Passive;
            _target = target;
            _value = value;
        }

        public override void OnApply(EffectContext context)
        {
            _modifier = new AttributeModifier(_value, ModifierType.Flat, this);
            context.Attributes.AddModifier(_target, _modifier);
        }

        public override void OnRemove(EffectContext context)
        {
            context.Attributes.RemoveModifier(_target, _modifier);
        }
    }
}
