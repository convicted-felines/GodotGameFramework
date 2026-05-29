using System;

namespace GameMain
{
    /// <summary>
    /// 概率触发效果（Proc），每次调用 TryProc() 以设定概率激活内部 Effect。
    /// 典型用途：命中时 5% 概率触发震慑、冰冻等附加效果。
    /// </summary>
    public sealed class ProcEffect : EffectBase
    {
        private readonly float _procChance;
        private readonly IEffect _innerEffect;
        private readonly Random _rng = new();

        public ProcEffect(string id, float procChance, IEffect innerEffect)
        {
            EffectId = id;
            EffectType = EffectType.Triggered;
            _procChance = procChance;
            _innerEffect = innerEffect;
        }

        /// <summary>
        /// 在命中、暴击等事件处调用此方法尝试触发内部 Effect。
        /// 返回 true 表示本次成功触发。
        /// </summary>
        public bool TryProc(EffectContext context)
        {
            if (_rng.NextDouble() >= _procChance) return false;
            _innerEffect.OnApply(context);
            return true;
        }
    }
}
