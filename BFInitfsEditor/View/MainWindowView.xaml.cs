using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BFInitfsEditor.Data;

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
            using (var file = File.OpenRead("initfs_Linux"))
            {
                var reader = InitfsReader.GetInstance();
                var entity = reader.Read(file);
            }
        }
    }
}
