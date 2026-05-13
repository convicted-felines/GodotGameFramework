//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Entity;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体组件。
    /// 封装 IEntityManager，提供实体的显示/隐藏/查询/父子附加等能力。
    /// 在 Godot 场景树中作为 Node 存在，自动注册到 GameEntry。
    ///
    /// 使用步骤：
    ///   1. 在 BaseComponent 节点下（或之后）添加 EntityComponent 节点。
    ///   2. 在 Inspector 中配置实体组（EntityGroupNames）和对应参数。
    ///   3. 调用 ShowEntity / HideEntity 来驱动实体生命周期。
    ///   4. 用户的实体脚本继承 EntityLogic，放到 PackedScene 的根节点上。
    /// </summary>
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        // ── Inspector 配置 ─────────────────────────────────────────────────────

        /// <summary>要预先注册的实体组名称列表（顺序与下方参数列表对应）。</summary>
        [Export] public string[] EntityGroupNames = Array.Empty<string>();

        /// <summary>各实体组对象池自动释放间隔（秒）。</summary>
        [Export] public float[] InstanceAutoReleaseIntervals = Array.Empty<float>();

        /// <summary>各实体组对象池容量。</summary>
        [Export] public int[] InstanceCapacities = Array.Empty<int>();

        /// <summary>各实体组对象池过期时间（秒）。</summary>
        [Export] public float[] InstanceExpireTimes = Array.Empty<float>();

        /// <summary>各实体组对象池优先级。</summary>
        [Export] public int[] InstancePriorities = Array.Empty<int>();

        // ── 内部状态 ───────────────────────────────────────────────────────────

        private IEntityManager m_EntityManager = null;

        // ── 属性代理 ───────────────────────────────────────────────────────────

        public int EntityCount => m_EntityManager.EntityCount;
        public int EntityGroupCount => m_EntityManager.EntityGroupCount;

        // ── 事件（透传给用户订阅） ────────────────────────────────────────────

        public event EventHandler<ShowEntitySuccessEventArgs> ShowEntitySuccess
        {
            add => m_EntityManager.ShowEntitySuccess += value;
            remove => m_EntityManager.ShowEntitySuccess -= value;
        }

        public event EventHandler<ShowEntityFailureEventArgs> ShowEntityFailure
        {
            add => m_EntityManager.ShowEntityFailure += value;
            remove => m_EntityManager.ShowEntityFailure -= value;
        }

        public event EventHandler<ShowEntityUpdateEventArgs> ShowEntityUpdate
        {
            add => m_EntityManager.ShowEntityUpdate += value;
            remove => m_EntityManager.ShowEntityUpdate -= value;
        }

        public event EventHandler<ShowEntityDependencyAssetEventArgs> ShowEntityDependencyAsset
        {
            add => m_EntityManager.ShowEntityDependencyAsset += value;
            remove => m_EntityManager.ShowEntityDependencyAsset -= value;
        }

        public event EventHandler<HideEntityCompleteEventArgs> HideEntityComplete
        {
            add => m_EntityManager.HideEntityComplete += value;
            remove => m_EntityManager.HideEntityComplete -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_EntityManager = GameFrameworkEntry.GetModule<IEntityManager>();
            if (m_EntityManager == null)
            {
                GameFrameworkLog.Fatal("Entity manager is invalid.");
                return;
            }

            // 注入对象池管理器
            var objectPoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            if (objectPoolManager == null)
            {
                GameFrameworkLog.Fatal("Object pool manager is invalid.");
                return;
            }
            m_EntityManager.SetObjectPoolManager(objectPoolManager);

            // 注入资源管理器（可选：ResourceComponent 尚未实现时跳过）
            var resourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (resourceManager != null)
            {
                m_EntityManager.SetResourceManager(resourceManager);
            }

            // 注入实体辅助器
            m_EntityManager.SetEntityHelper(new EntityHelper(this));

            // 注册 Inspector 中配置的实体组
            RegisterEntityGroupsFromExport();
        }

        // ── 实体组操作 ─────────────────────────────────────────────────────────

        public bool HasEntityGroup(string entityGroupName) =>
            m_EntityManager.HasEntityGroup(entityGroupName);

        public IEntityGroup GetEntityGroup(string entityGroupName) =>
            m_EntityManager.GetEntityGroup(entityGroupName);

        public IEntityGroup[] GetAllEntityGroups() =>
            m_EntityManager.GetAllEntityGroups();

        /// <summary>
        /// 动态添加实体组（运行时调用，Inspector 中未配置时使用）。
        /// </summary>
        public bool AddEntityGroup(string entityGroupName,
            float instanceAutoReleaseInterval = 60f,
            int instanceCapacity = 16,
            float instanceExpireTime = 60f,
            int instancePriority = 0)
        {
            if (m_EntityManager.HasEntityGroup(entityGroupName))
                return false;

            var groupHelper = CreateGroupHelperNode(entityGroupName);
            return m_EntityManager.AddEntityGroup(entityGroupName,
                instanceAutoReleaseInterval, instanceCapacity,
                instanceExpireTime, instancePriority, groupHelper);
        }

        // ── 实体查询 ───────────────────────────────────────────────────────────

        public bool HasEntity(int entityId) => m_EntityManager.HasEntity(entityId);
        public bool HasEntity(string entityAssetName) => m_EntityManager.HasEntity(entityAssetName);

        public IEntity GetEntity(int entityId) => m_EntityManager.GetEntity(entityId);
        public IEntity GetEntity(string entityAssetName) => m_EntityManager.GetEntity(entityAssetName);

        public IEntity[] GetEntities(string entityAssetName) => m_EntityManager.GetEntities(entityAssetName);
        public void GetEntities(string entityAssetName, List<IEntity> results) => m_EntityManager.GetEntities(entityAssetName, results);

        public IEntity[] GetAllLoadedEntities() => m_EntityManager.GetAllLoadedEntities();
        public void GetAllLoadedEntities(List<IEntity> results) => m_EntityManager.GetAllLoadedEntities(results);

        public int[] GetAllLoadingEntityIds() => m_EntityManager.GetAllLoadingEntityIds();
        public void GetAllLoadingEntityIds(List<int> results) => m_EntityManager.GetAllLoadingEntityIds(results);

        public bool IsLoadingEntity(int entityId) => m_EntityManager.IsLoadingEntity(entityId);
        public bool IsValidEntity(IEntity entity) => m_EntityManager.IsValidEntity(entity);

        // ── 实体显示 / 隐藏 ────────────────────────────────────────────────────

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName) =>
            m_EntityManager.ShowEntity(entityId, entityAssetName, entityGroupName);

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, int priority) =>
            m_EntityManager.ShowEntity(entityId, entityAssetName, entityGroupName, priority);

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, object userData) =>
            m_EntityManager.ShowEntity(entityId, entityAssetName, entityGroupName, userData);

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, int priority, object userData) =>
            m_EntityManager.ShowEntity(entityId, entityAssetName, entityGroupName, priority, userData);

        public void HideEntity(int entityId) => m_EntityManager.HideEntity(entityId);
        public void HideEntity(int entityId, object userData) => m_EntityManager.HideEntity(entityId, userData);
        public void HideEntity(IEntity entity) => m_EntityManager.HideEntity(entity);
        public void HideEntity(IEntity entity, object userData) => m_EntityManager.HideEntity(entity, userData);

        public void HideAllLoadedEntities() => m_EntityManager.HideAllLoadedEntities();
        public void HideAllLoadedEntities(object userData) => m_EntityManager.HideAllLoadedEntities(userData);
        public void HideAllLoadingEntities() => m_EntityManager.HideAllLoadingEntities();

        // ── 父子实体 ───────────────────────────────────────────────────────────

        public IEntity GetParentEntity(int childEntityId) => m_EntityManager.GetParentEntity(childEntityId);
        public IEntity GetParentEntity(IEntity childEntity) => m_EntityManager.GetParentEntity(childEntity);

        public int GetChildEntityCount(int parentEntityId) => m_EntityManager.GetChildEntityCount(parentEntityId);

        public IEntity GetChildEntity(int parentEntityId) => m_EntityManager.GetChildEntity(parentEntityId);
        public IEntity GetChildEntity(IEntity parentEntity) => m_EntityManager.GetChildEntity(parentEntity);

        public IEntity[] GetChildEntities(int parentEntityId) => m_EntityManager.GetChildEntities(parentEntityId);
        public void GetChildEntities(int parentEntityId, List<IEntity> results) => m_EntityManager.GetChildEntities(parentEntityId, results);
        public IEntity[] GetChildEntities(IEntity parentEntity) => m_EntityManager.GetChildEntities(parentEntity);
        public void GetChildEntities(IEntity parentEntity, List<IEntity> results) => m_EntityManager.GetChildEntities(parentEntity, results);

        public void AttachEntity(int childEntityId, int parentEntityId) => m_EntityManager.AttachEntity(childEntityId, parentEntityId);
        public void AttachEntity(int childEntityId, int parentEntityId, object userData) => m_EntityManager.AttachEntity(childEntityId, parentEntityId, userData);
        public void AttachEntity(int childEntityId, IEntity parentEntity) => m_EntityManager.AttachEntity(childEntityId, parentEntity);
        public void AttachEntity(int childEntityId, IEntity parentEntity, object userData) => m_EntityManager.AttachEntity(childEntityId, parentEntity, userData);
        public void AttachEntity(IEntity childEntity, int parentEntityId) => m_EntityManager.AttachEntity(childEntity, parentEntityId);
        public void AttachEntity(IEntity childEntity, int parentEntityId, object userData) => m_EntityManager.AttachEntity(childEntity, parentEntityId, userData);
        public void AttachEntity(IEntity childEntity, IEntity parentEntity) => m_EntityManager.AttachEntity(childEntity, parentEntity);
        public void AttachEntity(IEntity childEntity, IEntity parentEntity, object userData) => m_EntityManager.AttachEntity(childEntity, parentEntity, userData);

        public void DetachEntity(int childEntityId) => m_EntityManager.DetachEntity(childEntityId);
        public void DetachEntity(int childEntityId, object userData) => m_EntityManager.DetachEntity(childEntityId, userData);
        public void DetachEntity(IEntity childEntity) => m_EntityManager.DetachEntity(childEntity);
        public void DetachEntity(IEntity childEntity, object userData) => m_EntityManager.DetachEntity(childEntity, userData);

        public void DetachChildEntities(int parentEntityId) => m_EntityManager.DetachChildEntities(parentEntityId);
        public void DetachChildEntities(int parentEntityId, object userData) => m_EntityManager.DetachChildEntities(parentEntityId, userData);
        public void DetachChildEntities(IEntity parentEntity) => m_EntityManager.DetachChildEntities(parentEntity);
        public void DetachChildEntities(IEntity parentEntity, object userData) => m_EntityManager.DetachChildEntities(parentEntity, userData);

        // ── 对象池参数设置（运行时调整） ───────────────────────────────────────

        public void SetInstanceLocked(IEntity entity, bool locked) =>
            GetEntityGroup(entity.EntityGroup.Name)?.SetEntityInstanceLocked(entity.Handle, locked);

        public void SetInstancePriority(IEntity entity, int priority) =>
            GetEntityGroup(entity.EntityGroup.Name)?.SetEntityInstancePriority(entity.Handle, priority);

        // ── 私有辅助 ───────────────────────────────────────────────────────────

        private void RegisterEntityGroupsFromExport()
        {
            int count = EntityGroupNames?.Length ?? 0;
            for (int i = 0; i < count; i++)
            {
                string groupName = EntityGroupNames[i];
                if (string.IsNullOrEmpty(groupName) || m_EntityManager.HasEntityGroup(groupName))
                    continue;

                float releaseInterval = i < InstanceAutoReleaseIntervals.Length ? InstanceAutoReleaseIntervals[i] : 60f;
                int capacity = i < InstanceCapacities.Length ? InstanceCapacities[i] : 16;
                float expireTime = i < InstanceExpireTimes.Length ? InstanceExpireTimes[i] : 60f;
                int priority = i < InstancePriorities.Length ? InstancePriorities[i] : 0;

                var groupHelper = CreateGroupHelperNode(groupName);
                m_EntityManager.AddEntityGroup(groupName, releaseInterval, capacity, expireTime, priority, groupHelper);
            }
        }

        /// <summary>为实体组创建并注册对应的场景树容器节点。</summary>
        private EntityGroupHelper CreateGroupHelperNode(string groupName)
        {
            var helper = new EntityGroupHelper();
            helper.Name = $"EntityGroup_{groupName}";
            AddChild(helper);
            return helper;
        }
    }
}
