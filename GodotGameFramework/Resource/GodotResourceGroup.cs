//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Resource;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// Godot 资源组：在 Godot 中无实际加载意义，作为元数据跟踪容器。
    /// 所有资源均已就绪（Godot res:// 内置资源始终可用）。
    /// </summary>
    internal sealed class GodotResourceGroup : IResourceGroup
    {
        private readonly List<string> m_ResourceNames = new();

        public GodotResourceGroup(string name) => Name = name;

        public string Name { get; }
        public bool Ready => true;
        public int TotalCount => m_ResourceNames.Count;
        public int ReadyCount => m_ResourceNames.Count;
        public long TotalLength => 0L;
        public long TotalCompressedLength => 0L;
        public long ReadyLength => 0L;
        public long ReadyCompressedLength => 0L;
        public float Progress => 1f;

        public void AddResource(string name) => m_ResourceNames.Add(name);

        public string[] GetResourceNames() => m_ResourceNames.ToArray();

        public void GetResourceNames(List<string> results)
        {
            results.Clear();
            results.AddRange(m_ResourceNames);
        }
    }
}
