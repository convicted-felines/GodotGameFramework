//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Entity;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体组辅助器基类。
    /// 继承此类以实现自定义实体组辅助器，在 EntityComponent 的 Inspector 中填写完整类名即可生效。
    /// 每个实体组会创建一个对应的辅助器实例并挂载到 EntityComponent 下。
    /// </summary>
    public abstract partial class EntityGroupHelperBase : Node, IEntityGroupHelper
    {
    }
}
