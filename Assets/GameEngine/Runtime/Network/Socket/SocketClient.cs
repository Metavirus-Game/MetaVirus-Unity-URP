//using LuaFramework;

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using static GameEngine.Common.EngineConsts;

namespace GameEngine.Network.Socket
{
    public class SocketClient
    {
        private TcpClient _client = null;
        private NetworkStream _outStream = null;

        private const int MaxRead = 8192;
        private readonly byte[] _byteBuffer = new byte[MaxRead];

        public UnityAction<byte[]> onReceiveData;
        public UnityAction<SocketEvent, string> onSocketEvent;

        private string _host;
        private int _port;

        public SocketClient()
        {
        }

        private void InitSocket()
        {
        }


        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ConnectServer(string host, int port)
        {
            _client = null;
            _host = host;
            _port = port;
            InitSocket();
            try
            {
                var address = Dns.GetHostAddresses(host);
                if (address.Length == 0)
                {
                    Debug.LogError("host invalid");
                    return;
                }

                _client = address[0].AddressFamily == AddressFamily.InterNetworkV6
                    ? new TcpClient(AddressFamily.InterNetworkV6)
                    : new TcpClient(AddressFamily.InterNetwork);

                _client.SendTimeout = 1000;
                _client.ReceiveTimeout = 1000;
                _client.NoDelay = true;
                _client.BeginConnect(host, port, OnConnect, null);
            }
            catch (Exception e)
            {
                Close();
                Debug.LogError(e.Message);
            }
        }


        /// <summary>
        /// 连接上服务器
        /// </summary>
        private void OnConnect(IAsyncResult asr)
        {
            if (_client.Connected)
            {
                _outStream = _client.GetStream();
                _client.GetStream().BeginRead(_byteBuffer, 0, MaxRead, OnRead, null);
                onSocketEvent?.Invoke(SocketEvent.Connected, "");
            }
            else
            {
                onSocketEvent?.Invoke(SocketEvent.ConnectFailed, "");
            }
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        private void OnRead(IAsyncResult asr)
        {
            try
            {
                var bytesRead = 0;
                if (_client == null)
                {
                    OnDisconnected(SocketDisType.Disconnect, "Client Disconnect");
                    return;
                }

                lock (_client.GetStream())
                {
                    //读取字节流到缓冲区
                    bytesRead = _client.GetStream().EndRead(asr);
                }

                if (bytesRead < 1)
                {
                    //包尺寸有问题，断线处理
                    OnDisconnected(SocketDisType.Disconnect, "bytesRead < 1");
                    return;
                }

                byte[] read = new byte[bytesRead];
                Array.Copy(_byteBuffer, read, bytesRead);
                onReceiveData?.Invoke(read);

                lock (_client.GetStream())
                {
                    //分析完，再次监听服务器发过来的新消息
                    Array.Clear(_byteBuffer, 0, _byteBuffer.Length); //清空数组
                    _client.GetStream().BeginRead(_byteBuffer, 0, MaxRead, new AsyncCallback(OnRead), null);
                }
            }
            catch (Exception ex)
            {
                //PrintBytes();
                Debug.LogError(ex);
                OnDisconnected(SocketDisType.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 写数据
        /// </summary>
        public void WriteMessage(byte[] message)
        {
            if (IsConnected())
            {
                _outStream.BeginWrite(message, 0, message.Length, OnWrite, null);
            }
            else
            {
                onSocketEvent?.Invoke(SocketEvent.NotConnect, "");
                Debug.LogError("client.connected----->>false");
            }
        }

        void OnWrite(IAsyncResult r)
        {
            try
            {
                _outStream.EndWrite(r);
            }
            catch (Exception ex)
            {
                Debug.LogError("OnWrite--->>>" + ex.Message);
            }
        }

        /// <summary>
        /// 丢失链接
        /// </summary>
        private void OnDisconnected(SocketDisType dis, string msg)
        {
            Close(); //关掉客户端链接

            var socketEvent = SocketEvent.Exception;
            if (dis == SocketDisType.Disconnect)
            {
                socketEvent = SocketEvent.Disconnected;
            }

            onSocketEvent?.Invoke(socketEvent, msg);
            Debug.LogError("Connection was closed by the server:>" + msg + " Distype:>" + dis);
        }

        public bool IsConnected()
        {
            return _client != null && _client.Connected;
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        public void Close()
        {
            if (_client is { Connected: true }) _client.Close();
            _client = null;
        }
    }
}