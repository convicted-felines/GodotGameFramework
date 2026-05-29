namespace GameMain
{
    /// <summary>
    /// 行为接口：游戏中所有可发生的事件（攻击、施法、被击、造成伤害等）都实现此接口。
    /// Priority 数值越小越先执行，外部注入方通过选择合适的 Priority 插入到目标行为前后。
    /// </summary>
    public interface IAction
    {
        string ActionId { get; }
        int Priority { get; }

        void Execute(ActionContext context);
    }
}
