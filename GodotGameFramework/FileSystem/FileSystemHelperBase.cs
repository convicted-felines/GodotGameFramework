//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.FileSystem;

namespace GodotGameFramework
{
    /// <summary>
    /// 文件系统辅助器基类。继承此类实现自定义文件系统辅助器，在 FileSystemComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class FileSystemHelperBase : IFileSystemHelper
    {
        public abstract FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew);
    }
}
