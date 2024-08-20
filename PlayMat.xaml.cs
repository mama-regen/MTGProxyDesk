using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using MTGProxyDesk.Windows;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MTGProxyDesk
{
    public partial class PlayMat : Page, INotifyPropertyChanged
    {
        private Cache<CardControl> _deckControl;
        private CardControl deckControl { get => _deckControl.Value; }
        private Cache<CardControl> _commanderControl;
        private CardControl commanderControl { get => _commanderControl.Value; }
        private Cache<CardControl> _graveyardControl;
        private CardControl graveyardControl { get => _graveyardControl.Value; }
        private Cache<CardControl> _exileControl;
        private CardControl exileControl { get => _exileControl.Value; }

        private int drawNmb { get; set; } = 1;
        private int showNextNmb { get; set; } = 1;

        private ImageBrush? _backgroundImage = null;
        public ImageBrush? BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                _backgroundImage = value;
                OnPropertyChanged("BackgroundImage");
            }
        }

        private ImageBrush? _showCard = null;
        public ImageBrush? ShowCard
        {
            get => _showCard;
            set
            {
                _showCard = value;
                OnPropertyChanged("ShowCard");
            }
        }

        private Visibility _showCardVisibility = Visibility.Collapsed;
        public Visibility ShowCardVisibility
        {
            get => _showCardVisibility;
            set
            {
                _showCardVisibility = value;
                OnPropertyChanged("ShowCardVisibility");
            }
        }

        public bool IsCommander
        {
            get => MagicDeck.Instance.Commander != null;
        }

        public bool ShowExile
        {
            get => Exile.Instance.CardCount > 0;
        }

        public bool ShowGraveyard
        {
            get => Exile.Instance.CardCount > 0;
        }

        public bool ShowDeck
        {
            get => MagicDeck.Instance.CardShuffle.Length > 0;
        }

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public PlayMat()
        {
            InitializeComponent();

            DataContext = this;
            _deckControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("DeckControl")!);
            _commanderControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("CommanderControl")!);
            _graveyardControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("GraveyardControl")!);
            _exileControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("ExileControl")!);

            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, Setup);
        }

        public void DrawCard(object sender, RoutedEventArgs e)
        {

        }

        public void NextCard(object sender, RoutedEventArgs e)
        {

        }

        public void UpdateDeckViewCount(int value, object sender)
        {
            showNextNmb = value;
        }

        public void UpdateDeckDrawCount(int value, object sender)
        {
            drawNmb = value;
        }

        public void ShuffleDeck(object sender, RoutedEventArgs e)
        {
            MagicDeck.Instance.Shuffle();
        }

        private void Setup()
        {
            GetPlaymatImage();
            for (int _ = 0; _ < 3; _++) AddMainRow();
            if (IsCommander)
            {
                CardHole right2 = GetByUid<CardHole>("Right_2")!;
                right2.IsEnabled = false;
                right2.Visibility = Visibility.Collapsed;

                commanderControl.Visibility = Visibility.Visible;
                commanderControl.Card = MagicDeck.Instance.Commander;
            }

            HandDisplay.Instance.Show();

            // TESTING
            for (int i = 0; i < 7; i++) Hand.Instance.AddCard(MagicDeck.Instance.Draw());
            // TESTING

            HandDisplay.Instance.DisplayHand();
        }

        private void AddMainRow()
        {
            Grid newRow = new Grid();
            newRow.Width = 1535;
            newRow.Height = 270;
            newRow.Margin = new Thickness(0);
            newRow.HorizontalAlignment = HorizontalAlignment.Center;
            newRow.VerticalAlignment = VerticalAlignment.Top;
            for (int i = 0; i < 8; i++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                newRow.ColumnDefinitions.Add(colDef);

                CardHole hole = new CardHole();
                hole.Margin = new Thickness(6);
                Grid.SetColumn(hole, i);
                newRow.Children.Add(hole);
            }
            MainArea.Children.Add(newRow);
            if (MainArea.Children.Count > 3)
            {
                MainArea.GetAncestorOfType<ScrollViewer>()!.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private void GetPlaymatImage()
        {
            DirectoryInfo dir = new DirectoryInfo(Helper.DocumentsFolder);
            string[] accept = new string[] { ".png", ".jpg", ".jpeg" };
            FileInfo[] playmat = dir
                .GetFiles("*.*")
                .Where(f => accept.Contains(f.Extension.ToLower()) && f.Name.Replace(f.Extension, "").StartsWith("playmat"))
                .ToArray();

            if (playmat.Length > 0) 
            {
                Random rand = new Random();
                BackgroundImage = new ImageBrush(Helper.LoadBitmap(playmat[rand.Next(playmat.Length - 1)].FullName));
                return;
            }
            BackgroundImage = new ImageBrush(new BitmapImage(Helper.ResourceUri("img/playmat_default.png")));
        }

        private T? GetByUid<T>(string uid) where T : DependencyObject
        {
            var x = this.GetChildOfType<T>();
            return this.GetChildrenOfType<T>().Where(c =>
            {
                PropertyInfo? prop = c.GetType().GetProperty("Uid");
                if (prop == null || prop.PropertyType != typeof(string)) return false;
                return prop.GetValue(c)!.ToString() == uid;
            }).FirstOrDefault();
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
