namespace BFInitfsEditor.Model
{
    public class FileEntry
    {
        public ulong UnknownSize { get; set; } // leb128
        public byte[] UnknownHeaderPart { get; set; } // 2 bytes

        public byte[] UnknownData { get; set; }

        public string Type { get; set; }
        public ulong StructSize { get; set; }

        public ulong FilePathSize { get; set; } // leb128
        public string FilePath { get; set; }

        public ulong FileSize { get; set; } //leb128
        public byte[] FileData { get; set; }
    }
}