using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MTGProxyDesk.Extensions
{
    public static class DependencyObjectExt
    {
        public static T? GetChildOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (!(child is T)) child = GetChildOfType<T>(child);
                if (child != null) return (T)child;
            }
            return null;
        }

        public static IEnumerable<T> GetChildrenOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            List<T> children = new List<T>();
            GetChildrenOfType(obj, children);
            return children;
        }

        public static T? GetAncestorOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = obj;
            while (parent != null)
            {
                if (parent is T) return (T)parent;
                parent = LogicalTreeHelper.GetParent(parent);
            }

            return null;
        }

        public static T Copy<T>(this T control) where T : DependencyObject
        {
            return (T)XamlReader.Parse(XamlWriter.Save(control));
        }

        private static void GetChildrenOfType<T>(DependencyObject obj, List<T> childList) where T : DependencyObject
        {
            if (obj == null) return;

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child is T) childList.Add((T)child);
                else GetChildrenOfType<T>(child, childList);
            }
        }
    }
}
