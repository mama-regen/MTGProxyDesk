using MTGProxyDesk.Classes;
using MTGProxyDesk.Windows;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MTGProxyDesk
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            MagicDeck.Instance.Clear();
            Hand.Instance.Clear();
            Graveyard.Instance.Clear();
            Exile.Instance.Clear();
            HandDisplay.CloseInstance();

            try { Directory.Delete(Helper.TempFolder, true); } catch 
            {
                try
                {
                    foreach (FileInfo file in new DirectoryInfo(Helper.TempFolder).GetFiles()) file.Delete();
                } catch { }
            }
        }
    }
}