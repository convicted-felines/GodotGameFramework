namespace GameMain
{
    /// <summary>
    /// 施加 Effect 行为：向目标的 EffectContainer 添加指定 Effect。
    /// 典型用途：弹道命中时施加减速、灼烧等 Buff/Debuff，或减护甲装备在伤害前插入减护甲 Buff。
    /// </summary>
    public sealed class ApplyEffectAction : IAction
    {
        public string ActionId { get; }
        public int Priority { get; }

        private readonly IEffect _effect;

        public ApplyEffectAction(string id, IEffect effect, int priority = 50)
        {
            ActionId = id;
            Priority = priority;
            _effect = effect;
        }

        public void Execute(ActionContext context)
        {
            if (context.IsCancelled) return;
            context.TargetEffects.AddEffect(_effect);
        }
    }
}
