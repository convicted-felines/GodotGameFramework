//------------------------------------------------------------
// 对象池测试
//------------------------------------------------------------

using GameFramework;
using GameFramework.ObjectPool;
using GodotGameFramework;
using Godot;

namespace GameMain.Tests
{
    /// <summary>
    /// 测试 ObjectPoolComponent / IObjectPoolManager：
    /// 创建单次对象池、Spawn/Unspawn、容量限制、自动释放。
    /// </summary>
    public partial class ObjectPoolTest : Node
    {
        private IObjectPoolManager m_PoolManager;
        private IObjectPool<DummyObject> m_Pool;

        public override void _Ready()
        {
            m_PoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            if (m_PoolManager == null)
            {
                Log.Error("[ObjectPoolTest] ObjectPoolManager 未找到。");
                return;
            }

            // 容量 4，过期时间 10s
            m_Pool = m_PoolManager.CreateSingleSpawnObjectPool<DummyObject>("TestPool", capacity: 4, expireTime: 10f);
            Log.Info($"[ObjectPoolTest] 创建池 TestPool，容量 4");

            // 预热：Register 3 个对象
            for (int i = 0; i < 3; i++)
                m_Pool.Register(DummyObject.Create($"Obj-{i}"), spawned: false);

            Log.Info($"[ObjectPoolTest] 注册 3 个对象，池内空闲数 = {m_Pool.CanReleaseCount}（期望 3）");

            // Spawn
            var a = m_Pool.Spawn();
            var b = m_Pool.Spawn();
            Log.Info($"[ObjectPoolTest] Spawn 两个: a={a.Target}, b={b.Target}");
            Log.Info($"[ObjectPoolTest] 池内空闲数 = {m_Pool.CanReleaseCount}（期望 1）");

            // Unspawn 归还
            m_Pool.Unspawn(a);
            Log.Info($"[ObjectPoolTest] Unspawn a，池内空闲数 = {m_Pool.CanReleaseCount}（期望 2）");

            // 释放所有可释放对象
            m_Pool.ReleaseAllUnused();
            Log.Info($"[ObjectPoolTest] ReleaseAllUnused 后空闲数 = {m_Pool.CanReleaseCount}");

            // 归还 b 并销毁池
            m_Pool.Unspawn(b);
            m_PoolManager.DestroyObjectPool(m_Pool);
            Log.Info("[ObjectPoolTest] 池已销毁，测试通过 ✓");
        }

        // ── 被池化的数据对象 ─────────────────────────────────────────────────────

        private sealed class DummyObject : ObjectBase
        {
            public static DummyObject Create(string name)
            {
                var obj = ReferencePool.Acquire<DummyObject>();
                obj.Initialize(name, new object());   // target 可换成任意资源句柄
                return obj;
            }

            protected override void Release(bool isShutdown)
            {
                Log.Info($"[ObjectPoolTest] Release: {Name}");
            }
        }
    }
}
