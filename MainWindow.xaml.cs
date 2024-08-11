using System.IO;
using System.Windows;

namespace MTGProxyDesk
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnWindowClosing!;
        }

        public void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string tempFolder = System.IO.Path.Join(System.IO.Path.GetTempPath(), "mtg_prox_desk");
            if (Directory.Exists(tempFolder))
            {
                try { Directory.Delete(tempFolder, true); } catch { }
            }
        }
    }
}