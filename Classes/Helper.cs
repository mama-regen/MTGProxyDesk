using System.Windows;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;

namespace MTGProxyDesk
{
    public static class Helper
    {
        public static string DocumentsFolder
        {
            get
            {
                string path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MTG_ProxyDesk");
                if (!Path.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    GenerateDocuments();
                }
                return path;
            }
        }

        public static string TempFolder
        {
            get
            {
                string path = Path.Join(Path.GetTempPath(), "mtg_prox_desk");
                if(!Path.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        public static Uri ResourceUri(string path)
        {
            return new Uri(
                "pack://application:,,,/" +
                Assembly.GetCallingAssembly().GetName().Name +
                ";component/" + path,
                UriKind.Absolute
            );
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

        public static void GenerateDocuments()
        {
            Directory.CreateDirectory(DocumentsFolder);
    
            foreach (string file in new string[] { "cds.py", "py_readme.md" }) {
                Uri makeUri = ResourceUri("scripts/" + file);
                var resourceInfo = Application.GetResourceStream(makeUri);

                using (FileStream write = new FileStream(Path.Join(DocumentsFolder, file == "cds.py" ? "_ConvertDeck.py" : "README.md"), FileMode.Create, FileAccess.Write))
                {
                    resourceInfo.Stream.CopyTo(write);
                }
            }

            foreach (string color in new string[] { "white", "blue", "black", "red", "green" })
            {
                string filename = "playmat_" + color + ".png";
                Uri makeUri = ResourceUri("img/" + filename);
                var resourceInfo = Application.GetResourceStream(makeUri);

                using (FileStream write = new FileStream(Path.Join(DocumentsFolder, "playmat_" + color + ".png"), FileMode.Create, FileAccess.Write))
                {
                    resourceInfo.Stream.CopyTo(write);
                }
            }

            using (FileStream write = new FileStream(Path.Join(DocumentsFolder, "playmat_credits.txt"), FileMode.Create, FileAccess.Write))
            {
                string fileContent = "playmat_blue: https://pixabay.com/illustrations/photoshop-manipulation-fantazy-1617999/\n" +
                "playmat_red: https://pixabay.com/illustrations/lava-cracked-background-fire-656827/\n" +
                "playmat_white: https://pixabay.com/illustrations/ai-generated-tree-spirit-spirit-8831760/\n" +
                "playmat_green: https://pixabay.com/photos/fantasy-forest-fairies-night-7452256/\n" +
                "playmat_black: https://pixabay.com/illustrations/background-middle-ages-village-road-7632590/";
                byte[] byteContent = Encoding.ASCII.GetBytes(fileContent);
                write.Write(byteContent, 0, byteContent.Length);
            }

            using (FileStream write = new FileStream(Path.Join(DocumentsFolder, "ConvertDeck.bat"), FileMode.Create, FileAccess.Write))
            {
                string fileContent = "python --version>NUL\nif errorlevel 1 goto noPython\n\ngoto:runConverter\n\n";
                fileContent += ":noPython\nmsg \"%username%\" \"Python 3 is required to run the converter.\"\nexit\n\n";
                fileContent += ":runConverter\npython -m ensurepip\npython ./_ConvertDeck.py";
                byte[] byteContent = Encoding.ASCII.GetBytes(fileContent);
                write.Write(byteContent, 0, byteContent.Length);
            }
        }
    }

    public class Cache<T> where T : DependencyObject
    {
        private Func<T> _get = () => throw new Exception("Cache getter func not set!");
        private Cache() { }
        private T? _value = null;

        public T Value
        {
            get
            {
                if (_value == null) { _value = _get(); }
                return _value;
            }
        }

        public static Cache<T> Init(Func<T> getter)
        {
            Cache<T> cache = new Cache<T>();
            cache._get = getter;
            return cache;
        }

        public bool Clear()
        {
            if (_value == null) return false;
            _value = null;
            return true;
        }
    }
}
