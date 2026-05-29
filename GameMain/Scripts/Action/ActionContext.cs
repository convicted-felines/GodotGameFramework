using Godot;

namespace GameMain
{
    /// <summary>
    /// 行为执行上下文，携带本次行为所需的所有运行时数据。
    /// 行为链中的每个 Action 都读写同一个 Context，实现数据的链式传递。
    /// </summary>
    public sealed class ActionContext
    {
        /// <summary>行为发起者实体。</summary>
        public Node Source { get; }

        /// <summary>行为目标实体。</summary>
        public Node Target { get; }

        /// <summary>发起者的属性容器。</summary>
        public AttributeContainer SourceAttributes { get; }

        /// <summary>目标的属性容器。</summary>
        public AttributeContainer TargetAttributes { get; }

        /// <summary>目标的 EffectContainer，用于向目标施加 Buff/Debuff。</summary>
        public EffectContainer TargetEffects { get; }

        /// <summary>
        /// 可在行为链中流转的中间伤害值。
        /// 前置 Action（如减护甲）可修改此值，后续 Action（如造成伤害）读取最终结果。
        /// </summary>
        public float DamageValue { get; set; }

        /// <summary>标记行为链是否被某个 Action 中断，中断后后续 Action 不再执行。</summary>
        public bool IsCancelled { get; set; }

        public ActionContext(
            Node source,
            Node target,
            AttributeContainer sourceAttributes,
            AttributeContainer targetAttributes,
            EffectContainer targetEffects,
            float damageValue = 0f)
        {
            Source = source;
            Target = target;
            SourceAttributes = sourceAttributes;
            TargetAttributes = targetAttributes;
            TargetEffects = targetEffects;
            DamageValue = damageValue;
        }
    }
}
