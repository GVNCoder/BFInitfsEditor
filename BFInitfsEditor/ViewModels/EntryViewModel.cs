using BFInitfsEditor.Model;

namespace BFInitfsEditor.ViewModels
{
    public class EntryViewModel
    {
        public int ID { get; set; }
        public EntryType Type { get; set; }
        public string FullPath { get; set; }
        public string Name { get; set; }

        public FileEntry Entry { get; set; }

        public EntryViewModel[] SubEntries { get; set; }
    }
}