namespace BFInitfsEditor.Model
{
    public class Entity
    {
        public byte[] Header { get; set; }

        // 4 byte zero

        // 1 byte 'x' begin hash indicator
        public byte[] Hash { get; set; }
        // 1 byte 'x' end hash indicator

        // 30 byte zero

        public byte[] EncryptionKey { get; set; }

        // 3 zero bytes

        public Data Data { get; set; }
    }
}