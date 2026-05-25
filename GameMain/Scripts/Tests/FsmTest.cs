//------------------------------------------------------------
// FSM 系统测试
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GodotGameFramework;
using Godot;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试 FsmManager：创建 FSM、状态切换、数据传递、销毁。
    /// FSM 的 Owner 是本测试类自身（作为普通 C# 对象）。
    /// </summary>
    public partial class FsmTest : Node
    {
        private IFsmManager m_FsmManager;
        private IFsm<FsmTest> m_Fsm;

        public override void _Ready()
        {
            m_FsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
            if (m_FsmManager == null)
            {
                Log.Error("[FsmTest] FsmManager 未找到，BaseComponent 未初始化？");
                return;
            }

            m_Fsm = m_FsmManager.CreateFsm("TestFsm", this,
                new StateIdle(),
                new StateRunning(),
                new StateDone());

            m_Fsm.Start<StateIdle>();
            Log.Info($"[FsmTest] FSM 已启动，当前状态: {m_Fsm.CurrentState?.GetType().Name}（期望 StateIdle）");
        }

        public override void _Process(double delta)
        {
            if (m_Fsm == null || m_Fsm.IsDestroyed) return;

            // FSM 由 GameFrameworkEntry.Update 驱动，这里只观察结果
            Log.Info($"[FsmTest] _Process: 当前状态 = {m_Fsm.CurrentState?.GetType().Name}");

            if (m_Fsm.CurrentState is StateDone)
            {
                Log.Info("[FsmTest] 已到达 Done 状态，测试通过 ✓ 销毁 FSM");
                m_FsmManager.DestroyFsm(m_Fsm);
                SetProcess(false);
            }
        }

        // ── 状态定义 ─────────────────────────────────────────────────────────────

        private class StateIdle : FsmState<FsmTest>
        {
            private float m_Timer;

            protected override void OnEnter(IFsm<FsmTest> fsm)
            {
                m_Timer = 0f;
                Log.Info("[FsmTest] → Idle: 等待 0.5s 后切换到 Running");
            }

            protected override void OnUpdate(IFsm<FsmTest> fsm, float elapseSeconds, float realElapseSeconds)
            {
                m_Timer += realElapseSeconds;
                if (m_Timer >= 0.5f)
                    ChangeState<StateRunning>(fsm);
            }

            protected override void OnLeave(IFsm<FsmTest> fsm, bool isShutdown)
            {
                Log.Info("[FsmTest] ← Idle");
            }
        }

        private class StateRunning : FsmState<FsmTest>
        {
            private int m_TickCount;

            protected override void OnEnter(IFsm<FsmTest> fsm)
            {
                m_TickCount = 0;
                // 通过 FSM 数据槽在状态间传值
                fsm.SetData<VarInt32>("TickCount", 0);
                Log.Info("[FsmTest] → Running: 累计 3 帧后切换到 Done");
            }

            protected override void OnUpdate(IFsm<FsmTest> fsm, float elapseSeconds, float realElapseSeconds)
            {
                m_TickCount++;
                fsm.SetData<VarInt32>("TickCount", m_TickCount);
                Log.Info($"[FsmTest] Running tick #{m_TickCount}");
                if (m_TickCount >= 3)
                    ChangeState<StateDone>(fsm);
            }

            protected override void OnLeave(IFsm<FsmTest> fsm, bool isShutdown)
            {
                Log.Info($"[FsmTest] ← Running (共 tick {m_TickCount} 次)");
            }
        }

        private class StateDone : FsmState<FsmTest>
        {
            protected override void OnEnter(IFsm<FsmTest> fsm)
            {
                int ticks = fsm.GetData<VarInt32>("TickCount");
                Log.Info($"[FsmTest] → Done: 从数据槽读到 TickCount = {ticks}（期望 3）");
            }
        }
    }
}
