using GameFramework;
using GameFramework.Network;
using Godot;
using System;
using System.Collections.Generic;

namespace GodotGameFramework
{
    /// <summary>
    /// 网络组件。封装 INetworkManager，提供多频道 TCP 连接管理。
    ///
    /// 网络频道默认使用 DefaultNetworkChannelHelper（4 字节长度前缀协议）。
    /// 项目可自定义继承 DefaultNetworkChannelHelper 并传入 CreateNetworkChannel。
    ///
    /// 使用示例：
    ///   var net = GameEntry.GetComponent＜NetworkComponent＞();
    ///   var channel = net.CreateNetworkChannel("Main", new DefaultNetworkChannelHelper());
    ///   channel.Connect("127.0.0.1", 9000);
    ///   channel.Send(new RawPacket(data));
    /// </summary>
    public sealed partial class NetworkComponent : GameFrameworkComponent
    {
        private INetworkManager m_NetworkManager = null;

        // ── 属性 ───────────────────────────────────────────────────────────────

        public int NetworkChannelCount => m_NetworkManager.NetworkChannelCount;

        // ── 事件透传 ───────────────────────────────────────────────────────────

        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected
        {
            add => m_NetworkManager.NetworkConnected += value;
            remove => m_NetworkManager.NetworkConnected -= value;
        }

        public event EventHandler<NetworkClosedEventArgs> NetworkClosed
        {
            add => m_NetworkManager.NetworkClosed += value;
            remove => m_NetworkManager.NetworkClosed -= value;
        }

        public event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat
        {
            add => m_NetworkManager.NetworkMissHeartBeat += value;
            remove => m_NetworkManager.NetworkMissHeartBeat -= value;
        }

        public event EventHandler<NetworkErrorEventArgs> NetworkError
        {
            add => m_NetworkManager.NetworkError += value;
            remove => m_NetworkManager.NetworkError -= value;
        }

        public event EventHandler<NetworkCustomErrorEventArgs> NetworkCustomError
        {
            add => m_NetworkManager.NetworkCustomError += value;
            remove => m_NetworkManager.NetworkCustomError -= value;
        }

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            base._Ready();

            m_NetworkManager = GameFrameworkEntry.GetModule<INetworkManager>();
            if (m_NetworkManager == null)
            {
                GameFrameworkLog.Fatal("Network manager is invalid.");
                return;
            }
        }

        // ── 频道管理 ───────────────────────────────────────────────────────────

        public bool HasNetworkChannel(string name) =>
            m_NetworkManager.HasNetworkChannel(name);

        public INetworkChannel GetNetworkChannel(string name) =>
            m_NetworkManager.GetNetworkChannel(name);

        public INetworkChannel[] GetAllNetworkChannels() =>
            m_NetworkManager.GetAllNetworkChannels();

        public void GetAllNetworkChannels(List<INetworkChannel> results) =>
            m_NetworkManager.GetAllNetworkChannels(results);

        /// <summary>创建 TCP 网络频道（使用默认协议辅助器）。</summary>
        public INetworkChannel CreateNetworkChannel(string name)
        {
            return m_NetworkManager.CreateNetworkChannel(name, ServiceType.Tcp, new DefaultNetworkChannelHelper());
        }

        /// <summary>创建 TCP 网络频道（使用自定义协议辅助器）。</summary>
        public INetworkChannel CreateNetworkChannel(string name, INetworkChannelHelper helper)
        {
            return m_NetworkManager.CreateNetworkChannel(name, ServiceType.Tcp, helper);
        }

        public void DestroyNetworkChannel(string name) =>
            m_NetworkManager.DestroyNetworkChannel(name);
    }
}
