using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Linq.Expressions;

namespace MTGProxyDesk
{
    public partial class NewDeckPage : Page
    {
        private MagicDeck _Deck;

        public NewDeckPage()
        {
            InitializeComponent();
            DataContext = this;
            _Deck = MagicDeck.Instance;
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }

        public async void AddCommander(object sender, RoutedEventArgs e)
        {
            Commander.HideMenu();
            Commander.Card = await GetCardSelection();
            if (Commander.Card != null)
            {
                Button addBtn = GetMenuButton(Commander, "Add");
                Button rmvBtn = GetMenuButton(Commander, "Remove");

                addBtn.IsEnabled = false;
                addBtn.Visibility = Visibility.Hidden;
                rmvBtn.IsEnabled = true;
                rmvBtn.Visibility = Visibility.Visible;

                foreach (CardControl cardCtrl in this.GetChildOfType<ScrollViewer>()!.GetChildrenOfType<CardControl>())
                {
                    if (cardCtrl.Card == null || cardCtrl.Card.AllowAnyAmount) return;
                    NumberPicker countCtrl = cardCtrl.GetChildOfType<NumberPicker>()!;
                    countCtrl.Value = "1";
                    countCtrl.IsEnabled = false;
                    countCtrl.Visibility = Visibility.Hidden;
                }
            }
        }

        public void RemoveCommander(object sender, RoutedEventArgs e)
        {
            Commander.Card = null;

            Button addBtn = GetMenuButton(Commander, "Add");
            Button rmvBtn = GetMenuButton(Commander, "Remove");

            addBtn.IsEnabled = true;
            addBtn.Visibility = Visibility.Visible;
            rmvBtn.IsEnabled = false;
            rmvBtn.Visibility = Visibility.Hidden;

            GetMenuButton(Commander, "Add").Visibility = Visibility.Visible;
            GetMenuButton(Commander, "Remove").Visibility = Visibility.Hidden;
            foreach (CardControl cardCtrl in this.GetChildOfType<ScrollViewer>()!.GetChildrenOfType<CardControl>())
            {
                NumberPicker nmbPck = cardCtrl.GetChildOfType<NumberPicker>()!;
                nmbPck.IsEnabled = true;
                nmbPck.Visibility = Visibility.Visible;
            }
        }

        public async void AddCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            cardCtrl.HideMenu();
            Card selection = await GetCardSelection();
            if (selection == null) return;
            CardControl? exists = this.GetChildrenOfType<CardControl>()!.Where(
                ctrl => ctrl.Card != null && ctrl.Card.Id == selection.Id
            ).FirstOrDefault();
            if (exists != null)
            {
                exists.GetChildOfType<NumberPicker>()!.Add();
                return;
            }
            cardCtrl.Card = selection;

            CardControl newCardCtrl = new CardControl();
            AddMenuGrid(newCardCtrl);
            newCardCtrl.Margin = new Thickness(5);

            Button addBtn = GetMenuButton(cardCtrl, "Add");
            Button rmvBtn = GetMenuButton(cardCtrl, "Remove");

            addBtn.IsEnabled = false;
            addBtn.Visibility = Visibility.Hidden;
            rmvBtn.IsEnabled = true;
            rmvBtn.Visibility = Visibility.Visible;

            NumberPicker counter = cardCtrl.GetChildOfType<NumberPicker>()!;
            if (Commander.Card == null || cardCtrl.Card.AllowAnyAmount)
            {
                counter.IsEnabled = true;
                counter.Visibility = Visibility.Visible;
                
            } else if (Commander.Card != null)
            {
                counter.IsEnabled = false;
                counter.Visibility = Visibility.Hidden;
            }
            counter.Max = selection.AllowAnyAmount ? int.MaxValue : 4;

            Grid container = cardCtrl.GetAncestorOfType<Grid>()!;
            Grid newContainer;
            int cardCount = container.Children.Count;
            if (cardCount == 7)
            {
                cardCount = 0;
                newContainer = NewGridRow();
            }
            else newContainer = container;

