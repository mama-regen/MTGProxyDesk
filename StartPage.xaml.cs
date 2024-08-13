using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MTGProxyDesk
{
    public partial class StartPage : Page, INotifyPropertyChanged
    {
        private MagicDeck magicDeck;
        private string filePath = "";

        public string FileName
        {
            get
            {
                if (filePath != "") return Path.GetFileName(filePath);
                return magicDeck.Name;
            }
            set { }
        }

        private SolidColorBrush _bg1 = Constants.Colors["Background1"].AsBrush();
        public SolidColorBrush BG1 { get => _bg1; }

        private SolidColorBrush _bg2 = Constants.Colors["Background2"].AsBrush();
        public SolidColorBrush BG2 { get => _bg2; }

        private SolidColorBrush _fg1 = Constants.Colors["Foreground1"].AsBrush();
        public SolidColorBrush FG1 { get => _fg1; }

        private SolidColorBrush _fg2 = Constants.Colors["Foreground2"].AsBrush();
        public SolidColorBrush FG2 { get => _fg2; }

        private LinearGradientBrush _loaderGradient = new LinearGradientBrush();
        public LinearGradientBrush LoaderGradient { get => _loaderGradient; }

        private BitmapImage _backgroundImage;
        public BitmapImage BackgroundImage
        {
            get => _backgroundImage;
            private set 
            {
                _backgroundImage = value;
                this.OnPropertyChanged("BackgroundImage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public StartPage()
        {
            InitializeComponent();
            this.DataContext = this;

            Helper.EnsureDocumentsFolder();
            _backgroundImage = new BitmapImage(new Uri("pack://application:,,,/img/bg.png"));
            GetBackgroundImage();

            magicDeck = MagicDeck.Instance;
            Application.Current.MainWindow.WindowState = WindowState.Normal;

            Random rnd = new Random();
            bool trueColors = rnd.Next(1, 10) == 1;

            _loaderGradient.StartPoint = new Point(0, 0);
            _loaderGradient.EndPoint = new Point(1, 0);

            int i = 0;
            foreach (string name in new string[] { "Green", "Red", "Black", "Blue", "White" })
            {
                GradientStop gs = new GradientStop();
                gs.Color = Constants.Colors[name + (trueColors ? "True" : "")].AsColor();
                gs.Offset = 0.25 * i;
                _loaderGradient.GradientStops.Add(gs);
                i++;
            }
            OnPropertyChanged("LoaderGradient");

            if (magicDeck.Name != "")
            {
                NoDeckLoaded.Visibility = Visibility.Collapsed;
                NoDeckLoaded.IsEnabled = false;
                DeckLoaded.Visibility = Visibility.Visible;
                DeckLoaded.IsEnabled = true;
            }
        }

        public void BrowseDeck(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Helper.GetDocumentsFolder();
            ofd.Filter = "MTG Proxy Deck (*.mpd)|*.mpd|All Files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
                OnPropertyChanged("FileName");
                DeckName.Text = Path.GetFileName(filePath);

                NoDeckLoaded.Visibility = Visibility.Collapsed;
                NoDeckLoaded.IsEnabled = false;
                DeckLoaded.Visibility = Visibility.Visible;
                DeckLoaded.IsEnabled = true;
            }
        }

        public void NewDeck(object sender, RoutedEventArgs e)
        {
            magicDeck.ClearDeck();
            EditDeck(sender, e);
        }

        public void EditDeck(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.ToString() == "Edit Deck")
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += (sender, e) => magicDeck.Load(filePath == "" ? magicDeck.FilePath : filePath, (BackgroundWorker)sender!);
                worker.ProgressChanged += (sender, e) =>
                {
                    LoadProgress.Value = 100 - e.ProgressPercentage;
                };
                worker.RunWorkerCompleted += (_, __) => this.NavigationService.Navigate(new NewDeckPage());
                worker.RunWorkerAsync();
            } else this.NavigationService.Navigate(new NewDeckPage());
        }

        public void BeginPlay(object sender, RoutedEventArgs e)
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = this.PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GetBackgroundImage()
        {
            (BitmapImage, string)? bg = await Card.GetRandomArt();
            if (bg == null) return;
            BackgroundImage = bg.Value.Item1;
            ArtistCredit.Content = "Artist: " + bg.Value.Item2;
        }
    }
}
