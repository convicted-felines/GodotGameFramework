using GameFramework.FileSystem;

namespace GodotGameFramework
{
    /// <summary>
    /// Godot 文件系统辅助器。提供基于 GodotFileSystemStream 的流创建。
    /// </summary>
    public sealed class GodotFileSystemHelper : IFileSystemHelper
    {
        public FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            return new GodotFileSystemStream(fullPath, access, createNew);
        }
    }
}
