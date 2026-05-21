using GameFramework.Network;
using System.IO;

namespace GodotGameFramework
{
    /// <summary>
    /// 默认 TCP 网络频道辅助器。
    ///
    /// 协议约定（项目可自行继承或替换）：
    ///   包头固定 4 字节（int32 大端序）：表示后续 body 字节长度。
    ///   body 为任意字节流，由上层 PacketHandler 负责反序列化。
    ///
    /// 心跳：发送长度为 0 的空包（包头 = 0x00000000）。
    ///
    /// 项目若需自定义协议，继承此类并覆盖以下方法：
    ///   PacketHeaderLength, Serialize, DeserializePacketHeader, DeserializePacket
    /// </summary>
    public class DefaultNetworkChannelHelper : NetworkChannelHelperBase
    {
        protected INetworkChannel m_NetworkChannel;

        // ── INetworkChannelHelper ──────────────────────────────────────────────

        /// <summary>包头长度：4 字节 int32 body 长度。</summary>
        public override int PacketHeaderLength => 4;

        public override void Initialize(INetworkChannel networkChannel)
        {
            m_NetworkChannel = networkChannel;
        }

        public override void Shutdown()
        {
            m_NetworkChannel = null;
        }

        public virtual void PrepareForConnecting()
        {
            // 连接前的准备工作（如重置序列号）
        }

        public override bool SendHeartBeat()
        {
            if (m_NetworkChannel == null) return false;
            m_NetworkChannel.Send(new HeartBeatPacket());
            return true;
        }

        public override bool Serialize<T>(T packet, Stream destination)
        {
            if (packet is HeartBeatPacket)
            {
                // 心跳：body 长度 0
                WriteInt32BE(destination, 0);
                return true;
            }

            if (packet is RawPacket raw)
            {
                WriteInt32BE(destination, raw.Body.Length);
                destination.Write(raw.Body, 0, raw.Body.Length);
                return true;
            }

            return false;
        }

        public override IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData)
        {
            customErrorData = null;

            byte[] headerBytes = new byte[PacketHeaderLength];
            int read = source.Read(headerBytes, 0, PacketHeaderLength);
            if (read < PacketHeaderLength)
            {
                customErrorData = "Incomplete packet header.";
                return null;
            }

            int bodyLength = ReadInt32BE(headerBytes, 0);
            return new DefaultPacketHeader(bodyLength);
        }

        public override Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData)
        {
            customErrorData = null;

            if (packetHeader is not DefaultPacketHeader header)
            {
                customErrorData = "Invalid packet header type.";
                return null;
            }

            if (header.PacketLength == 0)
                return new HeartBeatPacket();

            byte[] body = new byte[header.PacketLength];
            int read = source.Read(body, 0, body.Length);
            if (read < body.Length)
            {
                customErrorData = "Incomplete packet body.";
                return null;
            }

            return new RawPacket(body);
        }

        // ── 工具方法 ───────────────────────────────────────────────────────────

        protected static void WriteInt32BE(Stream stream, int value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        protected static int ReadInt32BE(byte[] buf, int offset) =>
            (buf[offset] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3];
    }

    // ── 辅助数据类型 ───────────────────────────────────────────────────────────

    /// <summary>默认包头：仅包含 body 长度。</summary>
    public sealed class DefaultPacketHeader : IPacketHeader
    {
        public int PacketLength { get; }
        public DefaultPacketHeader(int bodyLength) => PacketLength = bodyLength;
    }

    /// <summary>心跳包（空 body）。</summary>
    public sealed class HeartBeatPacket : Packet
    {
        public override int Id => 0;
        public override void Clear() { }
    }

    /// <summary>原始字节包：body 为任意字节数组，由业务层解析。</summary>
    public sealed class RawPacket : Packet
    {
        public override int Id => -1;
        public byte[] Body { get; private set; }

        public RawPacket(byte[] body) => Body = body;

        public override void Clear() => Body = null;
    }
}
