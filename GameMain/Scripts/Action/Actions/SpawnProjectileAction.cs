using Godot;
using System;

namespace GameMain
{
    /// <summary>
    /// 生成弹道实体行为：在指定位置生成子弹场景，并将行为注入器列表传递给子弹。
    /// 子弹命中时会构建一条新的 ActionChain，注入者可向该链注册自己的 Action。
    /// </summary>
    public sealed class SpawnProjectileAction : IAction
    {
        public string ActionId { get; }
        public int Priority { get; }

        private readonly PackedScene _projectileScene;
        private readonly Action<ActionChain> _chainConfigurator;

        /// <param name="id">行为标识。</param>
        /// <param name="projectileScene">子弹的 PackedScene。</param>
        /// <param name="chainConfigurator">
        ///     子弹命中时回调，用于向命中行为链注入额外 Action。
        ///     传入 null 则子弹只执行默认的 DealDamageAction。
        /// </param>
        /// <param name="priority">优先级，默认 0（先于伤害结算执行）。</param>
        public SpawnProjectileAction(
            string id,
            PackedScene projectileScene,
            Action<ActionChain> chainConfigurator = null,
            int priority = 0)
        {
            ActionId = id;
            Priority = priority;
            _projectileScene = projectileScene;
            _chainConfigurator = chainConfigurator;
        }

        public void Execute(ActionContext context)
        {
            if (context.IsCancelled) return;

            var bullet = _projectileScene.Instantiate<Node>();

            // 将命中链配置器注入到子弹，子弹命中时再执行
            if (bullet is IProjectile projectile)
            {
                var hitChain = new ActionChain();
                hitChain.Insert(new DealDamageAction());
                _chainConfigurator?.Invoke(hitChain);
                projectile.SetHitChain(hitChain, context);
            }

            context.Source.GetParent()?.AddChild(bullet);
        }
    }
}
