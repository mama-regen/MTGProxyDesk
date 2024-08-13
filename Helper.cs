using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Net;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;

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

        public static BitmapImage DownloadImage(Uri uri, string filepath)
        {
            if (File.Exists(filepath)) return LoadBitmap(filepath);

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
                    return LoadBitmap(filepath);
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

        public static BitmapImage LoadBitmap(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = fs;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        public static string GetDocumentsFolder()
        {
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MTG_ProxyDesk");
        }

        public static void EnsureDocumentsFolder()
        {
            string myDocsDir = GetDocumentsFolder();
            if (!Path.Exists(myDocsDir))
            {
                Directory.CreateDirectory(myDocsDir);
    
                foreach (string file in new string[] { "cds.py", "py_readme.md" }) {
                    Uri makeUri = new Uri(
                        @"pack://application:,,,/" +
                        Assembly.GetCallingAssembly().GetName().Name +
                        ";component/scripts/" + file,
                        UriKind.Absolute
                    );
                    var resourceInfo = Application.GetResourceStream(makeUri);

                    using (FileStream write = new FileStream(Path.Join(myDocsDir, file == "cds.py" ? "_ConvertDeck.py" : "README.md"), FileMode.Create, FileAccess.Write))
                    {
                        resourceInfo.Stream.CopyTo(write);
                    }
                }
                using (FileStream write = new FileStream(Path.Join(myDocsDir, "ConvertDeck.bat"), FileMode.Create, FileAccess.Write))
                {
                    string fileContent = "python --version>NUL\nif errorlevel 1 goto noPython\n\ngoto:runConverter\n\n";
                    fileContent += ":noPython\nmsg \"%username%\" \"Python 3 is required to run the converter.\"\nexit\n\n";
                    fileContent += ":runConverter\npython -m ensurepip\npython ./_ConvertDeck.py";
                    byte[] byteContent = Encoding.ASCII.GetBytes(fileContent);
                    write.Write(byteContent, 0, byteContent.Length);
                }
            }
        }

        public static Func<T, Task> Debounce<T>(this Func<T, Task> func, int milliseconds = 300)
        {
            CancellationTokenSource? cts = null;

            return (arg) =>
            {
                cts?.Cancel();
                cts = new CancellationTokenSource(milliseconds);

                return Task.Delay(milliseconds, cts.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully) func(arg);
                }, TaskScheduler.Default);
            };
        }

        public static Action<T, U> Debounce<T, U>(this Action<T, U> func, int milliseconds = 300)
        {
            CancellationTokenSource? cts = null;

            return (argA, argB) =>
            {
                cts?.Cancel();
                cts = new CancellationTokenSource(milliseconds);

                Task.Delay(milliseconds, cts.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully) func(argA, argB);
                }, TaskScheduler.Default);
            };
        }

        public static SolidColorBrush AsBrush(this string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(color)!;
        }

        public static Color AsColor(this string color)
        {
            return (Color)ColorConverter.ConvertFromString(color);
        }
    }
}
