using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.ComponentModel;
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

        private BitmapImage _backgroundImage;
        public BitmapImage BackgroundImage
        {
            get => _backgroundImage;
            private set 
            {
                _backgroundImage = value;
                OnPropertyChanged("BackgroundImage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public StartPage()
        {
            InitializeComponent();
            DataContext = this;

            Helper.EnsureDocumentsFolder();
            _backgroundImage = new BitmapImage(new Uri("pack://application:,,,/img/bg.png"));
            GetBackgroundImage();

            magicDeck = MagicDeck.Instance;
            Application.Current.MainWindow.WindowState = WindowState.Normal;

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
                DeckName.Content = Path.GetFileName(filePath);

                NoDeckLoaded.Visibility = Visibility.Collapsed;
                NoDeckLoaded.IsEnabled = false;
                DeckLoaded.Visibility = Visibility.Visible;
                DeckLoaded.IsEnabled = true;
            }
        }

        public void NewDeck(object sender, RoutedEventArgs e)
        {
            magicDeck.ClearDeck();
            NavigationService.Navigate(new NewDeckPage());
        }

        public void EditDeck(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => magicDeck.Load(filePath == "" ? magicDeck.FilePath : filePath, (BackgroundWorker)sender!);
            worker.ProgressChanged += (sender, e) =>
            {
                LoadProgress.Value = 100 - e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (_, __) => NavigationService.Navigate(new NewDeckPage());
            worker.RunWorkerAsync();
        }

        public void BeginPlay(object sender, RoutedEventArgs e)
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GetBackgroundImage()
        {
            (BitmapImage, string)? bg = await Card.GetRandomArt();
            if (bg == null) return;
            BackgroundImage = bg.Value.Item1;
            ArtistCredit.Content = "Artist: " + bg.Value.Item2;
        }

        private void MPDButton_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
