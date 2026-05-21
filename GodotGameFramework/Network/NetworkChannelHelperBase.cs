//------------------------------------------------------------
// GodotGameFramework
//------------------------------------------------------------

using GameFramework.Network;

namespace GodotGameFramework
{
    /// <summary>
    /// 网络频道辅助器基类。继承此类实现自定义网络协议，在 NetworkComponent 的 Inspector 中填写完整类名即可生效。
    /// </summary>
    public abstract class NetworkChannelHelperBase : INetworkChannelHelper
    {
        public abstract int PacketHeaderLength { get; }
        public abstract void Initialize(INetworkChannel networkChannel);
        public abstract void Shutdown();
        public abstract bool SendHeartBeat();
        public abstract bool Serialize<T>(T packet, System.IO.Stream destination) where T : Packet;
        public abstract IPacketHeader DeserializePacketHeader(System.IO.Stream source, out object customErrorData);
        public abstract Packet DeserializePacket(IPacketHeader packetHeader, System.IO.Stream source, out object customErrorData);

        public void PrepareForConnecting()
        {
            
        }

    }
}
