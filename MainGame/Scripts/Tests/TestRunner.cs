//------------------------------------------------------------
// GodotGameFramework - 组件快速测试入口
//------------------------------------------------------------

using GodotGameFramework;
using Godot;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试运行器 — 挂载到独立测试场景的根节点。
    /// 在 Inspector 中选择要运行的测试模块，运行场景后查看 Output 面板。
    /// </summary>
    public partial class TestRunner : Node
    {
        public enum TestModule
        {
            Event,
            Fsm,
            ObjectPool,
            ReferencePool,
            DataNode,
            Setting,
        }

        [Export] public TestModule Module = TestModule.Event;

        public override void _Ready()
        {
            Log.Info($"========== TestRunner: {Module} ==========");

            switch (Module)
            {
                case TestModule.Event:        AddChild(new EventTest());        break;
                case TestModule.Fsm:          AddChild(new FsmTest());          break;
                case TestModule.ObjectPool:   AddChild(new ObjectPoolTest());   break;
                case TestModule.ReferencePool: AddChild(new ReferencePoolTest()); break;
                case TestModule.DataNode:     AddChild(new DataNodeTest());     break;
                case TestModule.Setting:      AddChild(new SettingTest());      break;
            }
        }
    }
}
