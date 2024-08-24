using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using MTGProxyDesk.Windows;
using MTGProxyDesk.Classes;

namespace MTGProxyDesk
{
    public partial class StartPage : Page, INotifyPropertyChanged
    {
        private string filePath = "";

        public string FileName
        {
            get
            {
                if (filePath != "") return Path.GetFileName(filePath);
                return MagicDeck.Instance.Name;
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

            HandDisplay.CloseInstance();
            CardViewer.CloseInstance();

            _backgroundImage = new BitmapImage(new Uri("pack://application:,,,/img/bg.png"));
            GetBackgroundImage();

            Application.Current.MainWindow.WindowState = WindowState.Normal;

            if (MagicDeck.Instance.Name != "")
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
            MagicDeck.Instance.Clear();
            NavigationService.Navigate(new NewDeckPage());
        }

        public void EditDeck(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => MagicDeck.Instance.Load(filePath == "" ? MagicDeck.Instance.FilePath : filePath, (BackgroundWorker)sender!);
            worker.ProgressChanged += (sender, e) =>
            {
                LoadProgress.Value = 100 - e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (_, __) => NavigationService.Navigate(new NewDeckPage());
            worker.RunWorkerAsync();
        }

        public void BeginPlay(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => MagicDeck.Instance.Load(filePath == "" ? MagicDeck.Instance.FilePath : filePath, (BackgroundWorker)sender!);
            worker.ProgressChanged += (sender, e) =>
            {
                LoadProgress.Value = 100 - e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (_, __) => NavigationService.Navigate(PlayMat.Instance);
            worker.RunWorkerAsync();
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
