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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MTGProxyDesk
{
    public partial class PlayMat : Page, INotifyPropertyChanged
    {
        public MagicDeck Deck { get; private set; }
        public Hand Hand { get; private set; }
        public Graveyard Graveyard { get; private set; }
        public Exile Exile { get; private set; }

        private Cache<CardControl> _deckControl;
        public CardControl DeckControl { get => _deckControl.Value; }
        private Cache<CardControl> _commanderControl;
        public CardControl CommanderControl { get => _commanderControl.Value; }
        private Cache<CardControl> _graveyardControl;
        public CardControl GraveyardControl { get => _graveyardControl.Value; }
        private Cache<CardControl> _exileControl;
        public CardControl ExileControl { get => _exileControl.Value; }

        private Cache<NumberPicker> _viewPicker;
        private NumberPicker ViewPicker { get => _viewPicker.Value; }
        private Cache<NumberPicker> _drawPicker;
        private NumberPicker DrawPicker { get => _drawPicker.Value; }

        private int drawNmb { get; set; } = 1;
        private int showNextNmb { get; set; } = 1;

        private bool pointerOnMenu = false;

        List<string> IgnoreList = new List<string> { "GraveyardControl", "ExileControl", "DeckControl" };

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

        public bool IsCommander { get => Deck.Commander != null; }

        public bool ShowExile { get => Exile.CardCount > 0; }

        public bool ShowGraveyard { get => Exile.CardCount > 0; }

        public bool ShowDeck { get => Deck.CardCount > 0; }

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public PlayMat(MagicDeck deck)
        {
            InitializeComponent();

            deck.SetParent(this);

            Deck = deck;
            Hand = new Hand(this);
            Graveyard = new Graveyard(this);
            Exile = new Exile(this);

            DataContext = this;

            _deckControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("DeckControl")!);
            _commanderControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("CommanderControl")!);
            _graveyardControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("GraveyardControl")!);
            _exileControl = Cache<CardControl>.Init(() => GetByUid<CardControl>("ExileControl")!);

            _viewPicker = Cache<NumberPicker>.Init(() => GetByUid<NumberPicker>("ViewPicker")!);
            _drawPicker = Cache<NumberPicker>.Init(() => GetByUid<NumberPicker>("DrawPicker")!);

            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, Setup);
        }

        public void PlayCommander(object sender, RoutedEventArgs e)
        {
            CommanderControl.PhaseOut();
            HeldCard.Set(CommanderControl.Card!.Value, Enums.PlaySource.Command);
        }

        public void DrawCard(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Math.Min(drawNmb, Deck.CardCount); i++)
            {
                Hand.AddCard(Deck.Draw());
            }
            HandDisplay.BringToFront();
            drawNmb = 1;
            DrawPicker.Value = "1";
            UpdateDeck();
        }

        public void NextCard(object sender, RoutedEventArgs e)
        {
            int[] cards = Deck.Next(showNextNmb);
            CardViewer viewer = new CardViewer(Deck);
            viewer.ShowCards(cards);
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
            Deck.Shuffle();
        }

        public void HideShowCard(object sender, RoutedEventArgs e)
        {
            ShowCard = null;
            ShowCardVisibility = Visibility.Collapsed;
        }

        public void ViewGraveyard(object sender, RoutedEventArgs e)
        {
            CardViewer viewer = new CardViewer(Graveyard);
            viewer.ShowCards(Graveyard.CardOrder);
        }

        public void ViewExile(object sender, RoutedEventArgs e)
        {
            CardViewer viewer = new CardViewer(Exile);
            viewer.ShowCards(Exile.CardOrder);
        }

        public void CardFollowMouse(object sender, MouseEventArgs e)
        {
            Card? heldCard = HeldCard.Get();
            if (heldCard == null)
            {
                CardCanvas.Children.Clear();
                return;
            }

            CardControl heldDisplay;
            if (CardCanvas.Children.Count == 0)
            {
                heldDisplay = new CardControl();
                heldDisplay.Width = 100;
                heldDisplay.Height = 140;
                heldDisplay.Card = CardStock.IndexOf(heldCard);
                CardCanvas.Children.Add(heldDisplay);
            }
            else heldDisplay = CardCanvas.GetChildOfType<CardControl>()!;

            Point pos = e.GetPosition(null);
            (double X, double Y) winSize = MainWindow.WindowSize;

            pos.Y = (pos.Y / winSize.Y) * CardCanvas.ActualHeight;
            pos.X = (pos.X / winSize.X) * CardCanvas.ActualWidth;

            Canvas.SetTop(heldDisplay, pos.Y);
            Canvas.SetLeft(heldDisplay, pos.X);
        }

        public void HandleMouseClick(object sender, MouseEventArgs e)
        {
            Card? heldCard = HeldCard.Get();
            if (heldCard == null) return;

            int cardIdx = CardStock.IndexOf(heldCard);

            if (e.RightButton == MouseButtonState.Pressed)
            {
                switch (HeldCard.Source)
                {
                    case Enums.PlaySource.Command:
                        CommanderControl.PhaseIn();
                        break;
                    case Enums.PlaySource.Hand:
                        Hand.AddCard(cardIdx);
                        HandDisplay.BringToFront();
                        break;
                    case Enums.PlaySource.Deck:
                        Deck.PlaceOnTop(cardIdx);
                        break;
                    case Enums.PlaySource.Exile:
                        Exile.AddCard(cardIdx);
                        break;
                    default:
                        Graveyard.AddCard(cardIdx);
                        break;
                }
                HeldCard.Set(null);
                CardCanvas.Children.Clear();
            }
        }

        public void Quit(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is BaseWindow && !(window is MainWindow))
                {
                    ((BaseWindow)window).CanClose = true;
                    window.Close();
                }
            }

            StartPage start = new StartPage();
            NavigationService.Navigate(start);
        }

        public async void SearchTokens(object sender, RoutedEventArgs e)
        {
            await DoTokenSearch();
        }

        public void TapAll(object sender, RoutedEventArgs e)
        {
            IEnumerable<CardControl> ctrls = this.GetChildrenOfType<CardControl>()!;
            foreach (CardControl ctrl in ctrls)
            {
                if (IgnoreList.Contains(ctrl.Uid) || ctrl.Card == null) continue;
                ctrl.Tap();
            }
        }

        public void UnTapAll(object sender, RoutedEventArgs e)
        {
            IEnumerable<CardControl> ctrls = this.GetChildrenOfType<CardControl>()!;
            foreach (CardControl ctrl in ctrls)
            {
                if (IgnoreList.Contains(ctrl.Uid)) continue;
                ctrl.UnTap();
            }
        }

        public void ExileGraveyard(object sender, RoutedEventArgs e)
        {
            int[] cards = Graveyard.CardOrder.ToArray();
            foreach (int card in cards)
            {
                Graveyard.RemoveCard(card);
                Exile.AddCard(card);
            }
            UpdateGraveyard();
            UpdateExile();
        }

        public void UnburyGraveyard(object sender, RoutedEventArgs e)
        {
            int[] cards = Graveyard.CardOrder.ToArray();
            foreach (int card in cards)
            {
                Graveyard.RemoveCard(card);
                Deck.AddCard(card);
            }
            Deck.Shuffle();
            UpdateGraveyard();
            UpdateDeck();
        }

        private async Task<bool> DoTokenSearch()
        {
            CardSearch search = new CardSearch();
            search.tokens = true;
            HeldCard.Set(null);
            search.Show();

            return await Task.Run(async () =>
            {
                while (HeldCard.Get() == null) await Task.Delay(100);
                Card card = HeldCard.Get()!;
                CardStock.Add(card);
                return true;
            });
        }

        private void Setup()
        {
            GetPlaymatImage();
            for (int _ = 0; _ < 3; _++) AddMainRow(false);
            if (IsCommander) CommanderControl.Card = Deck.Commander;

            for (int i = 0; i < 7; i++) Hand.AddCard(Deck.Draw());
            CardViewer initialView = new CardViewer(Hand);
            initialView.ShowCards(Hand.CardOrder, true);

            UpdateDeck();

            AddControlButtons(this.GetChildrenOfType<CardControl>());
        }

        public void UpdateGraveyard()
        {
            if (Graveyard.CardCount == 0)
            {
                GraveyardControl.Visibility = Visibility.Collapsed;
                GraveyardControl.Card = null;
                return;
            }

            GraveyardControl.Card = Graveyard.CardOrder.Last();
            GraveyardControl.TextContent = Graveyard.CardCount.ToString();
            GraveyardControl.Visibility = Visibility.Visible;
        }

        public void UpdateExile()
        {
            if (Exile.CardCount == 0)
            {
                ExileControl.Visibility = Visibility.Collapsed;
                ExileControl.Card = null;
                return;
            }

            ExileControl.Card = Exile.CardOrder.Last();
            ExileControl.Visibility = Visibility.Visible;
        }

        public void UpdateDeck()
        {
            int count = Deck.CardCount;
            DeckControl.Visibility = count == 0 ? Visibility.Collapsed : Visibility.Visible;
            DeckControl.TextContent = "Deck " + count.ToString();
            DrawPicker.Max = count;
            ViewPicker.Max = count;
        }

        public void ShowMenu(object sender, EventArgs e)
        {
            pointerOnMenu = true;

            Border menu = (sender is Border) ? (Border)sender : ((DependencyObject)sender).GetAncestorOfType<Border>()!;
            TranslateTransform? tt = menu.RenderTransform as TranslateTransform;

            if (tt == null)
            {
                tt = new TranslateTransform(190, 0);
                menu.RenderTransform = tt;
            } 
            else if (tt.X == 0) return;

            TimeSpan animTime = TimeSpan.FromMilliseconds(150);

            DoubleAnimation animT = new DoubleAnimation
            {
                To = 0,
                Duration = animTime,
                DecelerationRatio = 0.2,
                AccelerationRatio = 0.4,
                FillBehavior = FillBehavior.Stop
            };
            animT.Completed += (_, __) => tt.X = 0;

            Label tri = menu.GetChildOfType<Label>()!;
            RotateTransform? rt = tri.RenderTransform as RotateTransform;

            if (rt == null)
            {
                rt = new RotateTransform(0);
                tri.RenderTransform = rt;
            }

            DoubleAnimation animA = new DoubleAnimation
            {
                To = 180,
                Duration = animTime,
                FillBehavior = FillBehavior.Stop
            };
            animA.Completed += (_, __) => rt.Angle = 180;

            tt.BeginAnimation(TranslateTransform.XProperty, animT, HandoffBehavior.Compose);
            rt.BeginAnimation(RotateTransform.AngleProperty, animA, HandoffBehavior.Compose);
        }

        public async void HideMenu(object sender, EventArgs e)
        {
            pointerOnMenu = false;
            await Task.Delay(300);

            if (pointerOnMenu) return;

            Border menu = (sender is Border) ? (Border)sender : ((DependencyObject)sender).GetAncestorOfType<Border>()!;
            TranslateTransform? tt = menu.RenderTransform as TranslateTransform;

            if (tt == null)
            {
                tt = new TranslateTransform(0, 0);
                menu.RenderTransform = tt;
            }
            else if (tt.X == 190) return;

            TimeSpan animTime = TimeSpan.FromMilliseconds(300);

            DoubleAnimation anim = new DoubleAnimation
            {
                To = 190,
                Duration = animTime,
                DecelerationRatio = 0.4,
                AccelerationRatio = 0.2,
                FillBehavior = FillBehavior.Stop,
            };
            anim.Completed += (_, __) => tt.X = 190;

            Label tri = menu.GetChildOfType<Label>()!;
            RotateTransform? rt = tri.RenderTransform as RotateTransform;

            if (rt == null)
            {
                rt = new RotateTransform(180);
                tri.RenderTransform = rt;
            }

            DoubleAnimation animA = new DoubleAnimation
            {
                To = 0,
                Duration = animTime,
                FillBehavior = FillBehavior.Stop
            };
            animA.Completed += (_, __) => rt.Angle = 0;

            tt.BeginAnimation(TranslateTransform.XProperty, anim, HandoffBehavior.Compose);
            rt.BeginAnimation(RotateTransform.AngleProperty, animA, HandoffBehavior.Compose);
        }

        private void AddControlButtons(IEnumerable<CardControl> ctrls)
        {
            Func<string, Action, MPDButton> MakeButton = (t, a) =>
            {
                MPDButton btn = new MPDButton();
                btn.TextContent = t;
                btn.Click = (_, __) => a();
                btn.FontSize = 30;
                btn.Margin = new Thickness(30, 10, 30, 0);

                return btn;
            };

            Func<CardControl, CardPile, Action, Action> Updt = (c, p, a) =>
            {
                return () =>
                {
                    int card = c.Card!.Value;
                    c.Card = null;
                    Card? exists = CardStock.Get(card);
                    if (exists != null && !exists.IsToken) p.AddCard(card);
                    a();
                    c.HideMenu();
                };
            };
            if (IsCommander) IgnoreList.Add("CommanderControl");
            else CommanderControl.Children.Clear();

            foreach (CardControl ctrl in ctrls)
            {
                if (IgnoreList.Contains(ctrl.Uid)) continue;

                MPDButton cmd = MakeButton("Command Zone", () =>
                {
                    CommanderControl.PhaseIn();
                    ctrl.Card = null;
                });
                cmd.Visibility = Visibility.Collapsed;
                cmd.Uid = "CmdBtn";
                MPDButton bury = MakeButton("Destroy", Updt(ctrl, Graveyard, UpdateGraveyard));
                MPDButton exile = MakeButton("Exile", Updt(ctrl, Exile, UpdateExile));
                MPDButton hand = MakeButton("To Hand", Updt(ctrl, Hand, () => ((HandDisplay)Hand.Display!).DisplayHand()));
                MPDButton? tap = null;
                MPDButton? untap = null;
                tap = MakeButton("Tap", () => {
                    ctrl.Tap();
                    tap!.Visibility = Visibility.Collapsed;
                    untap!.Visibility = Visibility.Visible;
                });
                untap = MakeButton("UnTap", () =>
                {
                    ctrl.UnTap();
                    tap!.Visibility = Visibility.Visible;
                    untap!.Visibility = Visibility.Collapsed;
                });
                untap.Visibility = Visibility.Collapsed;

                ctrl.Children.Add(cmd);
                ctrl.Children.Add(bury);
                ctrl.Children.Add(exile);
                ctrl.Children.Add(hand);
                ctrl.Children.Add(tap!);
                ctrl.Children.Add(untap!);
            }
        }

        public void AddMainRow(object sender, RoutedEventArgs e)
        {
            AddMainRow(true);
        }

        private void AddMainRow(bool addButtons = false)
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

                CardControl ctrl = new CardControl();
                ctrl.Margin = new Thickness(6);
                ctrl.VerticalAlignment = VerticalAlignment.Center;
                ctrl.HorizontalAlignment = HorizontalAlignment.Center;
                ctrl.DefaultImageOnEmpty = false;
                Grid.SetColumn(ctrl, i);
                newRow.Children.Add(ctrl);
            }
            MainArea.Children.Add(newRow);
            if (MainArea.Children.Count > 3)
            {
                MainArea.GetAncestorOfType<ScrollViewer>()!.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }

            if (addButtons) AddControlButtons(newRow.GetChildrenOfType<CardControl>());
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
