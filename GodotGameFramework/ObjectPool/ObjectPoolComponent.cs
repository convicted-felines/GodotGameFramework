//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.ObjectPool;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 对象池组件。
    /// 封装 IObjectPoolManager，提供对象池的创建、查询、销毁和释放能力。
    /// 在 Godot 场景树中作为 Node 存在，自动注册到 GameEntry。
    ///
    /// 使用步骤：
    ///   1. 在 BaseComponent 节点下（或之后）添加 ObjectPoolComponent 节点。
    ///   2. 通过 CreateSingleSpawnObjectPool / CreateMultiSpawnObjectPool 创建对象池。
    ///   3. 调用 Spawn / Unspawn 获取和回收对象。
    /// </summary>
    public sealed partial class ObjectPoolComponent : GameFrameworkComponent
    {
        private IObjectPoolManager m_ObjectPoolManager = null;

        /// <summary>获取对象池数量。</summary>
        public int Count => m_ObjectPoolManager.Count;

        public override void _Ready()
        {
            base._Ready();

            m_ObjectPoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            if (m_ObjectPoolManager == null)
            {
                GameFrameworkLog.Fatal("Object pool manager is invalid.");
            }
        }

        // ── 检查 ───────────────────────────────────────────────────────────────

        public bool HasObjectPool<T>() where T : ObjectBase =>
            m_ObjectPoolManager.HasObjectPool<T>();

        public bool HasObjectPool(Type objectType) =>
            m_ObjectPoolManager.HasObjectPool(objectType);

        public bool HasObjectPool<T>(string name) where T : ObjectBase =>
            m_ObjectPoolManager.HasObjectPool<T>(name);

        public bool HasObjectPool(Type objectType, string name) =>
            m_ObjectPoolManager.HasObjectPool(objectType, name);

        public bool HasObjectPool(Predicate<ObjectPoolBase> condition) =>
            m_ObjectPoolManager.HasObjectPool(condition);

        // ── 获取 ───────────────────────────────────────────────────────────────

        public IObjectPool<T> GetObjectPool<T>() where T : ObjectBase =>
            m_ObjectPoolManager.GetObjectPool<T>();

        public ObjectPoolBase GetObjectPool(Type objectType) =>
            m_ObjectPoolManager.GetObjectPool(objectType);

        public IObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase =>
            m_ObjectPoolManager.GetObjectPool<T>(name);

        public ObjectPoolBase GetObjectPool(Type objectType, string name) =>
            m_ObjectPoolManager.GetObjectPool(objectType, name);

        public ObjectPoolBase GetObjectPool(Predicate<ObjectPoolBase> condition) =>
            m_ObjectPoolManager.GetObjectPool(condition);

        public ObjectPoolBase[] GetObjectPools(Predicate<ObjectPoolBase> condition) =>
            m_ObjectPoolManager.GetObjectPools(condition);

        public void GetObjectPools(Predicate<ObjectPoolBase> condition, List<ObjectPoolBase> results) =>
            m_ObjectPoolManager.GetObjectPools(condition, results);

        public ObjectPoolBase[] GetAllObjectPools() =>
            m_ObjectPoolManager.GetAllObjectPools();

        public void GetAllObjectPools(List<ObjectPoolBase> results) =>
            m_ObjectPoolManager.GetAllObjectPools(results);

        public ObjectPoolBase[] GetAllObjectPools(bool sort) =>
            m_ObjectPoolManager.GetAllObjectPools(sort);

        public void GetAllObjectPools(bool sort, List<ObjectPoolBase> results) =>
            m_ObjectPoolManager.GetAllObjectPools(sort, results);

        // ── 创建（单次获取） ────────────────────────────────────────────────────

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>();

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(expireTime);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, expireTime);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, expireTime);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, expireTime);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity, expireTime);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity, expireTime);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity, expireTime);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity, expireTime);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(capacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, capacity, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, capacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, capacity, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool<T>(name, autoReleaseInterval, capacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float autoReleaseInterval, int capacity, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateSingleSpawnObjectPool(objectType, name, autoReleaseInterval, capacity, expireTime, priority);

        // ── 创建（多次获取） ────────────────────────────────────────────────────

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>() where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>();

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(expireTime);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, expireTime);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, expireTime);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, expireTime);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity, expireTime);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity, expireTime);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity, priority);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(expireTime, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, expireTime, priority);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity, expireTime);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity, expireTime);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity, priority);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, expireTime, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, expireTime, priority);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(capacity, expireTime, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, capacity, expireTime, priority);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, capacity, expireTime, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, capacity, expireTime, priority);

        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool<T>(name, autoReleaseInterval, capacity, expireTime, priority);

        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float autoReleaseInterval, int capacity, float expireTime, int priority) =>
            m_ObjectPoolManager.CreateMultiSpawnObjectPool(objectType, name, autoReleaseInterval, capacity, expireTime, priority);

        // ── 销毁 ───────────────────────────────────────────────────────────────

        public bool DestroyObjectPool<T>() where T : ObjectBase =>
            m_ObjectPoolManager.DestroyObjectPool<T>();

        public bool DestroyObjectPool(Type objectType) =>
            m_ObjectPoolManager.DestroyObjectPool(objectType);

        public bool DestroyObjectPool<T>(string name) where T : ObjectBase =>
            m_ObjectPoolManager.DestroyObjectPool<T>(name);

        public bool DestroyObjectPool(Type objectType, string name) =>
            m_ObjectPoolManager.DestroyObjectPool(objectType, name);

        public bool DestroyObjectPool<T>(IObjectPool<T> objectPool) where T : ObjectBase =>
            m_ObjectPoolManager.DestroyObjectPool(objectPool);

        public bool DestroyObjectPool(ObjectPoolBase objectPool) =>
            m_ObjectPoolManager.DestroyObjectPool(objectPool);

        // ── 释放 ───────────────────────────────────────────────────────────────

        public void Release()
        {
            GameFrameworkLog.Info("Object pool release.");
            m_ObjectPoolManager.Release();
        }

        public void ReleaseAllUnused()
        {
            GameFrameworkLog.Info("Object pool release all unused.");
            m_ObjectPoolManager.ReleaseAllUnused();
        }
    }
}
