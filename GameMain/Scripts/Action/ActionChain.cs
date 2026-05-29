using System.Collections.Generic;

namespace GameMain
{
    /// <summary>
    /// 行为链：持有一组按 Priority 升序排列的 Action，顺序执行。
    /// 外部注入者（装备/Buff/技能）通过 Insert() 在任意位置插入自己的 Action，
    /// 通过 Remove() 在来源销毁时注销，不影响链中其他 Action。
    /// </summary>
    public sealed class ActionChain
    {
        private readonly List<IAction> _actions = new();

        public IReadOnlyList<IAction> Actions => _actions;

        /// <summary>按 Priority 升序插入，相同 Priority 的后插入者排在后面。</summary>
        public void Insert(IAction action)
        {
            int index = _actions.Count;
            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i].Priority > action.Priority)
                {
                    index = i;
                    break;
                }
            }
            _actions.Insert(index, action);
        }

        public void Remove(IAction action) => _actions.Remove(action);

        public void RemoveById(string actionId)
        {
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                if (_actions[i].ActionId == actionId)
                    _actions.RemoveAt(i);
            }
        }

        /// <summary>
        /// 顺序执行所有 Action。任一 Action 将 context.IsCancelled 设为 true 后中断执行。
        /// </summary>
        public void Execute(ActionContext context)
        {
            foreach (var action in _actions)
            {
                if (context.IsCancelled) break;
                action.Execute(context);
            }
        }
    }
}
