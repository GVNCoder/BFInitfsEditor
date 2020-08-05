using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

using BFInitfsEditor.Annotations;
using BFInitfsEditor.Data;
using BFInitfsEditor.Model;
using BFInitfsEditor.Service;
using BFInitfsEditor.ViewModels;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace BFInitfsEditor.View
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window, INotifyPropertyChanged
    {
        public MainWindowView()
        {
            InitializeComponent();

            // setup some settings
            UITextEditor.Encoding = Encoding.ASCII;
            UITextEditor.TextArea.SelectionChanged += _TextEditorSelectionChangedHandler;

            // create some services
            _writer = InitfsWriter.GetInstance();
            _reader = InitfsReader.GetInstance();

            // setup some initial values
            _isFileLoaded = false;
            _isEdited = false;

            // setup changeable props
            _storageSelected = 0;
            _storageSizeControl = 0;
            _storageEdit = string.Empty;
        }

        #region Fields

        private readonly IInitfsWriter _writer;
        private readonly IInitfsReader _reader;

        private Entity _entity;
        private IEnumerable<EntryViewModel> _itemsSource;

        private bool _isFileLoaded;
        private bool _isEdited;

        private ulong _currentEditEntrySize;
        private ulong _currentEditEntryOriginalSize;

        #endregion

        #region INotifyPropertyChanged impl

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void _OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void _SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (storage.Equals(value)) return;

            storage = value;
            _OnPropertyChanged(propertyName);
        }

        #endregion

        #region Changeble props

        private int _storageSelected;
        private int _storageSizeControl;
        private string _storageEdit;

        /// <summary>
        /// Selected chars in current document
        /// </summary>
        public int Selected
        {
            get => _storageSelected;
            set => _SetProperty(ref _storageSelected, value);
        }

        /// <summary>
        /// Difference between untouched entry and modified entry
        /// </summary>
        public int SizeControl
        {
            get => _storageSizeControl;
            set => _SetProperty(ref _storageSizeControl, value);
        }

        /// <summary>
        /// Current document full name
        /// </summary>
        public string Edit
        {
            get => _storageEdit;
            set => _SetProperty(ref _storageEdit, value);
        }


        #endregion

        #region Shortcut commands redirections

        private void _OpenFileCommandExecuted(object sender, ExecutedRoutedEventArgs e)
            => UIOpenFileItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        private void _SaveAsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
            => UISaveAsItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        private void _QuickSaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
            => _QuickSaveHandler();

        #endregion

        #region Wnd Handlers

        /// <summary>
        /// Window loaded Handler
        /// </summary>
        private void _WndLoadedHandler(object sender, RoutedEventArgs e)
        {
            // TODO: Load resources
        }

        /// <summary>
        /// Window closing Handler
        /// </summary>
        private void _WndClosingHandler(object sender, CancelEventArgs e)
        {
            // if we have not saved changes
            if (_isEdited)
            {
                var dialogResult = MessageBox.Show("The file has been modified. Save it?", "Initfs Editor",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    // call to menu handler
                    _SaveMenuClickHandler(sender, null);
                }
            }

            e.Cancel = false;
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
                _HandleOpenFile(dialogResult.FileName);
            }
            else // if file is not specified or invalid file specified
            {
                MessageBox.Show("File is not specified or invalid file specified", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _HandleOpenFile(string path)
        {
            try
            {
                // parse entity
                _entity = _ReadEntity(path);
                _itemsSource = _entity.Data.Entries.Select(e => new EntryViewModel { ID = e.ID, FullPath = e.FilePath, Entry = e });

                UITreeView.ItemsSource = _itemsSource;

                _isFileLoaded = true;

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
            // if not file to save
            if (! _isFileLoaded) return;

            var dialogResult = DialogHelper.SaveFileDialog("Save file", "All (*.*)|*.*");

            // results validation
            if (dialogResult.IsClosed) return;

            var fileExtension = Path.GetExtension(dialogResult.FileName);
            if (!string.IsNullOrEmpty(dialogResult.FileName) && string.IsNullOrEmpty(fileExtension))
            {
                _HandleSaveFile(dialogResult.FileName);
                _isEdited = false;
            }
            else // if file is not specified or invalid file specified
            {
                MessageBox.Show("File is not specified or invalid file specified", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _HandleSaveFile(string path)
        {
            try
            {
                // write result initfs_ to selected file
                using (var file = File.OpenWrite(path))
                    _writer.Write(file, _entity);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// "About" menu click handler
        /// </summary>
        private void _AboutMenuClickHandler(object sender, RoutedEventArgs e)
        {
            // TODO: Handle click
        }

        #endregion

        #region TreeView Handlers

        /// <summary>
        /// TreeView Preview selection changed Handler
        /// </summary>
        private void _TreeViewPreviewSelectionChangedHandler(object sender, CancelEventArgs e)
        {
            var difference = _currentEditEntryOriginalSize - _currentEditEntrySize;
            if (difference != 0)
            {
                // show error
                MessageBox.Show("Edited entry size doesn't match with original entry", "Size error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // cancel selection
                e.Cancel = true;
            }
            else
            {
                // allow selection changed event
                e.Cancel = false;
            }
        }

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
                    _isEdited = true;
                }
            }

            _LoadEntryFileToTextEditor(newItem.Entry);
            _currentEditEntrySize = newItem.Entry.FileSize;
            _currentEditEntryOriginalSize = _currentEditEntrySize;

            UITextEditor.IsEnabled = true;
            UITextEditor.IsModified = false;

            Edit = newItem.FullPath;
        }

        private void _LoadEntryFileToTextEditor(FileEntry entry)
        {
            // unsubscribe changes tracker from old document
            var oldDocument = UITextEditor.Document;
            if (oldDocument != null)
            {
                oldDocument.Changing -= _TextEditorDocumentChangedHandler;
            }

            // create new document
            var textContent = Encoding.ASCII.GetString(entry.FileData);
            var document = _CreateDocumentFromString(textContent);

            UITextEditor.Document = document;
        }

        private TextDocument _CreateDocumentFromString(string content)
        {
            var document = new TextDocument(content);
            document.Changing += _TextEditorDocumentChangedHandler;

            return document;
        }

        #endregion

        #region TextEditor Handlers

        /// <summary>
        /// TextEditor QuickSave shortcut Handler
        /// </summary>
        private void _QuickSaveHandler()
        {
            var item = (EntryViewModel) UITreeView.SelectedItem;
            var isEdited = UITextEditor.IsModified;

            if (isEdited)
            {
                _SaveEntry(item.Entry, UITextEditor.Text);
                _isEdited = true;
            }

            UITextEditor.IsModified = false;
        }

        /// <summary>
        /// TextEditor Selection changed Handler
        /// </summary>
        private void _TextEditorSelectionChangedHandler(object sender, EventArgs e)
        {
            var textArea = (TextArea) sender;
            Selected = textArea.Selection.Length;
        }

        /// <summary>
        /// TextEditor document changes Handler
        /// </summary>
        private void _TextEditorDocumentChangedHandler(object sender, DocumentChangeEventArgs e)
        {
            // calculate changes size
            _currentEditEntrySize -= (ulong) e.RemovalLength;
            _currentEditEntrySize += (ulong) e.InsertionLength;

            // calculate difference
            SizeControl = (int)(_currentEditEntryOriginalSize - _currentEditEntrySize);
        }

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
                entry.FileSize -= (ulong) Math.Abs(sizeDifference);
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
