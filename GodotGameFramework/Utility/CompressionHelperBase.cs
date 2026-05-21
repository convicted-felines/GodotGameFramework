//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using System.IO;

namespace GodotGameFramework.Utility
{
    /// <summary>
    /// 压缩辅助器基类。继承此类实现自定义压缩算法，在 BaseComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class CompressionHelperBase : global::GameFramework.Utility.Compression.ICompressionHelper
    {
        public abstract bool Compress(byte[] bytes, int offset, int length, Stream compressedStream);
        public abstract bool Compress(Stream stream, Stream compressedStream);
        public abstract bool Decompress(byte[] bytes, int offset, int length, Stream decompressedStream);
        public abstract bool Decompress(Stream stream, Stream decompressedStream);
    }
}
