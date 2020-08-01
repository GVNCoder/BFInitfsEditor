using Microsoft.Win32;

namespace BFInitfsEditor.Data
{
    public struct DialogResult
    {
        public bool IsClosed { get; set; }
        public bool? DialogResultRef { get; set; }
        public string FileName { get; set; }
    }

    public static class DialogHelper
    {
        public static DialogResult OpenFileDialog(string title, string filter)
        {
            // create dialog instance
            var dialogInstance = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                FileName = string.Empty
            };

            // show dialog and wait return
            var workResult = dialogInstance.ShowDialog();
            // create dialog result
            var dialogResult = new DialogResult
            {
                DialogResultRef = workResult,
                IsClosed = !workResult.GetValueOrDefault(),
                FileName = dialogInstance.FileName
            };

            return dialogResult;
        }
    }
}