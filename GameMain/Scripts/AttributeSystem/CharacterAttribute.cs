using System.Collections.Generic;

namespace GameMain
{
    /// <summary>
    /// 单个属性，支持固定值和百分比修饰器堆栈。
    /// 最终值 = (基础值 + sum(固定修饰)) * (1 + sum(百分比修饰))
    /// </summary>
    public sealed class CharacterAttribute
    {
        private float _baseValue;
        private readonly List<AttributeModifier> _flatModifiers = new();
        private readonly List<AttributeModifier> _percentModifiers = new();
        private float _cachedValue;
        private bool _isDirty = true;

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                _isDirty = true;
            }
        }

        public float Value
        {
            get
            {
                if (_isDirty) Recalculate();
                return _cachedValue;
            }
        }

        public void AddModifier(AttributeModifier modifier)
        {
            if (modifier.ModifierType == ModifierType.Flat)
                _flatModifiers.Add(modifier);
            else
                _percentModifiers.Add(modifier);
            _isDirty = true;
        }

        public bool RemoveModifier(AttributeModifier modifier)
        {
            bool removed = _flatModifiers.Remove(modifier) || _percentModifiers.Remove(modifier);
            if (removed) _isDirty = true;
            return removed;
        }

        public void RemoveAllFromSource(object source)
        {
            _flatModifiers.RemoveAll(m => m.Source == source);
            _percentModifiers.RemoveAll(m => m.Source == source);
            _isDirty = true;
        }

        private void Recalculate()
        {
            float flat = 0f;
            foreach (var mod in _flatModifiers) flat += mod.Value;

            float percent = 0f;
            foreach (var mod in _percentModifiers) percent += mod.Value;

            _cachedValue = (_baseValue + flat) * (1f + percent);
            _isDirty = false;
        }
    }
}
