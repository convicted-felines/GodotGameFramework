//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework;
using GameFramework.Entity;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体辅助器。
    /// 负责将 Godot PackedScene 资源实例化为 Node，
    /// 并从实例中提取 EntityLogic 作为框架认识的 IEntity，
    /// 以及在实体回收时将节点从场景树中移除。
    /// </summary>
    public sealed class EntityHelper : IEntityHelper
    {
        private readonly Node m_EntityRoot;

        /// <param name="entityRoot">所有实体节点挂载的根节点（EntityComponent 自身）。</param>
        public EntityHelper(Node entityRoot)
        {
            m_EntityRoot = entityRoot;
        }

        /// <summary>
        /// 实例化实体资源（PackedScene → Node）。
        /// asset 由 ResourceManager 加载，此处只负责 Instantiate。
        /// </summary>
        public object InstantiateEntity(object entityAsset)
        {
            if (entityAsset is not PackedScene packedScene)
            {
                GameFrameworkLog.Error($"Entity asset '{entityAsset}' is not a PackedScene.");
                return null;
            }

            return packedScene.Instantiate();
        }

        /// <summary>
        /// 从已实例化的 Node 中创建 IEntity。
        /// 约定：PackedScene 根节点必须挂载一个继承自 EntityLogic 的脚本。
        /// 将节点挂载到对应实体组的 Helper 节点（场景树容器）下。
        /// </summary>
        public IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup, object userData)
        {
            if (entityInstance is not Node instanceNode)
            {
                GameFrameworkLog.Error($"Entity instance '{entityInstance}' is not a Node.");
                return null;
            }

            if (instanceNode is not EntityLogic entityLogic)
            {
                // 尝试找根节点下第一个 EntityLogic 子节点（兼容根节点是 Node3D / Node2D 的场景）
                entityLogic = FindEntityLogic(instanceNode);
                if (entityLogic == null)
                {
                    GameFrameworkLog.Error($"Entity instance '{instanceNode.Name}' has no EntityLogic component.");
                    instanceNode.QueueFree();
                    return null;
                }
            }

            // 挂载到实体组对应的容器节点下
            if (entityGroup?.Helper is EntityGroupHelper groupHelper)
            {
                groupHelper.AddChild(instanceNode);
            }
            else
            {
                m_EntityRoot.AddChild(instanceNode);
            }

            return entityLogic;
        }

        /// <summary>
        /// 释放实体——将节点从场景树中移除并释放内存。
        /// 对象池决定何时真正释放，此处只做 Godot 层的清理。
        /// </summary>
        public void ReleaseEntity(object entityAsset, object entityInstance)
        {
            if (entityInstance is Node node && GodotObject.IsInstanceValid(node))
            {
                node.QueueFree();
            }
        }

        // ── 私有辅助 ──────────────────────────────────────────────────────────

        private static EntityLogic FindEntityLogic(Node root)
        {
            if (root is EntityLogic logic)
                return logic;

            foreach (Node child in root.GetChildren())
            {
                var result = FindEntityLogic(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
