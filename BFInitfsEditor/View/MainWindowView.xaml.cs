using System.IO;
using System.Windows;

using BFInitfsEditor.Data;
using BFInitfsEditor.Model;

namespace BFInitfsEditor.View
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();

            // Tests
            var fileContent = File.ReadAllBytes("initfs_Linux");
            var reader = InitfsReader.GetInstance();
            var writer = InitfsWriter.GetInstance();

            Entity entity = null;
            using (var memoryStream = new MemoryStream(fileContent, false))
                entity = reader.Read(memoryStream);

            using (var file = File.OpenWrite("initfs_Linux_new"))
                writer.Write(file, entity);
        }
    }
}
