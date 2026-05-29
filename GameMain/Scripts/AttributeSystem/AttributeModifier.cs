namespace GameMain
{
    public enum ModifierType
    {
        Flat,
        Percent,
    }

    public sealed class AttributeModifier
    {
        public float Value { get; }
        public ModifierType ModifierType { get; }
        public object Source { get; }

        public AttributeModifier(float value, ModifierType type, object source)
        {
            Value = value;
            ModifierType = type;
            Source = source;
        }
    }
}
