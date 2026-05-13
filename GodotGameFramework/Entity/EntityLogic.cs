//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Entity;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体逻辑基类。
    /// 用户自己的实体脚本继承此类，挂载在 PackedScene 的根节点上。
    /// 此类只包含生命周期回调，不持有任何 Godot 可见节点引用，
    /// 从而实现"预制体（场景）与逻辑分离"的目标。
    /// </summary>
    public abstract partial class EntityLogic : Node, IEntity
    {
        private int m_Id;
        private string m_EntityAssetName;
        private IEntityGroup m_EntityGroup;
        private bool m_Available;
        private bool m_Visible;

        // ── IEntity ──────────────────────────────────────────────────────────

        public int Id => m_Id;

        public string EntityAssetName => m_EntityAssetName;

        /// <summary>实体句柄，即 Node 自身。</summary>
        public object Handle => this;

        public IEntityGroup EntityGroup => m_EntityGroup;

        public bool Available => m_Available;

        public bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                Visible = value;   // 同步 Godot 可见性
            }
        }

        // ── IEntity 生命周期（由 EntityManager 调用，不要手动调用）────────────

        void IEntity.OnInit(int entityId, string entityAssetName, IEntityGroup entityGroup, bool isNewInstance, object userData)
        {
            m_Id = entityId;
            m_EntityAssetName = entityAssetName;
            m_EntityGroup = entityGroup;
            m_Available = false;
            m_Visible = false;
            OnInit(isNewInstance, userData);
        }

        void IEntity.OnShow(object userData)
        {
            m_Available = true;
            Visible = true;
            OnShow(userData);
        }

        void IEntity.OnHide(bool isShutdown, object userData)
        {
            Visible = false;
            m_Available = false;
            OnHide(isShutdown, userData);
        }

        void IEntity.OnRecycle()
        {
            m_Id = 0;
            OnRecycle();
        }

        void IEntity.OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            OnUpdate(elapseSeconds, realElapseSeconds);
        }

        void IEntity.OnAttached(IEntity childEntity, object userData)
        {
            OnAttached(childEntity as EntityLogic, userData);
        }

        void IEntity.OnDetached(IEntity childEntity, object userData)
        {
            OnDetached(childEntity as EntityLogic, userData);
        }

        void IEntity.OnAttachTo(IEntity parentEntity, object userData)
        {
            OnAttachTo(parentEntity as EntityLogic, userData);
        }

        void IEntity.OnDetachFrom(IEntity parentEntity, object userData)
        {
            OnDetachFrom(parentEntity as EntityLogic, userData);
        }

        // ── 供子类重写的虚方法（对应 Unity EntityLogic）──────────────────────

        /// <summary>实体初始化（仅在首次从池中取出时调用）。</summary>
        protected virtual void OnInit(bool isNewInstance, object userData) { }

        /// <summary>实体显示（每次从对象池取出均调用）。</summary>
        protected virtual void OnShow(object userData) { }

        /// <summary>实体隐藏（返回对象池前调用）。</summary>
        protected virtual void OnHide(bool isShutdown, object userData) { }

        /// <summary>实体回收（已回到对象池）。</summary>
        protected virtual void OnRecycle() { }

        /// <summary>实体每帧更新。</summary>
        protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) { }

        /// <summary>子实体附加到本实体。</summary>
        protected virtual void OnAttached(EntityLogic childEntity, object userData) { }

        /// <summary>子实体从本实体解除。</summary>
        protected virtual void OnDetached(EntityLogic childEntity, object userData) { }

        /// <summary>本实体附加到父实体。</summary>
        protected virtual void OnAttachTo(EntityLogic parentEntity, object userData) { }

        /// <summary>本实体从父实体解除。</summary>
        protected virtual void OnDetachFrom(EntityLogic parentEntity, object userData) { }
    }
}
