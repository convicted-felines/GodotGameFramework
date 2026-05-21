//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Entity;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体辅助器基类。
    /// 继承此类以实现自定义实体辅助器，在 EntityComponent 的 Inspector 中填写完整类名即可生效。
    /// 辅助器会被作为子节点挂载到 EntityComponent 下，可通过 EntityRoot 访问父节点。
    /// </summary>
    public abstract partial class EntityHelperBase : Node, IEntityHelper
    {
        /// <summary>实体根节点（即 EntityComponent 自身）。</summary>
        protected Node EntityRoot => GetParent();

        public abstract object InstantiateEntity(object entityAsset);

        public abstract IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup, object userData);

        public abstract void ReleaseEntity(object entityAsset, object entityInstance);
    }
}
