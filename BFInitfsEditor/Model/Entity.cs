namespace BFInitfsEditor.Model
{
    public class Entity
    {
        public const int AfterHeaderZero = 4;
        public const int AfterHashZero = 30;
        public const int AfterEncryptionKeyZero = 3;

        public byte[] Header { get; set; }

        // 4 byte zero

        public byte[] Hash { get; set; } // 'x' hash 'x'

        // 30 byte zero

        public byte[] EncryptionKey { get; set; }

        // 3 zero bytes

        public Data Data { get; set; } = new Data();
    }
}