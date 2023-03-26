using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameEngine.Base;
using GameEngine.Base.Attributes;
using GameEngine.Config;
using GameEngine.Network.Message;
using GameEngine.Network.Message.Attrs;
using GameEngine.Network.Message.Protocol.Module;
using GameEngine.Network.Socket;
using GameEngine.Network.Utils;
using GameEngine.Runtime.Network.Message.Protocol;
using GameEngine.Utils;
using UnityEngine;
using UnityEngine.Events;
using static GameEngine.Common.EngineConsts;

namespace GameEngine.Network
{
    [ServicePriority(ServicePriorityValue.NetworkService)]
    public class NetworkService : BaseService
    {
        private const float PacketTimeout = 10; //数据包超时时间

        public int segmentPerFrame = 10;
        private SocketClient _socket;

        private int _serial = 0;

        private MemoryStream _memStream;
        private BinaryReader _reader;

        private UnityAction<SocketEvent, string> _onSocketEventAction;

        public UnityAction<SocketEvent, string> OnSocketEventAction
        {
            get => _onSocketEventAction;
            set => _onSocketEventAction = value;
        }

        private int StreamRemainingByes => (int)(_memStream.Length - _memStream.Position);

        //private readonly DataEncrypt _encrypt = DataEncrypt.Inst;

        private readonly Queue<KeyValuePair<SocketEvent, string>> _socketEventQueue =
            new Queue<KeyValuePair<SocketEvent, string>>();

        //收到的数据包队列
        private readonly Queue<NetBusBasePacket> _dataQueue = new();

        //发送的数据包的等待列表(需要响应的)，由于协议机制限制，同一时间不能等待多条相同method数据包的回应
        //serial <=> waitingPacket
        private readonly Dictionary<int, WaitingPacket> _waitingPackets = new();

        //数据包监听器 msgCode <=> listener
        private readonly Dictionary<int, PacketListener> _packetListeners = new();

        private readonly Dictionary<short, Type> _protocolTypes = new();

        private bool _isReady = false;

        public override void PostConstruct()
        {
            // var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // foreach (var assembly in assemblies)
            // {
            //     var tps = assembly.GetTypes();
            //     foreach (var type in tps)
            //     {
            //         var pa = type.GetCustomAttribute<ProtocolAttribute>();
            //         if (pa != null)
            //         {
            //             _protocolTypes[pa.GloboalId] = type;
            //         }
            //     }
            // }

            var attrs = AppDomain.CurrentDomain.GetTypesHasAttribute<ProtocolAttribute>();

            foreach (var ta in attrs)
            {
                var pa = ta.Attribute;
                _protocolTypes[pa.GloboalId] = ta.Type;
            }

            _socket = new SocketClient();
            InitSocket();
        }

        public void SetOnSocketEventAction(UnityAction<SocketEvent, string> onSocketEventAction)
        {
            _onSocketEventAction = onSocketEventAction;
        }

        public bool IsConnected()
        {
            return _socket != null && _socket.IsConnected() && _isReady;
        }


        private void InitSocket()
        {
            _socket.onSocketEvent += OnSocketEvent; //异步调用
            _socket.onReceiveData += OnReceiveData; //异步调用
        }


        /**
         * 连接服务器，服务器ip和port在GameConfig中配置
         */
        public void ConnectServer(UnityAction<SocketEvent, string> onSocketEventAction = null)
        {
            var host = GameConfig.Inst.Server;
            var port = GameConfig.Inst.Port;
            Debug.Log($"Connecting to {host}:{port}");

            if (onSocketEventAction != null)
            {
                _onSocketEventAction = onSocketEventAction;
            }

            if (!_socket.IsConnected())
            {
                _socket.ConnectServer(host, port);
            }
        }

        private IEnumerator TryReconnect()
        {
            var tryTimes = 1;
            var storeAction = _onSocketEventAction;

            while (tryTimes <= 3 && !IsConnected())
            {
                var times = tryTimes;
                Debug.Log($"Server Disconnected, Try reconnect {times} time after {times} second...");
                //等待1秒
                yield return new WaitForSeconds(times);
                var state = SocketEvent.NotConnect;
                ConnectServer((evt, msg) =>
                {
                    if (evt != SocketEvent.Connected)
                    {
                        Debug.Log($"Try reconnect {times} time failed with state {evt}");
                    }

                    if (evt == SocketEvent.ConnectFailed)
                    {
                        storeAction?.Invoke(evt, times.ToString());
                    }

                    state = evt;
                });

                yield return new WaitUntil(() => state != SocketEvent.NotConnect);

                if (state == SocketEvent.Connected)
                {
                    //重连成功了
                    Debug.Log("Server reconnect successful.");
                    SetOnSocketEventAction(storeAction);
                    storeAction?.Invoke(SocketEvent.Reconnected, "");
                    storeAction?.Invoke(SocketEvent.Connected, "");
                }

                tryTimes++;
            }
        }

