using System;

namespace GameEngine.Network.Utils
{
    public class BytesConverter
    {
        public static int GetBigEndian(byte[] data)
        {
            var value = 0;
            for (var i = 0; i < data.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                {
                    value |= data[i] << (i * 8);
                }
                else
                {
                    value |= data[i] << ((data.Length - i - 1) * 8);
                }
            }

            return GetBigEndian(value);
        }

        public static int GetBigEndian(int value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static short GetBigEndian(short value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static ushort GetBigEndian(ushort value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static uint GetBigEndian(uint value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static long GetBigEndian(long value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static double GetBigEndian(double value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder(value) : value;
        }

        public static float GetBigEndian(float value)
        {
            return BitConverter.IsLittleEndian ? SwapByteOrder((int)value) : value;
        }

        public static int GetLittleEndian(int value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        public static uint GetLittleEndian(uint value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        public static ushort GetLittleEndian(ushort value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        public static double GetLittleEndian(double value)
        {
            return BitConverter.IsLittleEndian ? value : SwapByteOrder(value);
        }

        private static int SwapByteOrder(int value)
        {
            var swap = (int)((uint)((0x000000FF) & (value >> 24)
                                    | (0x0000FF00) & (value >> 8)
                                    | (0x00FF0000) & (value << 8))
                             | (0xFF000000) & (value << 24));
            return swap;
        }

        private static long SwapByteOrder(long value)
        {
            var uvalue = (ulong)value;
            var swap = ((0x00000000000000FF) & (uvalue >> 56)
                        | (0x000000000000FF00) & (uvalue >> 40)
                        | (0x0000000000FF0000) & (uvalue >> 24)
                        | (0x00000000FF000000) & (uvalue >> 8)
                        | (0x000000FF00000000) & (uvalue << 8)
                        | (0x0000FF0000000000) & (uvalue << 24)
                        | (0x00FF000000000000) & (uvalue << 40)
                        | (0xFF00000000000000) & (uvalue << 56));

            return (long)swap;
        }

        private static short SwapByteOrder(short value)
        {
            return (short)((0x00FF & (value >> 8))
                           | (0xFF00 & (value << 8)));
        }

        private static ushort SwapByteOrder(ushort value)
        {
            return (ushort)((0x00FF & (value >> 8))
                            | (0xFF00 & (value << 8)));
        }

        private static uint SwapByteOrder(uint value)
        {
            var swap = ((0x000000FF) & (value >> 24)
                        | (0x0000FF00) & (value >> 8)
                        | (0x00FF0000) & (value << 8)
                        | (0xFF000000) & (value << 24));
            return swap;
        }

        private static double SwapByteOrder(double value)
        {
            var buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer, 0, buffer.Length);
            return BitConverter.ToDouble(buffer, 0);
        }
    }
}