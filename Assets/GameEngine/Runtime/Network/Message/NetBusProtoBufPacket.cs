using System;
using System.Reflection;
using GameEngine.Network.Message.Attrs;
using GameEngine.Network.Message.Protobuf;
using GameEngine.Network.Utils;
using Google.Protobuf;
using UnityEngine;

namespace GameEngine.Network.Message
{
    /**
     * 与服务器通信的协议基类，基于Protobuf编码
     * 继承此类扩展协议数据
     */
    public class NetBusProtoBufPacket<T> : NetBusBasePacket where T : IMessage, new()
    {
        public T ProtoBufMsg { get; internal set; }

        private int GetKey(int crc)
        {
            return (int)((Serial << 16) & 0xffff0000) | (crc & 0x0000ffff);
        }

        public NetBusProtoBufPacket()
        {
        }

        public NetBusProtoBufPacket(T protoBufMsg)
        {
            ProtoBufMsg = protoBufMsg;
        }

        protected override byte[] EncodeBody()
        {
            var buffer = new ByteBufferBigEnding();
            var data = ProtoBufMsg?.ToByteArray();
            var crc = CRCUtil.GetCRC16(data);
            buffer.WriteBytes(CRCUtil.XOrData(data, GetKey(crc)));
            buffer.Write(crc);
            return buffer.ToBytes();
        }

        protected override void DecodeBody(byte[] body)
        {
            try
            {
                var buffer = new ByteBufferBigEnding(body);
                var data = buffer.ReadBytes();
                var crc = buffer.ReadInt();
                var key = GetKey(crc);

                var msg = new T();
                msg.MergeFrom(CRCUtil.XOrData(data, key));
                ProtoBufMsg = msg;
            }
            catch (Exception e)
            {
                Debug.LogError($"Decode Message {MainType:X}-{SubType:X} Body Error");
                Debug.LogException(e);
            }
        }
    }
}