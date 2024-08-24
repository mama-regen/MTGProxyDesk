using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Enums;
using MTGProxyDesk.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTGProxyDesk.Windows
{
    public partial class CardViewer : Window
    {
        private static CardViewer? _instance = null;
        private static object _lock = new object();
        public static CardViewer Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null )
                    {
                        _instance = new CardViewer();
                    }
                    return _instance;
                }
            }
        }
        public static void CloseInstance()
        {
            if (_instance == null) return;
            _instance._canClose = true;
            _instance.Close();
            _instance = null;
        }
        public static Action BringToFront = () => { };

        private bool _initialHand = false;
        private bool _canClose = false;

        private CardViewer()
        {
            InitializeComponent();
            DataContext = this;
            BringToFront = () =>
            {
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            };
        }

        public void DoMulligan(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Draw 1 less card?",
                "Mulligan",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Cancel) return;

            int count = this.GetChildrenOfType<CardControl>()!.Count();
            if (result == MessageBoxResult.Yes) count--;

            foreach (Card card in Hand.Instance.CardShuffle)
            {
                MagicDeck.Instance.AddCard(card);
            }
            MagicDeck.Instance.Shuffle();
            Hand.Instance.Clear();

            for (int i = 0; i < count; i++)
            {
                Hand.Instance.AddCard(MagicDeck.Instance.Draw());
            }

            ShowCards(Hand.Instance.CardShuffle, true);
        }

        public void ShowCards(IEnumerable<Card> cards, bool initialHand = false) 
        {
            _initialHand = initialHand;

            ViewGrid.Children.Clear();
            ViewGrid.ColumnDefinitions.Clear();
            ViewGrid.RowDefinitions.Clear();

            int columns = Math.Min(cards.Count(), 4);
            int rows = (int)Math.Ceiling(cards.Count() / 4.0);
            int i = 0;

            for (i = 0; i < columns; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                ViewGrid.ColumnDefinitions.Add(col);
            }

            for  (i = 0; i < rows; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                ViewGrid.RowDefinitions.Add(row);
            }

            Func<string, Action<object, RoutedEventArgs>, MPDButton> MakeButton = (t, a) =>
            {
                MPDButton btn = new MPDButton();
                btn.TextContent = t;
                btn.Click = a;
                btn.FontSize = 30;
                btn.Margin = new Thickness(30, 4, 30, 0);
                return btn;
            };

            i = 0;
            foreach (Card card in cards)
            {
                CardControl cardCtrl = new CardControl();
                cardCtrl.Card = card;
                cardCtrl.MaxHeight = 600;

                if (!initialHand)
                {
                    if (
                        MagicDeck.Instance.Commander != null && 
                        MagicDeck.Instance.Commander.Id == cardCtrl.Card.Id
                    )
                    {
                        MPDButton cmdBtn = MakeButton("Command Zone", ToCommandZone);
                        cardCtrl.Children.Add(cmdBtn);
                    }

                    MPDButton topDeck = MakeButton("Top Deck", TopDeck);
                    cardCtrl.Children.Add(topDeck);

                    MPDButton botDeck = MakeButton("Bottom Deck", BotDeck);
                    cardCtrl.Children.Add(botDeck);

                    MPDButton insertRand = MakeButton("Random Deck", InsertRand);
                    cardCtrl.Children.Add(insertRand);

                    MPDButton display = MakeButton("Reveal", DisplayCard);
                    cardCtrl.Children.Add(display);

                    if (cardCtrl.Card.PlaySource != Enums.PlaySource.Hand)
                    {
                        MPDButton toHand = MakeButton("To Hand", ToHand);
                        cardCtrl.Children.Add(toHand);
                    }

                    if (cardCtrl.Card.PlaySource != Enums.PlaySource.Exile)
                    {
                        MPDButton exile = MakeButton("Exile", ExileCard);
                        cardCtrl.Children.Add(exile);
                    }

                    if (cardCtrl.Card.PlaySource != Enums.PlaySource.Graveyard)
                    {
                        MPDButton graveyard = MakeButton("Graveyard", BuryCard);
                        cardCtrl.Children.Add(graveyard);
                    }
                }

                Grid.SetColumn(cardCtrl, (int)(i % 4));
                Grid.SetRow(cardCtrl, (int)(i++ / 4));

                ViewGrid.Children.Add(cardCtrl);
            }

            if (!IsVisible) Show();
            MulliganButton.Visibility = initialHand ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowCard(Card card)
        {
            ShowCards(new Card[] { card });
        }

        public void Accept(object _, RoutedEventArgs __) { Close(); }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_canClose)
            {
                base.OnClosing(e);
                return;
            }
            e.Cancel = true;
            if (_initialHand)
            {
                _initialHand = false;
                if (HandDisplay.Instance.WindowState == WindowState.Minimized)
                {
                    HandDisplay.Instance.WindowState = WindowState.Normal;
                } else if (!HandDisplay.Instance.IsVisible)
                {
                    HandDisplay.Instance.Show();
                    HandDisplay.Instance.DisplayHand();
                }
            }
            Hide();
        }

        private void ToCommandZone(object s, RoutedEventArgs _)
        {
            Replace(s, (__) => PlayMat.Instance.CommanderSlot.PhaseIn(), ()=>{});
        }

        private void TopDeck(object s, RoutedEventArgs _)
        { 
            Replace(s, MagicDeck.Instance.PlaceOnTop, PlayMat.Instance.UpdateDeck);
        }

        private void BotDeck(object s, RoutedEventArgs _)
        {
            Replace(s, MagicDeck.Instance.PlaceOnBottom, PlayMat.Instance.UpdateDeck);
        }

        private void InsertRand(object s, RoutedEventArgs _)
        {
            Replace(s, MagicDeck.Instance.PlaceAtRandom, PlayMat.Instance.UpdateDeck);
        }

        private void ToHand(object s, RoutedEventArgs _)
        {
            Replace(s, Hand.Instance.AddCard, HandDisplay.Instance.DisplayHand);
        }

        private void ExileCard(object s, RoutedEventArgs _)
        {
            Replace(s, Exile.Instance.AddCard, PlayMat.Instance.UpdateExile);
        }

        private void BuryCard(object s, RoutedEventArgs _)
        {
            Replace(s, Graveyard.Instance.AddCard, PlayMat.Instance.UpdateGraveyard);
        }

        private void Replace(object s, Action<Card> placement, Action viewUpdate)
        {
            CardControl ctrl = ((DependencyObject)s).GetAncestorOfType<CardControl>()!;
            RemoveFromSource(ctrl.Card!);
            placement(ctrl.Card!);
            ViewGrid.Children.Remove(ctrl);
            ReindexCards();
            viewUpdate();
        }

        private void DisplayCard(object sender, RoutedEventArgs __) 
        {
            PlayMat.Instance.ShowCard = new ImageBrush(((DependencyObject)sender).GetAncestorOfType<CardControl>()!.Card!.Image);
            PlayMat.Instance.ShowCardVisibility = Visibility.Visible;
        }

        private void ReindexCards()
        {
            IEnumerable<CardControl> ctrls = ViewGrid.GetChildrenOfType<CardControl>()!;
            ViewGrid.Children.Clear();
            ViewGrid.RowDefinitions.Clear();
            ViewGrid.ColumnDefinitions.Clear();

            if (ctrls.Count() == 0)
            {
                Hide();
                return;
            }

            int columns = Math.Min(ctrls.Count(), 4);
            int rows = (int)Math.Ceiling(ctrls.Count() / 4.0);
            int i = 0;

            for (i = 0; i < columns; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                ViewGrid.ColumnDefinitions.Add(col);
            }

            for (i = 0; i < rows; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                ViewGrid.RowDefinitions.Add(row);
            }

            i = 0;
            foreach (CardControl ctrl in ctrls)
            {
                Grid.SetColumn(ctrl, (int)(i % 4));
                Grid.SetRow(ctrl, (int)(i++ / 4));
                ViewGrid.Children.Add(ctrl);
            }
        }

        private void RemoveFromSource(Card card)
        {
            switch(card.PlaySource)
            {
                case PlaySource.Command:
                    PlayMat.Instance.CommanderSlot.PhaseOut();
                    break;
                case PlaySource.Hand:
                    Hand.Instance.RemoveCard(card);
                    HandDisplay.Instance.DisplayHand();
                    break;
                case PlaySource.Deck:
                    MagicDeck.Instance.RemoveCard(card);
                    PlayMat.Instance.UpdateDeck();
                    break;
                case PlaySource.Graveyard:
                    Graveyard.Instance.RemoveCard(card);
                    PlayMat.Instance.UpdateGraveyard();
                    break;
                case PlaySource.Exile:
                    Exile.Instance.RemoveCard(card);
                    PlayMat.Instance.UpdateExile();
                    break;
            }
        }
    }
}
