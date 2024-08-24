using MTGProxyDesk.Classes;
using MTGProxyDesk.Windows;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MTGProxyDesk
{
    public partial class MainWindow : Window
    {
        private static double _windowWidth = 0;
        private static double _windowHeight = 0;
        public static (double, double) WindowSize { get => (_windowWidth, _windowHeight); }
        public static Action BringToFront = () => { };

        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += OnWindowSizeChanged;
            BringToFront = () =>
            {
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            };
        }

        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowWidth = e.NewSize.Width;
            _windowHeight = e.NewSize.Height;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            MagicDeck.Instance.Clear();
            Hand.Instance.Clear();
            Graveyard.Instance.Clear();
            Exile.Instance.Clear();
            HandDisplay.CloseInstance();
            CardViewer.CloseInstance();

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