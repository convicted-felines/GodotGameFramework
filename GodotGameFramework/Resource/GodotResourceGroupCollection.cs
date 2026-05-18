//------------------------------------------------------------
// GodotGameFramework
// Based on UnityGameFramework by Jiang Yin
//------------------------------------------------------------

using GameFramework.Resource;
using System.Collections.Generic;
using System.Linq;

namespace GodotGameFramework
{
    /// <summary>
    /// Godot 资源组集合：聚合多个资源组的只读视图。
    /// </summary>
    internal sealed class GodotResourceGroupCollection : IResourceGroupCollection
    {
        private readonly List<IResourceGroup> m_Groups;

        public GodotResourceGroupCollection(List<IResourceGroup> groups) => m_Groups = groups;

        public bool Ready => true;
        public int TotalCount => m_Groups.Sum(g => g.TotalCount);
        public int ReadyCount => TotalCount;
        public long TotalLength => 0L;
        public long TotalCompressedLength => 0L;
        public long ReadyLength => 0L;
        public long ReadyCompressedLength => 0L;
        public float Progress => 1f;

        public IResourceGroup[] GetResourceGroups() => m_Groups.ToArray();

        public string[] GetResourceNames()
        {
            var list = new List<string>();
            GetResourceNames(list);
            return list.ToArray();
        }

        public void GetResourceNames(List<string> results)
        {
            results.Clear();
            foreach (var g in m_Groups)
                results.AddRange(g.GetResourceNames());
        }
    }
}
