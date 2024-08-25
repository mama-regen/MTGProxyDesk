using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MTGProxyDesk.Windows
{
    public partial class MainWindow : BaseWindow
    {
        public static (double, double) WindowSize { get; private set; }

        public MainWindow() : base()
        {
            InitializeComponent();
            CanClose = true;
            this.SizeChanged += OnWindowSizeChanged;
        }

        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSize = (e.NewSize.Width, e.NewSize.Height);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            
            foreach (Window window in Application.Current.Windows)
            {
                if (window is BaseWindow && !this.Equals(window))
                {
                    ((BaseWindow)window).CanClose = true;
                    window.Close();
                }
            }

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