//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework;
using System.IO;
using System.IO.Compression;

namespace GodotGameFramework.Utility
{
    /// <summary>
    /// 基于 System.IO.Compression (DeflateStream) 的压缩辅助器。
    /// </summary>
    public sealed class DefaultCompressionHelper : global::GameFramework.Utility.Compression.ICompressionHelper
    {
        public bool Compress(byte[] bytes, int offset, int length, Stream compressedStream)
        {
            try
            {
                using var deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true);
                deflate.Write(bytes, offset, length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Compress(Stream stream, Stream compressedStream)
        {
            try
            {
                using var deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true);
                stream.CopyTo(deflate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Decompress(byte[] bytes, int offset, int length, Stream decompressedStream)
        {
            try
            {
                using var input = new MemoryStream(bytes, offset, length);
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                deflate.CopyTo(decompressedStream);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Decompress(Stream stream, Stream decompressedStream)
        {
            try
            {
                using var deflate = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
                deflate.CopyTo(decompressedStream);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
