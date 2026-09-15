//------------------------------------------------------------
// 事件系统测试
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GodotGameFramework;
using Godot;
using System;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试 EventComponent：订阅、Fire（延迟）、FireNow（立即）、取消订阅。
    /// </summary>
    public partial class EventTest : Node
    {
        // 自定义事件 ID（避免与框架内置 ID 冲突，从 10000 起）
        private const int EventId_Ping = 10001;
        private const int EventId_Data = 10002;

        private EventComponent m_Event;
        private int m_PingCount;
        private bool m_DataReceived;

        public override void _Ready()
        {
            m_Event = GodotGameFramework.GameEntry.GetComponent<EventComponent>();
            if (m_Event == null)
            {
                Log.Error("[EventTest] EventComponent 未找到，请确保场景中存在 BaseComponent + EventComponent。");
                return;
            }

            // 订阅
            m_Event.Subscribe(EventId_Ping, OnPing);
            m_Event.Subscribe(EventId_Data, OnData);

            Log.Info("[EventTest] 已订阅 Ping 和 Data 事件");

            // FireNow：立即同步触发
            m_Event.FireNow(this, PingEventArgs.Create());
            Log.Info($"[EventTest] FireNow 后 PingCount = {m_PingCount}（期望 1）");

            // Fire：异步，下一帧触发
            m_Event.Fire(this, PingEventArgs.Create());
            m_Event.Fire(this, DataEventArgs.Create("hello-event"));

            Log.Info("[EventTest] Fire 已投递，等待下一帧...");
        }

        public override void _Process(double delta)
        {
            // 事件在 GameFrameworkEntry.Update 之后分发，_Process 中就能收到
            if (m_PingCount == 2 && m_DataReceived)
            {
                Log.Info("[EventTest] 全部事件收到，测试通过 ✓");

                // 取消订阅后再 Fire 不应再触发
                m_Event.Unsubscribe(EventId_Ping, OnPing);
                m_Event.Fire(this, PingEventArgs.Create());
                Log.Info("[EventTest] 取消订阅后 Fire，PingCount 应保持 2");
                SetProcess(false);
            }
        }

        private void OnPing(object sender, GameEventArgs e)
        {
            m_PingCount++;
            Log.Info($"[EventTest] OnPing #{m_PingCount}");
        }

        private void OnData(object sender, GameEventArgs e)
        {
            if (e is DataEventArgs data)
            {
                m_DataReceived = true;
                Log.Info($"[EventTest] OnData: {data.Payload}");
            }
        }

        // ── 轻量 EventArgs，使用 ReferencePool 回收 ──────────────────────────

        private sealed class PingEventArgs : GameEventArgs
        {
            public override int Id => EventId_Ping;

            public static PingEventArgs Create()
            {
                var e = ReferencePool.Acquire<PingEventArgs>();
                return e;
            }

            public override void Clear() { }
        }

        private sealed class DataEventArgs : GameEventArgs
        {
            public override int Id => EventId_Data;
            public string Payload { get; private set; }

            public static DataEventArgs Create(string payload)
            {
                var e = ReferencePool.Acquire<DataEventArgs>();
                e.Payload = payload;
                return e;
            }

            public override void Clear() => Payload = null;
        }
    }
}
