using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using BFInitfsEditor.Data;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;
using BFInitfsEditor.ViewModels;
using ICSharpCode.AvalonEdit.Document;

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

            //using (var file = File.OpenWrite("initfs_Linux_new"))
            //    writer.Write(file, entity);

            _writer = InitfsWriter.GetInstance();
            _reader = InitfsReader.GetInstance();
        }

        #region Fields

        private readonly IInitfsWriter _writer;
        private readonly IInitfsReader _reader;

        private Entity _entity;
        private IEnumerable<EntryViewModel> _itemsSource;

        #endregion

        #region Wnd Handlers

        private void _WndLoadedHandler(object sender, RoutedEventArgs e)
        {
            // TODO: Load resources
        }

        #endregion

        #region Menu commands Handlers

        /// <summary>
        /// "Open" menu click handler
        /// </summary>
        private void _OpenMenuClickHandler(object sender, RoutedEventArgs e)
        {
            var dialogResult = DialogHelper.OpenFileDialog("Select initfs_ file", "All (*.*)|*.*");

            // results validation
            if (dialogResult.IsClosed) return;

            var fileExtension = Path.GetExtension(dialogResult.FileName);
            if (! string.IsNullOrEmpty(dialogResult.FileName) && string.IsNullOrEmpty(fileExtension))
            {
                _HandleFile(dialogResult.FileName);
            }
            else // if file is not specified or invalid file specified
            {
                MessageBox.Show("File is not specified or invalid file specified", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _HandleFile(string path)
        {
            try
            {
                // parse entity
                _entity = _ReadEntity(path);
                _itemsSource = _entity.Data.Entries.Select(e => new EntryViewModel { ID = e.ID, FullPath = e.FilePath, Entry = e });

                UITreeView.ItemsSource = _itemsSource;

                // TODO: Create normal treeView
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Entity _ReadEntity(string path)
        {
            var fileContent = File.ReadAllBytes(path);

            Entity entity;
            using (var memoryStream = new MemoryStream(fileContent, false))
                entity = _reader.Read(memoryStream);

            return entity;
        }

        /// <summary>
        /// "Save" menu click handler
        /// </summary>
        private void _SaveMenuClickHandler(object sender, RoutedEventArgs e)
        {
            // TODO: Create Ctrl + S quick save
        }

        /// <summary>
        /// "About" menu click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _AboutMenuClickHandler(object sender, RoutedEventArgs e)
        {
            // TODO: Handle click
        }

        #endregion

        private void _TreeViewSelectionChangedHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // TODO: Ask to save it if changes was detected

            var item = (EntryViewModel) e.NewValue;
            var documentString = Encoding.ASCII.GetString(item.Entry.FileData);
            var document = new TextDocument(documentString);

            UITextEditor.Document = document;
        }
    }
}
