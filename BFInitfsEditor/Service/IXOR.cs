namespace BFInitfsEditor.Service
{
    public interface IXOR
    {
        byte[] Encrypt(byte[] data, byte[] key);
        byte[] Decrypt(byte[] data, byte[] key);
    }
}