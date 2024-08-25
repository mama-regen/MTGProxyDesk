using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTGProxyDesk.Windows
{
    public partial class CardViewer : BaseWindow
    {
        private bool _initialHand = false;
        private CardPile _source;

        public CardViewer(CardPile source)
        {
            InitializeComponent();
            DataContext = this;
            _source = source;
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

            Hand hand = (Hand)_source;

            int count = this.GetChildrenOfType<CardControl>()!.Count();
            if (result == MessageBoxResult.Yes) count--;

            foreach (Card card in hand.Cards) hand.Parent!.Deck.AddCard(card);
            hand.Parent!.Deck.Shuffle();
            hand.Clear();

            for (int i = 0; i < count; i++) hand.AddCard(hand.Parent.Deck.Draw());

            ShowCards(hand.CardOrder, true);
        }

        public void ShowCards(IEnumerable<int> cards, bool initialHand = false) 
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

            MagicDeck deck = _source is MagicDeck ? (MagicDeck)_source : _source.Parent!.Deck;

            i = 0;
            foreach (int cardIdx in cards)
            {
                CardControl cardCtrl = new CardControl();
                Card card = CardStock.Get(cardIdx)!;
                cardCtrl.Card = cardIdx;
                cardCtrl.MaxHeight = 600;

                if (!initialHand)
                {
                    if (
                        deck.Commander != null && 
                        CardStock.Get(deck.Commander)?.Id == (CardStock.Get(cardCtrl.Card)?.Id ?? "!")
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

                    if (_source.GetType() != typeof(Hand))
                    {
                        MPDButton toHand = MakeButton("To Hand", ToHand);
                        cardCtrl.Children.Add(toHand);
                    }

                    if (_source.GetType() != typeof(Exile))
                    {
                        MPDButton exile = MakeButton("Exile", ExileCard);
                        cardCtrl.Children.Add(exile);
                    }

                    if (_source.GetType() != typeof(Graveyard))
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

        public void ShowCard(int card)
        {
            ShowCards(new int[] { card });
        }

        public void Accept(object _, RoutedEventArgs __) { Close(); }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_initialHand)
            {
                _initialHand = false;
                if (((Hand)_source).Display!.WindowState == WindowState.Minimized)
                {
                    ((Hand)_source).Display!.WindowState = WindowState.Normal;
                } else if (!((Hand)_source).Display!.IsVisible)
                {
                    try
                    {
                        ((Hand)_source).Display!.Show();
                        (((Hand)_source).Display as HandDisplay)!.DisplayHand();
                    }
                    catch { }
                }
            }
            CanClose = true;
            base.OnClosing(e);
        }

        private void ToCommandZone(object s, RoutedEventArgs _)
        {
            Replace(s, (__, ___) => { }, _source.Parent!.CommanderControl.PhaseIn);
        }

        private void TopDeck(object s, RoutedEventArgs _)
        {
            MagicDeck deck = (_source is MagicDeck) ? (MagicDeck)_source : _source.Parent!.Deck;
            Replace(s, (i, __) => deck.PlaceOnTop(i), _source.Parent!.UpdateDeck);
        }

        private void BotDeck(object s, RoutedEventArgs _)
        {
            MagicDeck deck = (_source is MagicDeck) ? (MagicDeck)_source : _source.Parent!.Deck;
            Replace(s, (i, __) => deck.PlaceOnBottom(i), _source.Parent!.UpdateDeck);
        }

        private void InsertRand(object s, RoutedEventArgs _)
        {
            MagicDeck deck = (_source is MagicDeck) ? (MagicDeck)_source : _source.Parent!.Deck;
            Replace(s, (i, __) => deck.PlaceAtRandom(i), _source.Parent!.UpdateDeck);
        }

        private void ToHand(object s, RoutedEventArgs _)
        {
            Hand hand = (_source is Hand) ? (Hand)_source : _source.Parent!.Hand;
            Replace(s, hand.AddCard, ((HandDisplay)hand.Display!).DisplayHand);
        }

        private void ExileCard(object s, RoutedEventArgs _)
        {
            Exile exile = (_source is Exile) ? (Exile)_source : _source.Parent!.Exile;
            Replace(s, exile.AddCard, _source.Parent!.UpdateExile);
        }

        private void BuryCard(object s, RoutedEventArgs _)
        {
            Graveyard graveyard = (_source is Graveyard) ? (Graveyard)_source : _source.Parent!.Graveyard;
            Replace(s, graveyard.AddCard, _source.Parent!.UpdateGraveyard);
        }

        private void Replace(object s, Action<int, int> placement, Action viewUpdate)
        {
            CardControl ctrl = ((DependencyObject)s).GetAncestorOfType<CardControl>()!;
            _source.RemoveCard(ctrl.Card!.Value);
            placement(ctrl.Card!.Value, 1);
            ViewGrid.Children.Remove(ctrl);
            ReindexCards();
            viewUpdate();

            switch(_source.GetType().ToString().Split(".").Last())
            {
                case "Graveyard":
                    _source.Parent!.UpdateGraveyard();
                    break;
                case "Exile":
                    _source.Parent!.UpdateExile();
                    break;
                case "Deck":
                    _source.Parent!.UpdateDeck();
                    break;
                case "Hand":
                    ((HandDisplay)_source.Parent!.Hand.Display!).DisplayHand();
                    break;
            }
        }

        private void DisplayCard(object sender, RoutedEventArgs __) 
        {
            Card card = CardStock.Get(((DependencyObject)sender).GetAncestorOfType<CardControl>()!.Card!.Value)!;
            _source.Parent!.ShowCard = new ImageBrush(card.Image);
            _source.Parent!.ShowCardVisibility = Visibility.Visible;
        }

        private void ReindexCards()
        {
            IEnumerable<CardControl> ctrls = ViewGrid.GetChildrenOfType<CardControl>()!;
            ViewGrid.Children.Clear();
            ViewGrid.RowDefinitions.Clear();
            ViewGrid.ColumnDefinitions.Clear();

            if (ctrls.Count() == 0)
            {
                Close();
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
    }
}
