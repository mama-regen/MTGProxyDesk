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
    }
}
