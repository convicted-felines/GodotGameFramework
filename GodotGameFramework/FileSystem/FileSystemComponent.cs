using GameFramework;
using GameFramework.FileSystem;
using Godot;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 文件系统组件。封装 IFileSystemManager，统一管理游戏运行时的虚拟文件系统包。
    /// 适合将大量小文件打包成单一 .vfs 文件，降低 IO 碎片化开销。
    /// </summary>
    public sealed partial class FileSystemComponent : GameFrameworkComponent
    {
        private IFileSystemManager m_FileSystemManager = null;

        public int Count => m_FileSystemManager.Count;

        public override void _Ready()
        {
            base._Ready();

            m_FileSystemManager = GameFrameworkEntry.GetModule<IFileSystemManager>();
            if (m_FileSystemManager == null)
            {
                GameFrameworkLog.Fatal("File system manager is invalid.");
                return;
            }

            m_FileSystemManager.SetFileSystemHelper(new GodotFileSystemHelper());
        }

        // ── 生命周期 ───────────────────────────────────────────────────────────

        /// <summary>
        /// 创建新的虚拟文件系统包（写入模式）。
        /// </summary>
        public IFileSystem CreateFileSystem(string fullPath, FileSystemAccess access, int maxFileCount, int maxBlockCount)
        {
            return m_FileSystemManager.CreateFileSystem(fullPath, access, maxFileCount, maxBlockCount);
        }

        /// <summary>
        /// 加载已有的虚拟文件系统包。
        /// </summary>
        public IFileSystem LoadFileSystem(string fullPath, FileSystemAccess access)
        {
            return m_FileSystemManager.LoadFileSystem(fullPath, access);
        }

        /// <summary>
        /// 销毁文件系统，可选删除物理文件。
        /// </summary>
        public void DestroyFileSystem(IFileSystem fileSystem, bool deletePhysicalFile = false)
        {
            m_FileSystemManager.DestroyFileSystem(fileSystem, deletePhysicalFile);
        }

        // ── 查询 ───────────────────────────────────────────────────────────────

        public bool HasFileSystem(string fullPath) => m_FileSystemManager.HasFileSystem(fullPath);

        public IFileSystem GetFileSystem(string fullPath) => m_FileSystemManager.GetFileSystem(fullPath);

        public IFileSystem[] GetAllFileSystems() => m_FileSystemManager.GetAllFileSystems();

        public void GetAllFileSystems(List<IFileSystem> results) => m_FileSystemManager.GetAllFileSystems(results);
    }
}
