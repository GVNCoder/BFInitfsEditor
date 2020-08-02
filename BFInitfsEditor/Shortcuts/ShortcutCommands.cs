using System.Windows.Input;

namespace BFInitfsEditor.Shortcuts
{
    public static class ShortcutCommands
    {
        // Window shortcuts
        public static readonly RoutedUICommand OpenFileCommand = new RoutedUICommand();
        public static readonly RoutedUICommand SaveAsCommand = new RoutedUICommand();

        // TextEditor shortcuts
        public static readonly RoutedUICommand QuickSaveCommand = new RoutedUICommand();
    }
}