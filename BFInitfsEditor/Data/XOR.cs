using System;
using System.Collections.Generic;

using BFInitfsEditor.Service;

// set short alias
using CONST = BFInitfsEditor.Data.InitfsConstants;

namespace BFInitfsEditor.Data
{
    public class XOR : IXOR
    {
        public static IXOR GetInstance() => new XOR();

        // private ctor
        private XOR() { }

        private const int MAGIC_XOR = 0x7B;

        #region IXOR

        public byte[] Encrypt(byte[] data, byte[] key) => _XOR(data, key);
        public byte[] Decrypt(byte[] data, byte[] key) => _XOR(data, key);

        #endregion

        #region Private helpers

        private static byte[] _XOR(byte[] data, IReadOnlyList<byte> key)
        {
            var result = new byte[data.Length];
            Array.Copy(data, result, data.Length);

            for (var i = 0; i < data.Length; i++)
                result[i] ^= (byte)(key[i % CONST.MAGIC_SIZE] ^ MAGIC_XOR);

            return result;
        }

        #endregion
    }
}