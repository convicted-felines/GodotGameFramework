using GameFramework.FileSystem;
using GameFramework;
using Godot;
using System;
using System.IO;

namespace GodotGameFramework
{
    /// <summary>
    /// Godot 文件系统流。将 Godot 路径（user://）解析为绝对路径后用 System.IO.FileStream 操作。
    /// res:// 路径在 export 包中是只读虚拟文件系统，不支持 FileSystem 模块写入，仅允许 Read 访问。
    /// </summary>
    public sealed class GodotFileSystemStream : FileSystemStream, IDisposable
    {
        private readonly FileStream m_FileStream;

        public GodotFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            string resolvedPath = ResolvePath(fullPath);
            if (resolvedPath == null)
            {
                throw new GameFrameworkException($"Cannot resolve path: {fullPath}");
            }

            // 确保目录存在（写入时）
            if (access != FileSystemAccess.Read)
            {
                string dir = Path.GetDirectoryName(resolvedPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            m_FileStream = access switch
            {
                FileSystemAccess.Read => new FileStream(resolvedPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read),
                FileSystemAccess.Write => new FileStream(resolvedPath, createNew ? FileMode.Create : FileMode.Open, System.IO.FileAccess.Write, FileShare.Read),
                FileSystemAccess.ReadWrite => new FileStream(resolvedPath, createNew ? FileMode.Create : FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.Read),
                _ => throw new GameFrameworkException($"Unsupported access mode: {access}")
            };
        }

        protected override long Position
        {
            get => m_FileStream.Position;
            set => m_FileStream.Position = value;
        }

        protected override long Length => m_FileStream.Length;

        protected override void SetLength(long length) => m_FileStream.SetLength(length);

        protected override void Seek(long offset, SeekOrigin origin) => m_FileStream.Seek(offset, origin);

        protected override int ReadByte() => m_FileStream.ReadByte();

        protected override int Read(byte[] buffer, int startIndex, int length)
            => m_FileStream.Read(buffer, startIndex, length);

        protected override void WriteByte(byte value) => m_FileStream.WriteByte(value);

        protected override void Write(byte[] buffer, int startIndex, int length)
            => m_FileStream.Write(buffer, startIndex, length);

        protected override void Flush() => m_FileStream.Flush();

        protected override void Close() => m_FileStream.Close();

        public void Dispose() => m_FileStream.Dispose();

        // ── 路径解析 ───────────────────────────────────────────────────────────
        // user:// → OS.GetUserDataDir() / 相对路径
        // res://  → ProjectSettings.GlobalizePath（编辑器可用，export 包中只读）
        // 其他    → 直接使用原始路径（绝对路径）
        private static string ResolvePath(string path)
        {
            if (path.StartsWith("user://", StringComparison.Ordinal))
            {
                string relative = path.Substring("user://".Length);
                return Path.Combine(OS.GetUserDataDir(), relative);
            }

            if (path.StartsWith("res://", StringComparison.Ordinal))
            {
                // 在编辑器中可 Globalize；export 后只读
                return ProjectSettings.GlobalizePath(path);
            }

            return path;
        }
    }
}