        private void OnReceiveData(byte[] data)
        {
            _memStream.Seek(0, SeekOrigin.End);
            _memStream.Write(data, 0, data.Length);
            _memStream.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                if (StreamRemainingByes < NetBusBasePacket.HeadByteLenPos)
                {
                    break;
                }

                //read segment head
                var lenHeader = _reader.ReadBytes(NetBusBasePacket.HeadByteLenPos);
                var lenBytes = new byte[4];
                //提取2-5，total length
                Array.Copy(lenHeader, 2, lenBytes, 0, 4);

                var totalLen = BytesConverter.GetBigEndian(lenBytes);
                var bodyLen = totalLen - NetBusBasePacket.HeadByteLenPos;
                
                _memStream.Position -= NetBusBasePacket.HeadByteLenPos;
                
                if (StreamRemainingByes < totalLen)
                {
                    //not enough bytes for body
                    break;
                }
                

                var packet = _reader.ReadBytes(totalLen);
                var mainType = packet[0];
                var subType = packet[1];

                var msgCode = (short)((mainType << 8) | subType);

                if (_protocolTypes.ContainsKey(msgCode))
                {
                    var t = _protocolTypes[msgCode];
                    var ci = t.GetConstructor(Type.EmptyTypes);
                    var segment = (NetBusBasePacket)ci?.Invoke(Array.Empty<object>());
                    if (segment == null) continue;
                    segment.Decode(packet);
                    lock (_dataQueue)
                    {
                        _dataQueue.Enqueue(segment);
                    }
                }
                else
                {
                    Debug.Log($"Can't match the Packet Type to Code 0x[{msgCode:X4}] ");
                }
            }

            var remaining = _reader.ReadBytes(StreamRemainingByes);
            _memStream.SetLength(0);
            _memStream.Write(remaining, 0, remaining.Length);
        }

        private void OnSocketEvent(SocketEvent evt, string message)
        {
            lock (_socketEventQueue)
            {
                message ??= "";
                _socketEventQueue.Enqueue(new KeyValuePair<SocketEvent, string>(evt, message));
            }
        }

