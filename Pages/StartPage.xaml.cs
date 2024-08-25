using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using MTGProxyDesk.Classes;

namespace MTGProxyDesk
{
    public partial class StartPage : Page, INotifyPropertyChanged
    {
        private string filePath = "";

        public string FileName
        {
            get => Path.GetFileName(filePath);
        }

        private BitmapImage _backgroundImage = new BitmapImage();
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

            if (App.StartBG == null) BackgroundImage = new BitmapImage(Helper.ResourceUri("img/playmat_default.png"));
            else
            {
                BackgroundImage = App.StartBG!.Value.Image;
                ArtistCredit.Content = "Artist: " + App.StartBG!.Value.Artist;
            }

            Application.Current.MainWindow.WindowState = WindowState.Normal;
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
            NavigationService.Navigate(new NewDeckPage());
        }

        public void EditDeck(object sender, RoutedEventArgs e)
        {
            MagicDeck newDeck = new MagicDeck();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => newDeck.Load(filePath, (BackgroundWorker)sender!);
            worker.ProgressChanged += (sender, e) =>
            {
                LoadProgress.Value = 100 - e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (_, __) => NavigationService.Navigate(new NewDeckPage(newDeck));
            worker.RunWorkerAsync();
        }

        public void BeginPlay(object sender, RoutedEventArgs e)
        {
            MagicDeck playDeck = new MagicDeck();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => playDeck.Load(filePath, (BackgroundWorker)sender!);
            worker.ProgressChanged += (sender, e) =>
            {
                LoadProgress.Value = 100 - e.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (_, __) => NavigationService.Navigate(new PlayMat(playDeck));
            worker.RunWorkerAsync();
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
