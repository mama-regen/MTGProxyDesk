using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTGProxyDesk.Windows
{
    public partial class HandDisplay : Window
    {
        private static HandDisplay? _instance = null;
        private static readonly object _lock = new object();
        public static HandDisplay Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new HandDisplay();
                    }
                    return _instance;
                }
            }
        }
        public static void CloseInstance() {
            if (_instance == null) return;
            _instance!.CanClose = true;
            _instance.Close();
            _instance = null; 
        }
        public static Action BringToFront = () => { };

        private bool CanClose { get; set; } = false;

        private HandDisplay()
        {
            InitializeComponent();
            BringToFront = () =>
            {
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            };
        }

        public void DisplayHand()
        {
            Card[] cards = Hand.Instance.CardShuffle;
            CardGrid.Children.Clear();
            CardGrid.RowDefinitions.Clear();
            for (int i = 0; i < (int)Math.Ceiling(cards.Length/8.0); i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                CardGrid.RowDefinitions.Add(row);

                Func<string, Action<object, RoutedEventArgs>, MPDButton> MakeButton = (t, a) =>
                {
                    MPDButton btn = new MPDButton();
                    btn.TextContent = t;
                    btn.Click = a;
                    btn.Margin = new Thickness(30, 10, 30, 0);
                    btn.FontSize = 30;
                    return btn;
                };

                for (int j = i * 8; j < Math.Min((i + 1) * 8, cards.Length); j++)
                {
                    CardControl cardCtrl = new CardControl();
                    cardCtrl.Card = cards[j];

                    MPDButton playBtn = MakeButton("Play", PlayCard);
                    MPDButton discBtn = MakeButton("Discard", DiscardCard);
                    MPDButton exleBtn = MakeButton("Exile", ExileCard);
                    MPDButton revlBtn = MakeButton("Reveal", RevealCard);
                    MPDButton viewBtn = MakeButton("Inspect", InspectCard);

                    cardCtrl.Children.Add(playBtn);
                    cardCtrl.Children.Add(discBtn);
                    cardCtrl.Children.Add(exleBtn);
                    cardCtrl.Children.Add(revlBtn);
                    cardCtrl.Children.Add(viewBtn);

                    Grid.SetColumn(cardCtrl, (int)(j%8));
                    Grid.SetRow(cardCtrl, i);
                    CardGrid.Children.Add(cardCtrl);
                }
            }
        }

        public void UntapAllCards(object sender, RoutedEventArgs e)
        {
            IEnumerable<CardHole> cards = PlayMat.Instance.GetChildrenOfType<CardHole>()!;
            foreach (CardHole hole in cards) hole.UntapCard();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CanClose)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                return;
            }
            base.OnClosing(e);
        }

        private void PlayCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;
            PlayMat.Instance.HeldCard.Card = cardCtrl.Card;
            PlayMat.Instance.HeldCard.Visibility = Visibility.Visible;
            CardGrid.Children.Remove(cardCtrl);
            Hand.Instance.RemoveCard(cardCtrl.Card!);
            DisplayHand();
            MainWindow.BringToFront();
        }

        private void DiscardCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;
            Graveyard.Instance.AddCard(cardCtrl.Card!);
            CardGrid.Children.Remove(cardCtrl);
            Hand.Instance.RemoveCard(cardCtrl.Card!);
            DisplayHand();
            PlayMat.Instance.UpdateGraveyard();
        }

        private void ExileCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;
            Exile.Instance.AddCard(cardCtrl.Card!);
            CardGrid.Children.Remove(cardCtrl);
            Hand.Instance.RemoveCard(cardCtrl.Card!);
            DisplayHand();
            PlayMat.Instance.UpdateExile();
        }

        private void RevealCard(object sender, RoutedEventArgs e)
        {
            PlayMat.Instance.ShowCard = new ImageBrush(((DependencyObject)sender).GetAncestorOfType<CardControl>()!.Card!.Image);
            PlayMat.Instance.ShowCardVisibility = Visibility.Visible;
        }

        private void InspectCard(object sender, RoutedEventArgs e)
        {
            CardViewer.Instance.ShowCard(((DependencyObject)sender).GetAncestorOfType<CardControl>()!.Card!);
        }
    }
}
