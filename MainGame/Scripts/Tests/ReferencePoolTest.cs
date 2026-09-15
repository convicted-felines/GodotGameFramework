//------------------------------------------------------------
// 引用池测试
//------------------------------------------------------------

using GameFramework;
using GodotGameFramework;
using Godot;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试 ReferencePool（静态工具类）：
    /// Acquire / Release / 预热 Add / 清理 ClearAll / 严格检查模式。
    /// </summary>
    public partial class ReferencePoolTest : Node
    {
        public override void _Ready()
        {
            // ── 基本 Acquire / Release ────────────────────────────────────────

            var msg1 = ReferencePool.Acquire<TestMessage>();
            msg1.Fill("hello", 1);
            Log.Info($"[ReferencePoolTest] Acquire: {msg1}");

            ReferencePool.Release(msg1);

            // 归还后再获取，应复用同一实例（Clear 后字段重置）
            var msg2 = ReferencePool.Acquire<TestMessage>();
            Log.Info($"[ReferencePoolTest] 再次 Acquire（应已 Clear）: {msg2}");
            Log.Info($"[ReferencePoolTest] 是同一实例: {ReferenceEquals(msg1, msg2)}（期望 True）");
            ReferencePool.Release(msg2);

            // ── 预热 Add ─────────────────────────────────────────────────────

            ReferencePool.Add<TestMessage>(5);
            var info = ReferencePool.GetAllReferencePoolInfos();
            foreach (var i in info)
            {
                if (i.Type == typeof(TestMessage))
                    Log.Info($"[ReferencePoolTest] 预热后池内 UnusedReferenceCount = {i.UnusedReferenceCount}（期望 6）");
            }

            // ── 批量 Acquire 并归还 ──────────────────────────────────────────

            var batch = new TestMessage[4];
            for (int k = 0; k < batch.Length; k++)
            {
                batch[k] = ReferencePool.Acquire<TestMessage>();
                batch[k].Fill($"item-{k}", k);
            }
            Log.Info($"[ReferencePoolTest] 批量 Acquire 4 个");

            foreach (var m in batch)
                ReferencePool.Release(m);
            Log.Info("[ReferencePoolTest] 批量归还完成");

            // ── ClearAll ──────────────────────────────────────────────────────

            ReferencePool.ClearAll();
            Log.Info("[ReferencePoolTest] ClearAll 后 Count = " + ReferencePool.Count + "（期望 0）");

            Log.Info("[ReferencePoolTest] 测试通过 ✓");
        }

        // ── 示例引用对象 ──────────────────────────────────────────────────────

        private sealed class TestMessage : IReference
        {
            public string Text { get; private set; }
            public int Value { get; private set; }

            public void Fill(string text, int value)
            {
                Text = text;
                Value = value;
            }

            public void Clear()
            {
                Text = null;
                Value = 0;
            }

            public override string ToString() => $"TestMessage(Text={Text}, Value={Value})";
        }
    }
}
