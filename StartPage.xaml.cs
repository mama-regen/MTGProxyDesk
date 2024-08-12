using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.ComponentModel;

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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public StartPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Helper.EnsureDocumentsFolder();
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
    }
}
