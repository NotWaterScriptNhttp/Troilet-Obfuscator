using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCore.Troilet_Bytecode
{
    // This class exists only because of testing!!! :moyai: :moyai: :moyai:
    public class TroiletReader
    {
        private byte[] _buffer;
        private int _position = 0;

        private string ToBin(byte b)
        {
            StringBuilder str = new StringBuilder(8);
            byte[] bl = new byte[8];

            for (int i = 0; i < bl.Length; i++)
            {
                bl[bl.Length - 1 - i] = (byte)(((b & (1 << i)) != 0) ? 1 : 0);
            }

            foreach (byte num in bl) str.Append(num);

            return str.ToString();
        }

        public TroiletReader(byte[] buffer) => _buffer = buffer;

        public byte ReadByte()
        {
            string b1 = ToBin(_buffer[_position++]), data = ToBin(_buffer[_position++]), xorbin = "";

            for (int i = 0; i < 8; i++)
            {
                if (b1[i] == '0')
                    xorbin += data[i];
                else xorbin += (data[i] == '0' ? '1' : '0');
            }

            byte o = (byte)(Convert.ToByte(data, 2) ^ Convert.ToByte(xorbin, 2));

            Console.WriteLine($"O: {o}");

            return o;
        }
        public byte[] ReadBytes(int count)
        {
            byte[] buff = new byte[count];

            for (int i = 0; i < count; i++)
                buff[i] = ReadByte();

            return buff;
        }

        public bool ReadBool() => ReadByte() == 1;

        public short ReadInt16() => BitConverter.ToInt16(ReadBytes(2), 0);
        public ushort ReadUInt16() => BitConverter.ToUInt16(ReadBytes(2), 0);
        public int ReadInt32() => BitConverter.ToInt32(ReadBytes(4), 0);
        public uint ReadUInt32() => BitConverter.ToUInt32(ReadBytes(4), 0);
        public long ReadInt64() => BitConverter.ToInt64(ReadBytes(8), 0);
        public ulong ReadUInt64() => BitConverter.ToUInt64(ReadBytes(8), 0);

        private double ReadFloatingPoint()
        {
            ulong fullnumber = ReadUInt64();
            bool sign = (fullnumber & 1) == 1;

            long num = (long)(fullnumber >> 1);
            if (sign) num = -num;

            return Convert.ToDouble(num.ToString() + "," + ReadUInt64().ToString());
        }

        public float ReadFloat() => (float)ReadFloatingPoint();
        public double ReadDouble() => ReadFloatingPoint();

        public string ReadString()
        {
            int len = ReadInt32();

            if (len == 0)
                return "";

            return Encoding.UTF8.GetString(ReadBytes(len));
        }
    }
}
