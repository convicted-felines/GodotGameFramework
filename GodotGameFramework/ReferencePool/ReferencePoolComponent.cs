//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 引用池组件。
    /// 封装 GameFramework 静态类 ReferencePool，提供引用对象的获取、归还、预热和清理能力。
    /// 在 Godot 场景树中作为 Node 存在，自动注册到 GameEntry。
    ///
    /// 使用步骤：
    ///   1. 在场景树中添加 ReferencePoolComponent 节点。
    ///   2. 让需要池化的类实现 IReference 接口并实现 Clear()。
    ///   3. 通过 Acquire&lt;T&gt;() 获取引用，通过 Release() 归还引用。
    /// </summary>
    public sealed partial class ReferencePoolComponent : GameFrameworkComponent
    {
        private ReferenceStrictCheckType m_EnableStrictCheck = ReferenceStrictCheckType.AlwaysDisable;

        /// <summary>获取或设置是否开启强制检查模式。</summary>
        public ReferenceStrictCheckType EnableStrictCheck
        {
            get => m_EnableStrictCheck;
            set
            {
                m_EnableStrictCheck = value;
                bool strictCheck = value switch
                {
                    ReferenceStrictCheckType.AlwaysEnable => true,
                    ReferenceStrictCheckType.OnlyEnableWhenDevelopment => IsDebugBuild(),
                    _ => false,
                };

                if (strictCheck)
                {
                    GameFrameworkLog.Warning("Strict checking is enabled for the Reference Pool. It will drastically affect the performance.");
                }

                ReferencePool.EnableStrictCheck = strictCheck;
            }
        }

        /// <summary>获取引用池数量。</summary>
        public int Count => ReferencePool.Count;

        public override void _Ready()
        {
            base._Ready();

            // 默认关闭严格检查，保持运行时性能
            ReferencePool.EnableStrictCheck = false;
        }

        // ── 获取 ───────────────────────────────────────────────────────────────

        /// <summary>从引用池获取引用。</summary>
        public T Acquire<T>() where T : class, IReference, new() =>
            ReferencePool.Acquire<T>();

        /// <summary>从引用池获取引用。</summary>
        public IReference Acquire(Type referenceType) =>
            ReferencePool.Acquire(referenceType);

        // ── 归还 ───────────────────────────────────────────────────────────────

        /// <summary>将引用归还引用池。</summary>
        public void Release(IReference reference) =>
            ReferencePool.Release(reference);

        // ── 预热 ───────────────────────────────────────────────────────────────

        /// <summary>向引用池中追加指定数量的引用。</summary>
        public void Add<T>(int count) where T : class, IReference, new() =>
            ReferencePool.Add<T>(count);

        /// <summary>向引用池中追加指定数量的引用。</summary>
        public void Add(Type referenceType, int count) =>
            ReferencePool.Add(referenceType, count);

        // ── 移除 ───────────────────────────────────────────────────────────────

        /// <summary>从引用池中移除指定数量的引用。</summary>
        public void Remove<T>(int count) where T : class, IReference =>
            ReferencePool.Remove<T>(count);

        /// <summary>从引用池中移除指定数量的引用。</summary>
        public void Remove(Type referenceType, int count) =>
            ReferencePool.Remove(referenceType, count);

        /// <summary>从引用池中移除所有引用。</summary>
        public void RemoveAll<T>() where T : class, IReference =>
            ReferencePool.RemoveAll<T>();

        /// <summary>从引用池中移除所有引用。</summary>
        public void RemoveAll(Type referenceType) =>
            ReferencePool.RemoveAll(referenceType);

        // ── 查询 ───────────────────────────────────────────────────────────────

        /// <summary>获取所有引用池的信息。</summary>
        public ReferencePoolInfo[] GetAllReferencePoolInfos() =>
            ReferencePool.GetAllReferencePoolInfos();

        // ── 清理 ───────────────────────────────────────────────────────────────

        /// <summary>清除所有引用池。</summary>
        public void ClearAll()
        {
            GameFrameworkLog.Info("Reference pool clear all.");
            ReferencePool.ClearAll();
        }

        // ── 内部 ───────────────────────────────────────────────────────────────

        private static bool IsDebugBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
