using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BFInitfsEditor.Extension
{
    public static class ItemsControlExtension
    {
        // get the object that exists in the container at the specified point.
        public static object GetObjectAtPoint<ItemContainer>(this ItemsControl control, Point p)
            where ItemContainer : DependencyObject
        {
            // ItemContainer - can be ListViewItem, or TreeViewItem and so on(depends on control)
            ItemContainer obj = GetContainerAtPoint<ItemContainer>(control, p);
            if (obj == null)
                return null;

            // it is worth noting that the passed _control_ may not be the direct parent of the
            // container that exists at this point. This can be the case in a TreeView, where the
            // parent of a TreeViewItem may be either the TreeView or a intermediate TreeViewItem
            ItemsControl parentGenerator = obj.GetParentItemsControl();

            // hopefully this isn't possible?
            if (parentGenerator == null)
                return null;

            return parentGenerator.ItemContainerGenerator.ItemFromContainer(obj);
        }

        // use the VisualTreeHelper to find the container at the specified point.
        public static ItemContainer GetContainerAtPoint<ItemContainer>(this ItemsControl control, Point p)
            where ItemContainer : DependencyObject
        {
            HitTestResult result = VisualTreeHelper.HitTest(control, p);
            DependencyObject obj = result.VisualHit;

            while (VisualTreeHelper.GetParent(obj) != null && !(obj is ItemContainer))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            // Will return null if not found
            return obj as ItemContainer;
        }

        // walk up the visual tree looking for the nearest ItemsControl parent of the specified
        // depObject, returns null if one isn't found.
        public static ItemsControl GetParentItemsControl(this DependencyObject depObject)
        {
            DependencyObject obj = VisualTreeHelper.GetParent(depObject);
            while (VisualTreeHelper.GetParent(obj) != null && !(obj is ItemsControl))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            // will return null if not found
            return obj as ItemsControl;
        }
    }
}