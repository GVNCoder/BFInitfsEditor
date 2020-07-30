using System;
using System.IO;

namespace BFInitfsEditor.Extension
{
    /// <summary>
    /// See https://en.wikipedia.org/wiki/LEB128 for details
    /// </summary>
    public static class Leb128Extension
    {
        private const long SIGN_EXTEND_MASK = -1L;
        private const int INT64_BITSIZE = (sizeof(long) * 8);

        public static void WriteLEB128Signed(this BinaryReader reader, long value) => WriteLEB128Signed(reader, value, out _);

        public static void WriteLEB128Signed(this BinaryReader reader, long value, out int bytes)
        {
            bytes = 0;
            var more = true;
            var stream = reader.BaseStream;

            while (more)
            {
                var chunk = (byte)(value & 0x7fL); // extract a 7-bit chunk
                value >>= 7;

                var signBitSet = (chunk & 0x40) != 0; // sign bit is the msb of a 7-bit byte, so 0x40
                more = !((value == 0 && !signBitSet) || (value == -1 && signBitSet));
                if (more) { chunk |= 0x80; } // set msb marker that more bytes are coming

                stream.WriteByte(chunk);
                bytes += 1;
            };
        }

        public static void WriteLEB128Unsigned(this BinaryReader reader, ulong value) => WriteLEB128Unsigned(reader, value, out _);

        public static void WriteLEB128Unsigned(this BinaryReader reader, ulong value, out int bytes)
        {
            bytes = 0;
            var more = true;
            var stream = reader.BaseStream;

            while (more)
            {
                var chunk = (byte)(value & 0x7fUL); // extract a 7-bit chunk
                value >>= 7;

                more = value != 0;
                if (more) { chunk |= 0x80; } // set msb marker that more bytes are coming

                stream.WriteByte(chunk);
                bytes += 1;
            };
        }

        public static long ReadLEB128Signed(this BinaryReader reader) => ReadLEB128Signed(reader, out _);

        public static long ReadLEB128Signed(this BinaryReader reader, out int bytes)
        {
            bytes = 0;

            var value = 0L;
            var shift = 0;
            bool more = true, signBitSet = false;
            var stream = reader.BaseStream;

            while (more)
            {
                var next = stream.ReadByte();
                if (next < 0) { throw new InvalidOperationException("Unexpected end of stream"); }

                var b = (byte) next;
                bytes += 1;

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

        public static ulong ReadLEB128Unsigned(this BinaryReader reader) => ReadLEB128Unsigned(reader, out _);

        public static ulong ReadLEB128Unsigned(this BinaryReader reader, out int bytes)
        {
            bytes = 0;

            var value = 0UL;
            var shift = 0;
            var more = true;
            var stream = reader.BaseStream;

            while (more)
            {
                var next = stream.ReadByte();
                if (next < 0) { throw new InvalidOperationException("Unexpected end of stream"); }

                var b = (byte) next;
                bytes += 1;

                more = (b & 0x80) != 0;   // extract msb
                var chunk = b & 0x7fUL; // extract lower 7 bits
                value |= chunk << shift;
                shift += 7;
            }

            return value;
        }
    }
}