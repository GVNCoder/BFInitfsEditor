namespace BFInitfsEditor.Model
{
    public class Data
    {
        public ulong DataSize { get; set; } // leb128

        public FileEntry[] Entries { get; set; } = { }; // all files entries
    }
}