            Grid.SetColumn(newCardCtrl, cardCount);
            newContainer.Children.Add(newCardCtrl);
        }

        public void RemoveCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            cardCtrl.Card = null;

            CardControl[] ctrls = DeckContainer.GetChildrenOfType<CardControl>()!;

            int idx = ctrls.IndexOf(cardCtrl);
            Grid parent = cardCtrl.GetAncestorOfType<Grid>()!;
            parent.Children.Remove(cardCtrl);

            Grid[] rows = DeckContainer.GetChildrenOfType<Grid>()!;
            for (int i = idx + 1; i < ctrls.Length; i++)
            {
                CardControl toMove = ctrls[i];
                Grid moveParent = toMove.GetAncestorOfType<Grid>()!;
                if (Grid.GetColumn(toMove) == 0)
                {
                    moveParent.Children.Remove(toMove);
                    Grid.SetColumn(toMove, 6);
                    rows.ElementAt(rows.IndexOf(moveParent) - 1).Children.Add(toMove);
                } else Grid.SetColumn(toMove, Grid.GetColumn(toMove) - 1);
            }

            if (rows.Last().GetChildrenOfType<CardControl>().Count() == 0)
            {
                DeckContainer.Children.Remove(rows.Last());
            }
        }

        public void UpdateCardCount(int value, object sender)
        {
            CardControl parent = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;
            Card? card = parent.Card;
            if (card == null) return;
            card.Count = value;
            if (card.Count != value) ((NumberPicker)sender).Value = card.Count.ToString();
        }

        private Grid NewGridRow()
        {
            Grid grid = new Grid();
            for (int _ = 0; _ < 7; _++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = new GridLength(5, GridUnitType.Star);
                grid.ColumnDefinitions.Add(colDef);
            }
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(7, GridUnitType.Star);
            grid.RowDefinitions.Add(rowDef);

            DeckContainer.Children.Add(grid);
            return grid;
        }

        private void AddMenuGrid(CardControl cardCtrl)
        {
            var MakeButton = (string label, int row, bool hidden, RoutedEventHandler func) =>
            {
                Button btn = new Button();
                btn.BorderThickness = new Thickness(0);
                btn.Margin = new Thickness(30, 10, 30, 0);
                btn.Height = 30;
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4f4f4f")!;
                btn.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#dfdfdf")!;
                btn.Content = label;
                btn.Click += func;
                if (hidden) btn.Visibility = Visibility.Hidden;
                if (hidden) btn.IsEnabled = false;
                Grid.SetRow(btn, row);
                return btn;
            };

            Button addBtn = MakeButton("Add", 1, false, AddCard);
            Button rmvBtn = MakeButton("Remove", 2, true, RemoveCard);
            NumberPicker nmbPck = new NumberPicker();
            nmbPck.HorizontalAlignment = HorizontalAlignment.Center;
            nmbPck.Min = 1;
            nmbPck.Max = (cardCtrl.Card == null || !cardCtrl.Card.AllowAnyAmount) ? 4 : int.MaxValue;
            nmbPck.OnChange = UpdateCardCount;
            nmbPck.Visibility = Visibility.Hidden;
            nmbPck.IsEnabled = false;

            cardCtrl.Children.Add(addBtn);
            cardCtrl.Children.Add(nmbPck);
            cardCtrl.Children.Add(rmvBtn);
        }

        private Button GetMenuButton(DependencyObject obj, string label)
        {
            Button[] buttons = obj.GetChildrenOfType<Button>();
            return buttons.Where((Button x) => x.Content.ToString() == label).First();
        }

        private async Task<Card> GetCardSelection()
        {
            CardSearch search = new CardSearch();
            _Deck.CardBuffer = null;
            search.Show();

            return await Task.Run(async () =>
            {
                while (_Deck.CardBuffer == null) await Task.Delay(100);
                Card card = _Deck.CardBuffer;
                _Deck.CardBuffer = null;
                return card;
            });
        }
    }
}
