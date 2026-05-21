//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.FileSystem;
using GameFramework.Resource;
using System;

namespace GodotGameFramework
{
    /// <summary>
    /// 加载资源代理辅助器基类。继承此类实现自定义资源加载代理，在 ResourceComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class LoadResourceAgentHelperBase : ILoadResourceAgentHelper
    {
        public abstract event EventHandler<LoadResourceAgentHelperUpdateEventArgs> LoadResourceAgentHelperUpdate;
        public abstract event EventHandler<LoadResourceAgentHelperReadFileCompleteEventArgs> LoadResourceAgentHelperReadFileComplete;
        public abstract event EventHandler<LoadResourceAgentHelperReadBytesCompleteEventArgs> LoadResourceAgentHelperReadBytesComplete;
        public abstract event EventHandler<LoadResourceAgentHelperParseBytesCompleteEventArgs> LoadResourceAgentHelperParseBytesComplete;
        public abstract event EventHandler<LoadResourceAgentHelperLoadCompleteEventArgs> LoadResourceAgentHelperLoadComplete;
        public abstract event EventHandler<LoadResourceAgentHelperErrorEventArgs> LoadResourceAgentHelperError;

        public abstract void ReadFile(string fullPath);
        public abstract void ReadFile(IFileSystem fileSystem, string name);
        public abstract void ReadBytes(string fullPath);
        public abstract void ReadBytes(IFileSystem fileSystem, string name);
        public abstract void ParseBytes(byte[] bytes);
        public abstract void LoadAsset(object resource, string assetName, Type assetType, bool isScene);
        public abstract void Reset();
    }
}
