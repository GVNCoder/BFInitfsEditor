using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;

using BFInitfsEditor.Data;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;
using BFInitfsEditor.ViewModels;

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

            // setup some settings
            UITextEditor.Encoding = Encoding.ASCII;

            // create some services
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

        /// <summary>
        /// Window loaded Handler
        /// </summary>
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

            //using (var file = File.OpenWrite("initfs_Linux_new"))
            //    writer.Write(file, entity);
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

        #region TreeView Handlers

        /// <summary>
        /// TreeView Selection changed Handler
        /// </summary>
        private void _TreeViewSelectionChangedHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var oldItem = (EntryViewModel) e.OldValue;
            var newItem = (EntryViewModel) e.NewValue;
            var isEdited = UITextEditor.IsModified;

            // ask to save edited file
            if (isEdited)
            {
                var dialogResult = MessageBox.Show($"The file \"{oldItem.FullPath}\" has been modified, save it?",
                    "Save", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    _SaveEntry(oldItem.Entry, UITextEditor.Text);
                }
            }

            _LoadEntryFileToTextEditor(newItem.Entry);

            UITextEditor.IsEnabled = true;
            UITextEditor.IsModified = false;
        }

        private void _LoadEntryFileToTextEditor(FileEntry entry)
        {
            var document = Encoding.ASCII.GetString(entry.FileData);
            UITextEditor.Text = document;
        }

        #endregion

        #region TextEditor Handlers



        #endregion

        #region Shared helpers

        public void _SaveEntry(FileEntry entry, string newContent)
        {
            var bytes = Encoding.ASCII.GetBytes(newContent);
            var sizeDifference = bytes.Length - (long) entry.FileSize;

            // save new data content
            entry.FileData = bytes;
            // fix size difference
            if (sizeDifference < 0)
            {
                entry.StructSize -= (ulong) Math.Abs(sizeDifference);
                entry.FileSize -= (ulong)Math.Abs(sizeDifference);
            }
            else
            {
                entry.StructSize += (ulong) sizeDifference;
                entry.FileSize += (ulong) sizeDifference;
            }
        }

        #endregion
    }
}
