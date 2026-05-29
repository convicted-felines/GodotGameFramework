using System.Collections.Generic;

namespace GameMain
{
    public sealed class AttributeContainer
    {
        private readonly Dictionary<AttributeType, CharacterAttribute> _attributes = new();

        public CharacterAttribute Get(AttributeType type)
        {
            if (!_attributes.TryGetValue(type, out var attr))
            {
                attr = new CharacterAttribute();
                _attributes[type] = attr;
            }
            return attr;
        }

        public float GetValue(AttributeType type) => Get(type).Value;

        public void SetBase(AttributeType type, float value) => Get(type).BaseValue = value;

        public void AddModifier(AttributeType type, AttributeModifier modifier) =>
            Get(type).AddModifier(modifier);

        public void RemoveModifier(AttributeType type, AttributeModifier modifier) =>
            Get(type).RemoveModifier(modifier);

        public void RemoveAllFromSource(object source)
        {
            foreach (var attr in _attributes.Values)
                attr.RemoveAllFromSource(source);
        }
    }
}
