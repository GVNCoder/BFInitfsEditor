using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using BFInitfsEditor.CustomEventArgs;
using BFInitfsEditor.Extension;

namespace BFInitfsEditor.Controls
{
    public class CancelableTreeView : TreeView
    {
        static CancelableTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CancelableTreeView),
                new FrameworkPropertyMetadata(typeof(TreeView)));
        }

        // Register a routed event, note this event uses RoutingStrategy.Tunnel. per msdn docs
        // all "Preview" events should use tunneling.
        // http://msdn.microsoft.com/en-us/library/system.windows.routedevent.routingstrategy.aspx
        public static RoutedEvent PreviewSelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
            "PreviewSelectedItemChanged",
            RoutingStrategy.Tunnel,
            typeof(CancelEventHandler),
            typeof(CancelableTreeView));

        // give CLR access to routed event
        public event CancelEventHandler PreviewSelectedItemChanged
        {
            add
            {
                AddHandler(PreviewSelectedItemChangedEvent, value);
            }
            remove
            {
                RemoveHandler(PreviewSelectedItemChangedEvent, value);
            }
        }

        // override PreviewMouseDown
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            // determine which item is going to be selected based on the current mouse position
            object itemToBeSelected = this.GetObjectAtPoint<TreeViewItem>(e.GetPosition(this));

            // selection doesn't change if the target point is null (beyond the end of the list)
            // or if the item to be selected is already selected.
            if (itemToBeSelected != null && itemToBeSelected != SelectedItem)
            {
                bool shouldCancel;

                // call our new event
                OnPreviewSelectedItemChanged(out shouldCancel);
                if (shouldCancel)
                {
                    // if we are canceling the selection, mark this event has handled and don't
                    // propogate the event.
                    e.Handled = true;
                    return;
                }
            }

            // otherwise we want to continue normally
            base.OnPreviewMouseDown(e);
        }

        protected virtual void OnPreviewSelectedItemChanged(out bool shouldCancel)
        {
            CancelEventArgs e = new CancelEventArgs();
            if (PreviewSelectedItemChangedEvent != null)
            {
                // Raise our event with our custom CancelRoutedEventArgs
                RaiseEvent(new CancelRoutedEventArgs(PreviewSelectedItemChangedEvent, e));
            }
            shouldCancel = e.Cancel;
        }
    }
}