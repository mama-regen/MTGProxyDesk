using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTGProxyDesk.Windows
{
    public partial class HandDisplay : BaseWindow
    {
        private Hand _parent;

        public HandDisplay(Hand parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        public void DisplayHand()
        {
            int[] cards = _parent.CardOrder.ToArray();
            CardGrid.Children.Clear();
            CardGrid.RowDefinitions.Clear();

            for (int i = 0; i < (int)Math.Ceiling(cards.Length/8.0); i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);
                CardGrid.RowDefinitions.Add(row);

                CardControl? cardCtrl = null;

                Action<string, Action<object, RoutedEventArgs>> MakeButton = (t, a) =>
                {
                    MPDButton btn = new MPDButton();
                    btn.TextContent = t;
                    btn.Click = a;
                    btn.Margin = new Thickness(30, 10, 30, 0);
                    btn.FontSize = 30;
                    cardCtrl!.Children.Add(btn);
                };

                for (int j = i * 8; j < Math.Min((i + 1) * 8, cards.Length); j++)
                {
                    int c = j;
                    cardCtrl = new CardControl();
                    cardCtrl.Card = cards[j];

                    MakeButton("Play", PlayCard);
                    MakeButton("Discard", DiscardCard);
                    MakeButton("Exile", ExileCard);
                    MakeButton("Reveal", RevealCard);
                    MakeButton("Inspect", InspectCard);

                    MPDButton left = new MPDButton();
                    left.TextContent = "↤";
                    left.FontSize = 26;
                    left.Margin = new Thickness(0);
                    left.Click = (_, __) => MoveLeft(c);
                    Grid.SetColumn(left, 0);

                    MPDButton right = new MPDButton();
                    right.TextContent = "↦";
                    right.FontSize = 26;
                    right.Margin = new Thickness(0);
                    right.Click = (_, __) => MoveRight(c);
                    Grid.SetColumn(right, 1);

                    Grid reorder = new Grid();
                    ColumnDefinition leftCol = new ColumnDefinition();
                    leftCol.Width = new GridLength(1, GridUnitType.Star);
                    ColumnDefinition rightCol = new ColumnDefinition();
                    rightCol.Width = new GridLength(1, GridUnitType.Star);
                    reorder.ColumnDefinitions.Add(leftCol);
                    reorder.ColumnDefinitions.Add(rightCol);
                    reorder.Margin = new Thickness(30, 10, 30, 0);
                    reorder.Children.Add(left);
                    reorder.Children.Add(right);

                    cardCtrl.Children.Add(reorder);

                    Grid.SetColumn(cardCtrl, (int)(j%8));
                    Grid.SetRow(cardCtrl, i);
                    CardGrid.Children.Add(cardCtrl);
                }
            }
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

        private void MoveLeft(int idx)
        {
            if (idx == 0) return;
            List<int> cards = new List<int>();
            for (int i = 0; i < idx - 1; i++)
            {
                cards.Add(_parent.CardOrder.Dequeue());
            }
            int swap = _parent.CardOrder.Dequeue();
            cards.Add(_parent.CardOrder.Dequeue());
            cards.Add(swap);
            while (_parent.CardOrder.Count > 0)
            {
                cards.Add(_parent.CardOrder.Dequeue());
            }
            
            foreach (int card in cards) _parent.AddCard(card);

            DisplayHand();
        }

        private void MoveRight(int idx)
        {
            if (idx == _parent.CardOrder.Count - 1) return;
            List<int> cards = new List<int>();
            for (int i = 0; i < idx; i++)
            {
                cards.Add(_parent.CardOrder.Dequeue());
            }
            int swap = _parent.CardOrder.Dequeue();
            cards.Add(_parent.CardOrder.Dequeue());
            cards.Add(swap);
            while (_parent.CardOrder.Count > 0)
            {
                cards.Add(_parent.CardOrder.Dequeue());
            }

            foreach (int card in cards) _parent.AddCard(card);

            DisplayHand();
        }

        private void PlayCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            HeldCard.Set(cardCtrl.Card!.Value, Enums.PlaySource.Hand);

            CardGrid.Children.Remove(cardCtrl);
            _parent.RemoveCard(cardCtrl.Card!.Value);

            DisplayHand();
            MainWindow.BringToFront();
        }

        private void DiscardCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            _parent.Parent!.Graveyard.AddCard(cardCtrl.Card!.Value);

            CardGrid.Children.Remove(cardCtrl);
            _parent.RemoveCard(cardCtrl.Card!.Value);

            DisplayHand();
            _parent.Parent.UpdateGraveyard();
        }

        private void ExileCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            _parent.Parent!.Exile.AddCard(cardCtrl.Card!.Value);

            CardGrid.Children.Remove(cardCtrl);
            _parent.RemoveCard(cardCtrl.Card!.Value);

            DisplayHand();
            _parent.Parent.UpdateExile();
        }

        private void RevealCard(object sender, RoutedEventArgs e)
        {
            Card card = CardStock.Get(((DependencyObject)sender).GetAncestorOfType<CardControl>()!.Card!.Value)!;
            _parent.Parent!.ShowCard = new ImageBrush(card.Image);
            _parent.Parent!.ShowCardVisibility = Visibility.Visible;
        }

        private void InspectCard(object sender, RoutedEventArgs e)
        {
            CardViewer cardView = new CardViewer(_parent);
            cardView.ShowCard(((DependencyObject)sender).GetAncestorOfType<CardControl>()!.Card!.Value);
        }
    }
}
