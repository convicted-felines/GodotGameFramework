//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Entity;
using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体组辅助器。
    /// 每个实体组在 Godot 场景树中对应一个专属的 Node 容器，
    /// 所有属于该组的实体实例均挂载在此节点下，保持场景树整洁。
    /// </summary>
    public sealed partial class EntityGroupHelper : Node, IEntityGroupHelper
    {
        // IEntityGroupHelper 当前是空接口，此处无需实现额外成员。
        // Node 作为容器节点，实体 Helper 会将实例化的场景节点 AddChild 到此节点。
    }
}
