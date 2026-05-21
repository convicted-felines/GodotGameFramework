//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using Godot;

namespace GodotGameFramework
{
    /// <summary>
    /// 实体组配置（Inspector 可展开编辑的 Resource）。
    /// 对应 UnityGameFramework 中 EntityComponent.EntityGroup 的数据结构。
    /// </summary>
    [GlobalClass]
    public partial class EntityGroupConfig : Resource
    {
        /// <summary>实体组名称。</summary>
        [Export] public string Name = string.Empty;

        /// <summary>对象池自动释放间隔（秒）。</summary>
        [Export] public float InstanceAutoReleaseInterval = 60f;

        /// <summary>对象池容量。</summary>
        [Export] public int InstanceCapacity = 16;

        /// <summary>对象池过期时间（秒）。</summary>
        [Export] public float InstanceExpireTime = 60f;

        /// <summary>对象池优先级。</summary>
        [Export] public int InstancePriority = 0;
    }
}
