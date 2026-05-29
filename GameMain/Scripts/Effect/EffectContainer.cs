using System.Collections.Generic;

namespace GameMain
{
    /// <summary>
    /// 挂载在角色身上，管理所有活跃 Effect 的生命周期。
    /// 每帧调用 Tick() 驱动有时限的效果，自动清除已过期项。
    /// </summary>
    public sealed class EffectContainer
    {
        private readonly List<IEffect> _effects = new();
        private readonly EffectContext _context;

        public IReadOnlyList<IEffect> Effects => _effects;

        public EffectContainer(AttributeContainer attributes, object owner)
        {
            _context = new EffectContext(attributes, this, owner);
        }

        public void AddEffect(IEffect effect)
        {
            _effects.Add(effect);
            effect.OnApply(_context);
        }

        public void RemoveEffect(IEffect effect)
        {
            if (_effects.Remove(effect))
                effect.OnRemove(_context);
        }

        public void RemoveById(string effectId)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].EffectId != effectId) continue;
                _effects[i].OnRemove(_context);
                _effects.RemoveAt(i);
            }
        }

        public bool HasEffect(string effectId) => _effects.Exists(e => e.EffectId == effectId);

        public void Tick(float delta)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                effect.OnTick(_context, delta);
                if (!effect.IsExpired) continue;
                effect.OnRemove(_context);
                _effects.RemoveAt(i);
            }
        }

        public void Clear()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
                _effects[i].OnRemove(_context);
            _effects.Clear();
        }
    }
}
