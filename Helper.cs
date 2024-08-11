using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Net;
using System.Windows.Markup;
using System.Text.RegularExpressions;

namespace MTGProxyDesk
{
    public static class Helper
    {
        public static T? GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static T[] GetChildrenOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            List<T> result = new List<T>();
            Action<DependencyObject> _GetChildrenOfType = (DependencyObject d) => { };

            _GetChildrenOfType = (DependencyObject d) => {
                if (depObj == null) return;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
                {
                    var child = VisualTreeHelper.GetChild(d, i);
                    if (child is T)
                    {
                        result.Add(child as T);
                        continue;
                    }
                    _GetChildrenOfType(child);
                }
            };
            _GetChildrenOfType(depObj);

            return result.ToArray();
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

        public static string DownloadImage(Uri uri, string filepath)
        {
            if (File.Exists(filepath)) return String.Format("file:///{0}", filepath);

            WebRequest request = WebRequest.Create(uri);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (var memStream = new MemoryStream())
                {
                    string directory = Path.GetFullPath(Path.GetDirectoryName(filepath)!);
                    if (!Directory.Exists(filepath)) Directory.CreateDirectory(directory);

                    stream.CopyTo(memStream);
                    memStream.Position = 0;
                    File.WriteAllBytes(filepath, memStream.ToArray());
                    return String.Format("file:///{0}", filepath);
                }
            }
        }

        public static string GetFileType(this string filepath)
        {
            Regex reg = new Regex("[^a-z]", RegexOptions.IgnoreCase);
            return reg.Split(filepath.Split('.').Last())[0];
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T search)
        {
            for (int i = 0; i < items.Count(); i++)
            {
                if (EqualityComparer<T>.Default.Equals(items.ElementAt(i), search)) return i;
            }
            return -1;
        }
    }
}
