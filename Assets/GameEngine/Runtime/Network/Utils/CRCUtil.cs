namespace GameEngine.Network.Utils
{
    public static class CRCUtil
    {
        private static readonly int[] CRC16Table =
        {
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
            0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF
        };

        public static int GetCRC16(byte[] data)
        {
            var crc = 0x0;

            foreach (var d in data)
            {
                var b = (short)(d & 0x00ff);
                var tem = (short)((crc >> 0x0c) & 0x00ff);
                crc <<= 4;
                crc &= 0x0000ffff;
                crc ^= CRC16Table[(tem ^ (b >> 0x04)) & 0x0000ffff];
                crc &= 0x0000ffff;
                tem = (short)((crc >> 0x0c) & 0x00ff);
                crc <<= 4;
                crc &= 0x0000ffff;
                crc ^= CRC16Table[(tem ^ (b & 0x0f)) & 0x0000ffff];
                crc &= 0x0000ffff;
            }

            return crc;
        }

        public static byte[] XOrData(byte[] data, int key)
        {
            var keys = new int[4];

            for (var i = 3; i >= 0; i--)
            {
                keys[i] = (key >> (i * 8)) & 0xff;
            }

            for (var i = 0; i < data.Length; i++)
            {
                var k = keys[i % 4];
                data[i] = (byte)((data[i] ^ k) & 0xff);
            }

            return data;
        }
    }
}