using System;
using System.IO;
using System.Text;

namespace GameEngine.Network.Utils
{
    public class ByteBufferBigEnding
    {
        private MemoryStream _stream = null;
        private BinaryWriter _writer = null;
        private BinaryReader _reader = null;

        public ByteBufferBigEnding()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        public ByteBufferBigEnding(byte[] data)
        {
            if (data != null)
            {
                _stream = new MemoryStream(data);
                _reader = new BinaryReader(_stream);
            }
            else
            {
                _stream = new MemoryStream();
                _writer = new BinaryWriter(_stream);
            }
        }

        public void Close()
        {
            _writer?.Close();
            _reader?.Close();

            _stream?.Close();
            _writer = null;
            _reader = null;
            _stream = null;
        }

        public void Write(byte v)
        {
            _writer.Write(v);
        }

        public void Write(int v)
        {
            _writer.Write(BytesConverter.GetBigEndian(v));
        }

        public void Write(short v)
        {
            _writer.Write(BytesConverter.GetBigEndian(v));
        }

        public void Write(long v)
        {
            _writer.Write(BytesConverter.GetBigEndian(v));
        }

        public void Write(float v)
        {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            _writer.Write(BitConverter.ToSingle(temp, 0));
        }

        public void Write(double v)
        {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            _writer.Write(BitConverter.ToDouble(temp, 0));
        }

        public void Write(string v)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            _writer.Write(BytesConverter.GetBigEndian((ushort)bytes.Length));
            _writer.Write(bytes);
        }

        public void Write(byte[] v)
        {
            _writer.Write(v);
        }

        /**
         * 写入长度和byte数组
         */
        public void WriteBytes(byte[] v)
        {
            Write(v.Length);
            _writer.Write(v);
        }

        // public void WriteBuffer(LuaByteBuffer strBuffer)
        // {
        //     WriteBytes(strBuffer.buffer);
        // }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public int ReadInt()
        {
            return BytesConverter.GetBigEndian(_reader.ReadInt32());
        }

        public short ReadShort()
        {
            return BytesConverter.GetBigEndian(_reader.ReadInt16());
        }

        public long ReadLong()
        {
            return BytesConverter.GetBigEndian(_reader.ReadInt64());
        }

        public float ReadFloat()
        {
            byte[] temp = BitConverter.GetBytes(_reader.ReadSingle());
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }

        public double ReadDouble()
        {
            byte[] temp = BitConverter.GetBytes(_reader.ReadDouble());
            Array.Reverse(temp);
            return BitConverter.ToDouble(temp, 0);
        }

        public string ReadString()
        {
            var len = ReadShort();
            var buffer = _reader.ReadBytes(len);
            return Encoding.UTF8.GetString(buffer);
        }

        /**
         * 读取长度和byte数组
         */
        public byte[] ReadBytes()
        {
            var len = ReadInt();
            return ReadBytes(len);
        }

        public byte[] ReadBytes(int len)
        {
            return _reader.ReadBytes(len);
        }

        // public LuaByteBuffer ReadBuffer()
        // {
        //     byte[] bytes = ReadBytes();
        //     return new LuaByteBuffer(bytes);
        // }

        public byte[] ToBytes()
        {
            _writer.Flush();
            return _stream.ToArray();
        }

        public void Flush()
        {
            _writer.Flush();
        }
    }
}