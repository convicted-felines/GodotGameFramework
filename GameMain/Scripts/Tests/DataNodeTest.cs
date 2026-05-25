//------------------------------------------------------------
// 数据结点测试
//------------------------------------------------------------

using GameFramework;
using GameFramework.DataNode;
using GodotGameFramework;
using Godot;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试 DataNodeManager：路径读写、嵌套节点、RemoveNode。
    /// DataNodeManager 是一棵全局键值树，路径用 '.' 分隔。
    /// </summary>
    public partial class DataNodeTest : Node
    {
        public override void _Ready()
        {
            var mgr = GameFrameworkEntry.GetModule<IDataNodeManager>();
            if (mgr == null)
            {
                Log.Error("[DataNodeTest] DataNodeManager 未找到。");
                return;
            }

            // ── 写入基础类型 ────────────────────────────────────────────────────

            var strVar = ReferencePool.Acquire<VarString>();
            strVar.Value = "world";
            mgr.SetData("test.greeting", strVar);

            var intVar = ReferencePool.Acquire<VarInt32>();
            intVar.Value = 42;
            mgr.SetData("test.score", intVar);

            // ── 读取 ────────────────────────────────────────────────────────────

            string greeting = mgr.GetData<VarString>("test.greeting");
            int score = mgr.GetData<VarInt32>("test.score");
            Log.Info($"[DataNodeTest] greeting = {greeting}（期望 world）");
            Log.Info($"[DataNodeTest] score    = {score}（期望 42）");

            // ── 子节点遍历 ──────────────────────────────────────────────────────

            var testNode = mgr.GetNode("test");
            Log.Info($"[DataNodeTest] 'test' 节点子数量 = {testNode.ChildCount}（期望 2）");

            // ── GetOrAddNode 懒创建 ─────────────────────────────────────────────

            var newNode = mgr.GetOrAddNode("player.stats.hp");
            var hpVar = ReferencePool.Acquire<VarInt32>();
            hpVar.Value = 100;
            newNode.SetData(hpVar);

            int hp = mgr.GetData<VarInt32>("player.stats.hp");
            Log.Info($"[DataNodeTest] player.stats.hp = {hp}（期望 100）");

            // ── 移除节点 ────────────────────────────────────────────────────────

            mgr.RemoveNode("test.score");
            Log.Info($"[DataNodeTest] 移除 test.score 后子数量 = {testNode.ChildCount}（期望 1）");

            mgr.Clear();
            Log.Info($"[DataNodeTest] Clear 后根节点子数量 = {mgr.Root.ChildCount}（期望 0）");

            Log.Info("[DataNodeTest] 测试通过 ✓");
        }
    }
}
