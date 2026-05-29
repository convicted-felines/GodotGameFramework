namespace GameMain
{
    /// <summary>
    /// 弹道实体实现此接口，用于接收命中时的行为链和上下文。
    /// SpawnProjectileAction 生成子弹时调用 SetHitChain，子弹命中目标时调用 chain.Execute(context)。
    /// </summary>
    public interface IProjectile
    {
        void SetHitChain(ActionChain chain, ActionContext sourceContext);
    }
}
