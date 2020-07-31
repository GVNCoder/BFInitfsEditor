namespace BFInitfsEditor.Model
{
    public class FileEntry
    {
        public ulong UnknownSize { get; set; } // leb128
        public byte[] UnknownHeaderPart { get; set; } // 2 bytes

        public byte[] UnknownData { get; set; }

        public byte NextDataType1 { get; set; } // 7(string)  or 19(payload)
        public string Word1 { get; set; } // name

        public ulong FileNameSize { get; set; } // leb128
        public string FileName { get; set; }

        public byte NextDataType2 { get; set; } // 7(string)  or 19(payload)
        public string Word2 { get; set; } // payload

        public ulong FileSize { get; set; } //leb128
        public byte[] FileData { get; set; }
    }
}