        private void OnConnected()
        {
            _memStream = new MemoryStream();
            _reader = new BinaryReader(_memStream);
            //dataQueue = new Queue<NetBusBasePacket>();
            //_waitingPackets = new Dictionary<int, WaitingPacket>();
            //_packetListeners = new Dictionary<int, PacketListener>();
            _isReady = true;

            //send ClientLoginRequest to Server
            var req = new ClientLoginRequest
            {
                ModuleId = GameConfig.Inst.ModuleId,
                InstId = GameConfig.Inst.InstId
            };
            SendPacket(req, (resp) =>
            {
                var state = resp.IsSuccess
                    ? SocketEvent.Connected
                    : SocketEvent.ClientLoginFailed;

                Debug.Log($"Network State {state}");

                _onSocketEventAction?.Invoke(state, "");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainType"></param>
        /// <param name="subType"></param>
        /// <param name="listener">int msgCode, NetBusBasePacket segment</param>
        public void RegisterPacketListener(byte mainType, byte subType, UnityAction<RespPacket> listener)
        {
            int msgCode = toMsgCode(mainType, subType);

            if (_packetListeners.ContainsKey(msgCode))
            {
                var pl = _packetListeners[msgCode];
                pl.AddListener(listener);
            }
            else
            {
                var pl = new PacketListener(msgCode, listener);
                _packetListeners[msgCode] = pl;
            }
        }

        public void UnRegisterPacketListener(byte mainType, byte subType, UnityAction<RespPacket> listener)
        {
            int msgCode = toMsgCode(mainType, subType);

            if (_packetListeners.ContainsKey(msgCode))
            {
                var pl = _packetListeners[msgCode];
                pl.RemoveListener(listener);
            }
        }

        public Task<RespPacket> SendPacketToAsync(NetBusBasePacket packet, int dstId)
        {
            var taskCompletionSource = new TaskCompletionSource<RespPacket>();
            SendPacketTo(packet, dstId, resp => { taskCompletionSource.SetResult(resp); });
            return taskCompletionSource.Task;
        }

        public int SendPacketTo(NetBusBasePacket packet, int dstId, UnityAction<RespPacket> onResp = null)
        {
            var srcId = GameConfig.Inst.ClientGlobalId;
            packet.SrcId = srcId;
            packet.DstId = dstId;
            return SendPacket(packet, onResp);
        }

        public int SendPacket(NetBusBasePacket packet, UnityAction<RespPacket> onResp = null)
        {
            packet.Serial = NextSerial();
            if (!IsConnected()) return -1;
            if (onResp != null)
            {
                _waitingPackets[packet.Serial] = new WaitingPacket(packet.Serial, onResp);
            }

            _socket.WriteMessage(packet.Encode());
            return packet.Serial;
        }

        private int NextSerial()
        {
            lock (this)
            {
                _serial++;
                if (_serial < 0) _serial = 0;
                return _serial;
            }
        }

        private void OnDestroy()
        {
            _socket?.Close();
        }

        private void ProcessSocketEvents()
        {
            lock (_socketEventQueue)
            {
                if (_socketEventQueue.Count > 0)
                {
                    while (_socketEventQueue.Count > 0)
                    {
                        var socketEvt = _socketEventQueue.Dequeue();
                        switch (socketEvt.Key)
                        {
                            case SocketEvent.Connected:
                                //连接成功
                                OnConnected();
                                break;
                            case SocketEvent.Disconnected:
                                //断开连接
                                _isReady = false;
                                break;
                            case SocketEvent.Exception:
                                //异常断开连接
                                _isReady = false;
                                //尝试重连
                                StartCoroutine(TryReconnect());
                                break;
                            case SocketEvent.NotConnect:
                                //没有连接
                                break;
                        }

                        //GameEngineMain.CallMethod("LuaNetwork", "OnSocketEvent", socketEvt.Key, socketEvt.Value);
                        if (socketEvt.Key != SocketEvent.Connected)
                        {
                            Debug.LogError($"Network State {socketEvt.Key}");
                            _onSocketEventAction?.Invoke(socketEvt.Key, socketEvt.Value);
                        }
                    }
                }
            }
        }

        private void ProcessSegments()
        {
            lock (_dataQueue)
            {
                int count = 0;
                while (_dataQueue.Count > 0)
                {
                    var packet = _dataQueue.Dequeue();

                    //Debug.Log("process message: " + packet);

                    //先查找_waitingPackets是否包含响应回调
                    if (_waitingPackets.ContainsKey(packet.Serial))
                    {
                        var wp = _waitingPackets[packet.Serial];
                        _waitingPackets.Remove(packet.Serial);
                        wp.InvokeResponse(packet);
                    }


                    //再查找是否有常驻监听
                    int msgCode = packet.MsgCode;
                    if (_packetListeners.ContainsKey(msgCode))
                    {
                        var pl = _packetListeners[msgCode];
                        pl.InvokeListeners(packet);
                    }

                    //GameEngineMain.CallMethod("LuaNetwork", "OnReceiveData", segment);
                    count++;
                    if (count > segmentPerFrame)
                    {
                        break;
                    }
                }
            }
        }

        private void ProcessWaitingPackets()
        {
            var keys = _waitingPackets.Keys.ToArray();
            foreach (var s in keys)
            {
                var packet = _waitingPackets[s];
                if (packet.IncTimer(Time.deltaTime))
                {
                    //超时了，调用超时通知，并移除
                    try
                    {
                        packet.InvokeTimeout();
                    }
                    finally
                    {
                        _waitingPackets.Remove(s);
                    }
                }
            }
        }

        void Update()
        {
            try
            {
                ProcessSocketEvents();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            try
            {
                ProcessSegments();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            try
            {
                ProcessWaitingPackets();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void Disconnect()
        {
            _socket.Close();
            _isReady = false;
        }


        public static int toMsgCode(byte main, byte sub)
        {
            return main << 8 | sub;
        }

        private class PacketListener
        {
            private int MsgCode { get; }
            private UnityAction<RespPacket> packetListeners;

            public PacketListener(int msgCode, UnityAction<RespPacket> packetListener)
            {
                //Method = ProcessNetworkMethod(method);
                MsgCode = msgCode;
                packetListeners = packetListener;
            }

            public void AddListener(UnityAction<RespPacket> listener)
            {
                packetListeners += listener;
            }

            public void RemoveListener(UnityAction<RespPacket> listener)
            {
                packetListeners -= listener;
            }

            public void InvokeListeners(NetBusBasePacket packet)
            {
                packetListeners?.Invoke(new RespPacket(RespPacket.RespCode.Success, packet));
            }
        }

        private class WaitingPacket
        {
            private UnityAction<RespPacket> onResponse;

            private float timer = 0;

            public int Serial { get; }

            public WaitingPacket(int serial, UnityAction<RespPacket> onResponse)
            {
                Serial = serial;
                this.onResponse = onResponse;
            }

            public bool IncTimer(float delta)
            {
                timer += delta;
                if (timer > PacketTimeout)
                {
                    return true;
                }

                return false;
            }

            public void InvokeTimeout()
            {
                onResponse?.Invoke(new RespPacket(RespPacket.RespCode.Timeout, null));
            }

            public void InvokeResponse(NetBusBasePacket resp)
            {
                if (resp.IsErrorMsg && resp is ClientProtocolError error)
                {
                    onResponse?.Invoke(new RespPacket(error.MessageCode));
                }
                else
                {
                    onResponse?.Invoke(new RespPacket(RespPacket.RespCode.Success, resp));
                }
            }
        }
    }
}