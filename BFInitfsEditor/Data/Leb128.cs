using System.Collections.Generic;
using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    /// <summary>
    /// See https://en.wikipedia.org/wiki/LEB128 for details
    /// Slightly reworked from https://github.com/rzubek/mini-leb128/blob/master/LEB128.cs
    /// </summary>
    public class Leb128 : ILeb128
    {
        private const long SIGN_EXTEND_MASK = -1L;
        private const int INT64_BITSIZE = (sizeof(long) * 8);

        // private ctor
        private Leb128() { }

        public static ILeb128 GetInstance() => new Leb128();

        #region ILeb128

        public byte[] BuildLEB128Signed(long value)
        {
            var more = true;
            var bytesList = new List<byte>(2);

            while (more)
            {
                var chunk = (byte)(value & 0x7fL); // extract a 7-bit chunk
                value >>= 7;

                var signBitSet = (chunk & 0x40) != 0; // sign bit is the msb of a 7-bit byte, so 0x40
                more = !((value == 0 && !signBitSet) || (value == -1 && signBitSet));
                if (more) { chunk |= 0x80; } // set msb marker that more bytes are coming

                bytesList.Add(chunk);
            };

            return bytesList.ToArray();
        }

        public byte[] BuildLEB128Unsigned(ulong value)
        {
            var more = true;
            var bytesList = new List<byte>(2);

            while (more)
            {
                var chunk = (byte)(value & 0x7fUL); // extract a 7-bit chunk
                value >>= 7;

                more = value != 0;
                if (more) { chunk |= 0x80; } // set msb marker that more bytes are coming

                bytesList.Add(chunk);
            };

            return bytesList.ToArray();
        }

        public long ReadLEB128Signed(byte[] buffer, ref int beginPosition)
        {
            var value = 0L;
            var shift = 0;
            bool more = true, signBitSet = false;

            while (more)
            {
                var b = buffer[beginPosition++];

                more = (b & 0x80) != 0; // extract msb
                signBitSet = (b & 0x40) != 0; // sign bit is the msb of a 7-bit byte, so 0x40

                var chunk = b & 0x7fL; // extract lower 7 bits
                value |= chunk << shift;
                shift += 7;
            };

            // extend the sign of shorter negative numbers
            if (shift < INT64_BITSIZE && signBitSet) { value |= SIGN_EXTEND_MASK << shift; }

            return value;
        }

        public ulong ReadLEB128Unsigned(byte[] buffer, ref int beginPosition)
        {
            var value = 0UL;
            var shift = 0;
            var more = true;

            while (more)
            {
                var b = buffer[beginPosition++];

                more = (b & 0x80) != 0;   // extract msb
                var chunk = b & 0x7fUL; // extract lower 7 bits
                value |= chunk << shift;
                shift += 7;
            }

            return value;
        }

        #endregion
    }
}