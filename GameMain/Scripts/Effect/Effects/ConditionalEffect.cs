using System;

namespace GameMain
{
    /// <summary>
    /// 条件激活效果：每 Tick 检查条件，满足时激活内部 Effect，不满足时撤销。
    /// 典型用途：血量低于 30% 时 +50% 攻击力，满血时激活护盾。
    /// </summary>
    public sealed class ConditionalEffect : EffectBase
    {
        private readonly Func<EffectContext, bool> _condition;
        private readonly IEffect _innerEffect;
        private bool _isActive;

        public ConditionalEffect(string id, Func<EffectContext, bool> condition, IEffect innerEffect)
        {
            EffectId = id;
            EffectType = EffectType.Passive;
            _condition = condition;
            _innerEffect = innerEffect;
        }

        public override void OnTick(EffectContext context, float delta)
        {
            bool conditionMet = _condition(context);

            if (conditionMet && !_isActive)
            {
                _innerEffect.OnApply(context);
                _isActive = true;
            }
            else if (!conditionMet && _isActive)
            {
                _innerEffect.OnRemove(context);
                _isActive = false;
            }
        }

        public override void OnRemove(EffectContext context)
        {
            if (!_isActive) return;
            _innerEffect.OnRemove(context);
            _isActive = false;
        }
    }
}
