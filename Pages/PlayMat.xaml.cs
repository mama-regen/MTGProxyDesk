using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using MTGProxyDesk.Windows;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MTGProxyDesk
{
    public partial class PlayMat : Page, INotifyPropertyChanged
    {
        private static PlayMat? _instance = null;
        private static object  _lock = new object();
        public static PlayMat Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PlayMat();
                    }
                    return _instance;
                }
            }
        }

        private Cache<CardControl> _deckControl;
        public CardControl DeckControl { get => _deckControl.Value; }
        private Cache<CardHole> _commanderSlot;
        public CardHole CommanderSlot { get => _commanderSlot.Value; }
        private Cache<CardControl> _graveyardControl;
        public CardControl GraveyardControl { get => _graveyardControl.Value; }
        private Cache<CardControl> _exileControl;
        public CardControl ExileControl { get => _exileControl.Value; }

        private Cache<NumberPicker> _viewPicker;
        private NumberPicker ViewPicker { get => _viewPicker.Value; }
        private Cache<NumberPicker> _drawPicker;
        private NumberPicker DrawPicker { get => _drawPicker.Value; }

        private Cache<CardControl> _heldCard;
        public CardControl HeldCard { get => _heldCard.Value; }

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

        private PlayMat()
        {
            InitializeComponent();

            DataContext = this;
            _deckControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("DeckControl")!);
            _commanderSlot = Cache<CardHole>.Init(() => GetByUid<CardHole>("Right_2")!);
            _graveyardControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("GraveyardControl")!);
            _exileControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("ExileControl")!);

            _viewPicker = Cache<NumberPicker>.Init(() => GetByUid<NumberPicker>("ViewPicker")!);
            _drawPicker = Cache<NumberPicker>.Init(() => GetByUid<NumberPicker>("DrawPicker")!);
            _heldCard = Cache<CardControl>.Init(() => GetByUid<CardControl>("HeldCard")!);

            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, Setup);
        }

        public void PlayCommander(object sender, RoutedEventArgs e)
        {
            CommanderSlot.PhaseOut();
            HeldCard.Card = CommanderSlot.AssignedCard;
        }

        public void DrawCard(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Math.Min(drawNmb, MagicDeck.Instance.CardCount); i++)
            {
                Hand.Instance.AddCard(MagicDeck.Instance.Draw());
            }
            HandDisplay.Instance.DisplayHand();
            HandDisplay.BringToFront();
            drawNmb = 1;
            DrawPicker.Value = "1";
            UpdateDeck();
        }

        public void NextCard(object sender, RoutedEventArgs e)
        {
            Card[] cards = MagicDeck.Instance.Next(showNextNmb);
            CardViewer.Instance.ShowCards(cards);
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

        public void HideShowCard(object sender, RoutedEventArgs e)
        {
            ShowCard = null;
            ShowCardVisibility = Visibility.Collapsed;
        }

        public void ViewGraveyard(object sender, RoutedEventArgs e)
        {
            CardViewer.Instance.ShowCards(Graveyard.Instance.CardShuffle);
        }

        public void ViewExile(object sender, RoutedEventArgs e)
        {
            CardViewer.Instance.ShowCards(Exile.Instance.CardShuffle);
        }

        public void CardFollowMouse(object sender, MouseEventArgs e)
        {
            if (HeldCard == null || HeldCard.Card == null) return;

            Point pos = e.GetPosition(null);
            (double X, double Y) winSize = MainWindow.WindowSize;

            pos.Y = (pos.Y / winSize.Y) * CardCanvas.ActualHeight;
            pos.X = (pos.X / winSize.X) * CardCanvas.ActualWidth;

            Canvas.SetTop(HeldCard, pos.Y);
            Canvas.SetLeft(HeldCard, pos.X);
        }

        public void HandleMouseClick(object sender, MouseEventArgs e)
        {
            if (HeldCard == null || HeldCard.Card == null) return;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                switch (HeldCard.Card.PlaySource)
                {
                    case Enums.PlaySource.Command:
                        CommanderSlot.PhaseIn();
                        break;
                    case Enums.PlaySource.Hand:
                        Hand.Instance.AddCard(HeldCard.Card);
                        HandDisplay.Instance.DisplayHand();
                        HandDisplay.BringToFront();
                        break;
                    case Enums.PlaySource.Deck:
                        MagicDeck.Instance.PlaceOnTop(HeldCard.Card);
                        break;
                    case Enums.PlaySource.Exile:
                        Exile.Instance.AddCard(HeldCard.Card);
                        break;
                    default:
                        Graveyard.Instance.AddCard(HeldCard.Card);
                        break;
                }
                HeldCard.Card = null;
                HeldCard.Visibility = Visibility.Collapsed;
            }
        }

        private void Setup()
        {
            GetPlaymatImage();
            for (int _ = 0; _ < 3; _++) AddMainRow();
            if (IsCommander)
            {
                CardHole right2 = GetByUid<CardHole>("Right_2")!;
                right2.AssignedCard = MagicDeck.Instance.Commander;
                right2.SetButtons(new MPDButton()
                {
                    TextContent = "Play",
                    Click = PlayCommander,
                    FontSize = 40,
                    Margin = new Thickness(30, 0, 30, 0)
                });
            }

            for (int i = 0; i < 7; i++) Hand.Instance.AddCard(MagicDeck.Instance.Draw());
            CardViewer.Instance.ShowCards(Hand.Instance.CardShuffle, true);

            HeldCard.Card = MagicDeck.Instance.Cards[0];
            UpdateDeck();
        }

        public void UpdateGraveyard()
        {
            if (Graveyard.Instance.CardCount == 0)
            {
                GraveyardControl.Visibility = Visibility.Collapsed;
                GraveyardControl.Card = null;
                return;
            }

            GraveyardControl.Card = Graveyard.Instance.CardShuffle.Last();
            GraveyardControl.TextContent = Graveyard.Instance.CardCount.ToString();
            GraveyardControl.Visibility = Visibility.Visible;
        }

        public void UpdateExile()
        {
            if (Exile.Instance.CardCount == 0)
            {
                ExileControl.Visibility = Visibility.Collapsed;
                ExileControl.Card = null;
                return;
            }

            ExileControl.Card = Exile.Instance.CardShuffle.Last();
            ExileControl.Visibility = Visibility.Visible;
        }

        public void UpdateDeck()
        {
            DeckControl.Visibility = MagicDeck.Instance.CardCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            int count = MagicDeck.Instance.CardCount;
            DeckControl.TextContent = "Deck " + count.ToString();
            DrawPicker.Max = count;
            ViewPicker.Max = count;
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
