using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCore.Troilet_Bytecode
{
    public class TroiletWriter
    {
        public enum BufferFormat
        {
            Raw,
            B64
        }

        private MemoryStream _stream;
        internal BinaryWriter _writer;

        private Random _random;

        private static string ToBin(byte b)
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

        public TroiletWriter()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);

            _random = new Random();
        }

        public void WriteByte(byte b)
        {
            byte xorkey = (byte)_random.Next(0, 255);

            string databyte = ToBin((byte)(b ^ xorkey));
            string xorbyte = ToBin(xorkey);

            string byte1 = "";

            for (int i = 0; i < 8; i++)
                byte1 += (databyte[i] == xorbyte[i] ? "0" : "1");

            _writer.Write(Convert.ToByte(byte1, 2));
            _writer.Write(Convert.ToByte(databyte, 2));
        }
        public void ProtectedWrite(byte b) => WriteByte(b);
        public void ProtectedWrite(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
                WriteByte(b[i]);
        }

        public void ProtectedWrite(bool b) => WriteByte((byte)(b ? 1 : 0));

        public void ProtectedWrite(short i) => ProtectedWrite(BitConverter.GetBytes(i));
        public void ProtectedWrite(ushort i) => ProtectedWrite(BitConverter.GetBytes(i));
        public void ProtectedWrite(int i) => ProtectedWrite(BitConverter.GetBytes(i));
        public void ProtectedWrite(uint i) => ProtectedWrite(BitConverter.GetBytes(i));
        public void ProtectedWrite(long i) => ProtectedWrite(BitConverter.GetBytes(i));
        public void ProtectedWrite(ulong i) => ProtectedWrite(BitConverter.GetBytes(i));

        // Floating Point format made by the one and only me
        // Made my own cause i am too lazy to make IEEE 754 working + it makes it harder for people to get the numbers (i was just an idiot when doing IEEE)
        private void WriteFloatingPoint(double fp)
        {
            bool IsNegative(long i)
            {
                if (i + i < i)
                    return true;

                return false;
            }

            string[] data = fp.ToString().Split(',');

            long tmp = Convert.ToInt64(data[0]);

            ProtectedWrite((ulong)((Math.Abs(tmp) << 1) | (IsNegative(tmp) ? 1 : 0)));

            if (!fp.ToString().Contains(",") || data.Length < 2)
            {
                ProtectedWrite((ulong)0);
                return;
            }

            ProtectedWrite(Convert.ToUInt64(data[1]));
        }

        public void ProtectedWrite(float f) => WriteFloatingPoint(f);
        public void ProtectedWrite(double d) => WriteFloatingPoint(d);

        public void ProtectedWrite(string s)
        {
            ProtectedWrite(s.Length);

            if (s.Length == 0) return;

            ProtectedWrite(Encoding.UTF8.GetBytes(s));
        }

        public void CopyTo(MemoryStream stream) => _stream.CopyTo(stream);
        public byte[] GetBuffer(BufferFormat bf = BufferFormat.Raw)
        {
            switch (bf) // Added switch cause i want to add more bufferFormats tmr
            {
                case BufferFormat.Raw:
                    return _stream.ToArray();
                case BufferFormat.B64:
                    return Encoding.UTF8.GetBytes(Convert.ToBase64String(_stream.ToArray()));
                default:
                    return _stream.ToArray();
            }
        }
    }
}
