using BFInitfsEditor.Service;

namespace BFInitfsEditor.Data
{
    public class XOR : IXOR
    {
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            return new byte[1]; // TODO: dummy
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            return new byte[1]; // TODO: dummy
        }
    }
}