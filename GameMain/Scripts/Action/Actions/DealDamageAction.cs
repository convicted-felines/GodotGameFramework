using System;

namespace GameMain
{
    /// <summary>
    /// 造成伤害行为：读取 context.DamageValue，扣除目标当前护甲后扣减 HP。
    /// Priority 默认 100，注入者使用更小的 Priority 可在伤害结算前执行。
    /// </summary>
    public sealed class DealDamageAction : IAction
    {
        public string ActionId { get; }
        public int Priority { get; }

        public DealDamageAction(string id = "deal_damage", int priority = 100)
        {
            ActionId = id;
            Priority = priority;
        }

        public void Execute(ActionContext context)
        {
            if (context.IsCancelled) return;

            float armor = context.TargetAttributes.GetValue(AttributeType.Armor);
            float damage = Math.Max(0f, context.DamageValue - armor);

            var health = context.TargetAttributes.Get(AttributeType.CurrentHealth);
            health.BaseValue = Math.Max(0f, health.BaseValue - damage);
        }
    }
}
