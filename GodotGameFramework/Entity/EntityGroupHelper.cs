//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------


namespace GodotGameFramework
{
    /// <summary>
    /// 默认实体组辅助器。
    /// 每个实体组在 Godot 场景树中对应一个专属的 Node 容器，
    /// 所有属于该组的实体实例均挂载在此节点下，保持场景树整洁。
    /// </summary>
    public sealed partial class EntityGroupHelper : EntityGroupHelperBase
    {
    }
}
