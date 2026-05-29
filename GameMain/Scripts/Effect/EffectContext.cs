namespace GameMain
{
    /// <summary>
    /// Effect 执行时的上下文，包含目标的属性容器、Effect 容器和效果来源。
    /// </summary>
    public sealed class EffectContext
    {
        public AttributeContainer Attributes { get; }

        /// <summary>宿主的 EffectContainer，供 Effect 在执行时向目标添加/移除其他 Effect。</summary>
        public EffectContainer Effects { get; }

        /// <summary>施加此效果的来源（装备实例、技能实例等），用于溯源移除。</summary>
        public object Source { get; }

        public EffectContext(AttributeContainer attributes, EffectContainer effects, object source)
        {
            Attributes = attributes;
            Effects = effects;
            Source = source;
        }
    }
}
