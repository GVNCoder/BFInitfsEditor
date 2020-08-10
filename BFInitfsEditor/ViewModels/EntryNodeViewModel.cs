using BFInitfsEditor.Model;

namespace BFInitfsEditor.ViewModels
{
    public class EntryNodeViewModel
    {
        public EntryNodeType Type { get; set; }
        public string FullPath { get; set; }
        public string Name { get; set; }

        public FileEntry Entry { get; set; }

        public EntryNodeViewModel[] Nodes { get; set; }
    }
}