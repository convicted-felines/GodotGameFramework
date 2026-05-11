//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 事件组件。封装 IEventManager，提供订阅/发布游戏事件的能力。
    /// </summary>
    public sealed partial class EventComponent : GameFrameworkComponent
    {
        private IEventManager m_EventManager = null;

        public int EventHandlerCount => m_EventManager.EventHandlerCount;

        public int EventCount => m_EventManager.EventCount;

        public override void _Ready()
        {
            base._Ready();

            m_EventManager = GameFrameworkEntry.GetModule<IEventManager>();
            if (m_EventManager == null)
            {
                GameFrameworkLog.Fatal("Event manager is invalid.");
                return;
            }
        }

        public int Count(int id) => m_EventManager.Count(id);

        public bool Check(int id, EventHandler<GameEventArgs> handler) => m_EventManager.Check(id, handler);

        public void Subscribe(int id, EventHandler<GameEventArgs> handler) => m_EventManager.Subscribe(id, handler);

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler) => m_EventManager.Unsubscribe(id, handler);

        public void SetDefaultHandler(EventHandler<GameEventArgs> handler) => m_EventManager.SetDefaultHandler(handler);

        /// <summary>线程安全，下一帧分发。</summary>
        public void Fire(object sender, GameEventArgs e) => m_EventManager.Fire(sender, e);

        /// <summary>立即同步分发，非线程安全。</summary>
        public void FireNow(object sender, GameEventArgs e) => m_EventManager.FireNow(sender, e);
    }
}
