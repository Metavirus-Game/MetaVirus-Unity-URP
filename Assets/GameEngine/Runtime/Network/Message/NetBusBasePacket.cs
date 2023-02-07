using System;
using System.Reflection;
using GameEngine.Network.Message.Attrs;
using GameEngine.Network.Message.Protobuf;
using GameEngine.Network.Utils;
using Google.Protobuf;

namespace GameEngine.Network.Message
{
    public abstract class NetBusBasePacket
    {
        public const int MsgCodeStartPos = 0;
        public const int BodyLengthStartPos = 2;
        public const int HeadByteLenPos = 2 + 4 + 1;
        private Header Header { get; set; }
        public byte MainType { get; set; }
        public byte SubType { get; set; }

        public short MsgCode
        {
            get => (short)(((MainType << 8) & 0xff00 | SubType) & 0xffff);
            set
            {
                MainType = (byte)((value >> 8) & 0xff);
                SubType = (byte)(value & 0xff);
            }
        }

        public int SrcId
        {
            get => Header.SrcId;
            set => Header.SrcId = value;
        }

        public int DstId
        {
            get => Header.DstId;
            set => Header.DstId = value;
        }

        public int Serial
        {
            get => Header.Serial;
            set => Header.Serial = value;
        }

        public bool IsErrorMsg => Header.ErrorMsg;

        public int TypeFlag
        {
            get => Header.TypeFlag;
            set => Header.TypeFlag = value;
        }

        public long UserGlobalId
        {
            get => Header.JoygameId;
            set => Header.JoygameId = value;
        }

        public int SessionId
        {
            get => Header.SessionId;
            set => Header.SessionId = value;
        }

        public int ClientIp
        {
            get => Header.ClientIp;
            set => Header.ClientIp = value;
        }

        public byte[] CustomHeader
        {
            get => Header.CustomHeader.ToByteArray();
            set => Header.CustomHeader = ByteString.CopyFrom(value);
        }

        public NetBusBasePacket()
        {
            var protocol = GetType().GetCustomAttribute<ProtocolAttribute>();
            if (protocol != null)
            {
                MainType = protocol.MainType;
                SubType = protocol.SubType;
            }

            Header = new Header();
        }

        public byte[] Encode()
        {
            var customHeader = EncodeCustomHeader() ?? Array.Empty<byte>();
            CustomHeader = customHeader;

            var header = Header.ToByteArray();
            var body = EncodeBody() ?? Array.Empty<byte>();

            var buffer = new ByteBufferBigEnding();

            buffer.Write(MainType);
            buffer.Write(SubType);
            buffer.Write(body.Length + header.Length + HeadByteLenPos);
            buffer.Write((byte)header.Length);

            buffer.Write(header);
            buffer.Write(body);

            return buffer.ToBytes();
        }

        public void Decode(byte[] data)
        {
            var buffer = new ByteBufferBigEnding(data);
            MsgCode = buffer.ReadShort();
            var totalLength = buffer.ReadInt();
            var headerLength = buffer.ReadByte();

            var bodyLen = totalLength - headerLength - HeadByteLenPos;

            var header = buffer.ReadBytes(headerLength);
            Header.MergeFrom(header);

            var customHeader = Header.CustomHeader;
            if (!customHeader.IsEmpty)
            {
                DecodeCustomHeader(customHeader.ToByteArray());
            }

            var body = buffer.ReadBytes(bodyLen);
            DecodeBody(body);
        }

        /**
         * 编码数据， 覆盖以实现自定义编码
         */
        protected virtual byte[] EncodeBody()
        {
            return null;
        }

        /**
         * 解码数据， 覆盖已实现自定义解码
         */
        protected virtual void DecodeBody(byte[] data)
        {
        }

        protected virtual byte[] EncodeCustomHeader()
        {
            return null;
        }

        protected virtual void DecodeCustomHeader(byte[] customHeader)
        {
        }
    }
}