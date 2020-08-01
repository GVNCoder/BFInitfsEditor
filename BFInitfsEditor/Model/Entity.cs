namespace BFInitfsEditor.Model
{
    public class Entity
    {
        public byte[] Header { get; set; }

        // 4 byte zero

        public byte[] Hash { get; set; } // 'x' hash 'x'

        // 30 byte zero

        public byte[] EncryptionKey { get; set; }

        // 3 zero bytes

        public Data Data { get; set; } = new Data();
    }
}