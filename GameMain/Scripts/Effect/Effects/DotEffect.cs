using System;

namespace GameMain
{
    /// <summary>
    /// 持续伤害（中毒/燃烧/流血），按秒触发扣血。
    /// IsExpired 为 true 时由 EffectContainer 自动移除。
    /// </summary>
    public sealed class DotEffect : EffectBase
    {
        private readonly float _damagePerTick;
        private readonly float _tickInterval;
        private readonly float _duration;
        private float _elapsed;
        private float _tickTimer;

        public override bool IsExpired => _elapsed >= _duration;

        public DotEffect(string id, float damagePerTick, float tickInterval, float duration)
        {
            EffectId = id;
            EffectType = EffectType.Timed;
            _damagePerTick = damagePerTick;
            _tickInterval = tickInterval;
            _duration = duration;
        }

        public override void OnTick(EffectContext context, float delta)
        {
            _elapsed += delta;
            _tickTimer += delta;

            while (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                ApplyTick(context);
            }
        }

        private void ApplyTick(EffectContext context)
        {
            var health = context.Attributes.Get(AttributeType.CurrentHealth);
            health.BaseValue = Math.Max(0f, health.BaseValue - _damagePerTick);
        }
    }
}
