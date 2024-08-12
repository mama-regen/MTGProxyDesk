using Microsoft.Win32;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            LoadDeck();
        }

        public async void AddCommander(object sender, RoutedEventArgs e)
        {
            Commander.HideMenu();
            Commander.Card = await GetCardSelection();
            if (Commander.Card != null)
            {
                Button[] buttons = Commander.Children.OfType<Button>().ToArray();
                Button addBtn = buttons.Where(b => b.Content.ToString() == "Add").First();
                Button rmvBtn = buttons.Where(b => b.Content.ToString() == "Remove").First();

                addBtn.IsEnabled = false;
                addBtn.Visibility = Visibility.Collapsed;
                rmvBtn.IsEnabled = true;
                rmvBtn.Visibility = Visibility.Visible;

                foreach (CardControl cardCtrl in this.GetChildOfType<ScrollViewer>()!.GetChildrenOfType<CardControl>())
                {
                    if (cardCtrl.Card == null || cardCtrl.Card.AllowAnyAmount) return;
                    NumberPicker countCtrl = cardCtrl.Children.OfType<NumberPicker>().First();
                    countCtrl.Value = "1";
                    countCtrl.IsEnabled = false;
                    countCtrl.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void RemoveCommander(object sender, RoutedEventArgs e)
        {
            Commander.Card = null;

            Button[] buttons = Commander.Children.OfType<Button>().ToArray();
            Button addBtn = buttons.Where(b => b.Content.ToString() == "Add").First();
            Button rmvBtn = buttons.Where(b => b.Content.ToString() == "Remove").First();

            addBtn.IsEnabled = true;
            addBtn.Visibility = Visibility.Visible;
            rmvBtn.IsEnabled = false;
            rmvBtn.Visibility = Visibility.Collapsed;

            foreach (CardControl cardCtrl in DeckContainer.GetChildrenOfType<CardControl>())
            {
                if (cardCtrl.Card == null) continue;
                NumberPicker nmbPck = cardCtrl.Children.OfType<NumberPicker>().First();
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
                exists.Children.OfType<NumberPicker>().First().Add();
                return;
            }
            cardCtrl.Card = selection;

            CardControl newCardCtrl = NewCardControl();

            Button[] buttons = cardCtrl.Children.OfType<Button>().ToArray();
            Button addBtn = buttons.Where(b => b.Content.ToString() == "Add").First();
            Button rmvBtn = buttons.Where(b => b.Content.ToString() == "Remove").First();

            addBtn.IsEnabled = false;
            addBtn.Visibility = Visibility.Collapsed;
            rmvBtn.IsEnabled = true;
            rmvBtn.Visibility = Visibility.Visible;

            NumberPicker counter = cardCtrl.Children.OfType<NumberPicker>().First();
            if (Commander.Card == null || cardCtrl.Card.AllowAnyAmount)
            {
                counter.IsEnabled = true;
                counter.Visibility = Visibility.Visible;
                
            } else if (Commander.Card != null)
            {
                counter.IsEnabled = false;
                counter.Visibility = Visibility.Collapsed;
            }
            counter.Max = selection.AllowAnyAmount ? int.MaxValue : 4;

            Grid container = cardCtrl.GetAncestorOfType<Grid>()!;
            Grid newContainer;
            int cardCount = container.Children.OfType<CardControl>().Count();
            if (cardCount == 7)
            {
                cardCount = 0;
                newContainer = NewGridRow();
            }
            else newContainer = container;

            Grid.SetColumn(newCardCtrl, cardCount);
            newContainer.Children.Add(newCardCtrl);

            if (Commander != null && GetCardTotal() >= 100)
            {
                newCardCtrl.IsEnabled = false;
                newCardCtrl.Visibility = Visibility.Collapsed;
            }
        }

        public void RemoveCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            cardCtrl.Card = null;

            CardControl[] ctrls = DeckContainer.GetChildrenOfType<CardControl>()!;

            int idx = ctrls.IndexOf(cardCtrl);
            Grid parent = cardCtrl.GetAncestorOfType<Grid>()!;
            parent.Children.Remove(cardCtrl);

            Grid[] rows = DeckContainer.Children.OfType<Grid>().ToArray();
            for (int i = idx + 1; i < ctrls.Length; i++)
            {
                CardControl toMove = ctrls[i];
                if (Grid.GetColumn(toMove) == 0)
                {
                    Grid moveParent = toMove.GetAncestorOfType<Grid>()!;
                    moveParent.Children.Remove(toMove);
                    Grid.SetColumn(toMove, 6);
                    rows.ElementAt(rows.IndexOf(moveParent) - 1).Children.Add(toMove);
                } else Grid.SetColumn(toMove, Grid.GetColumn(toMove) - 1);
            }

            int lastIdx = rows.Count() - 1;
            if (rows.Last().Children.OfType<CardControl>().Count() == 0)
            {
                DeckContainer.Children.Remove(rows.Last());
                lastIdx--;
            }

            CardControl lastCard = ctrls.Where(c => c.Card == null).First();
            lastCard.IsEnabled = true;
            lastCard.Visibility = Visibility.Visible;
            Button[] lastButtons = lastCard.Children.OfType<Button>().ToArray();
            Button lastAdd = lastButtons.Where(b => b.Content.ToString() == "Add").First();
            Button lastRmv = lastButtons.Where(b => b.Content.ToString() == "Remove").First();
            lastAdd.IsEnabled = true;
            lastAdd.Visibility = Visibility.Visible;
            lastRmv.IsEnabled = false;
            lastRmv.Visibility = Visibility.Collapsed;
            Grid? lastCardParent = lastCard.GetAncestorOfType<Grid>();
            if (lastCardParent != null) lastCardParent.Children.Remove(lastCard);
            Grid.SetColumn(lastCard, rows.ElementAt(lastIdx).Children.Count - 1);
            rows.ElementAt(lastIdx).Children.Add(lastCard);
        }

        public void UpdateCardCount(int value, object sender)
        {
            CardControl parent = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;
            Card? card = parent.Card;
            if (card == null) return;
            int cardTotal = GetCardTotal() - card.Count - 1;
            card.Count = Commander.Card == null ? value : Math.Min(value, 100 - cardTotal);
            if (card.Count != value) ((NumberPicker)sender).Value = card.Count.ToString();
        }

        public void SaveDeck(object sender, RoutedEventArgs e)
        {
            SaveDeck();
        }

        public void GoBack(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private CardControl NewCardControl()
        {
            CardControl newCardCtrl = new CardControl();
            AddMenuGrid(newCardCtrl);
            newCardCtrl.Margin = new Thickness(5);

            return newCardCtrl;
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
                if (hidden) btn.Visibility = Visibility.Collapsed;
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
            nmbPck.Visibility = Visibility.Collapsed;
            nmbPck.IsEnabled = false;

            cardCtrl.Children.Add(addBtn);
            cardCtrl.Children.Add(nmbPck);
            cardCtrl.Children.Add(rmvBtn);
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

        private void LoadDeck()
        {
            if (_Deck == null || (_Deck.Commander == null && _Deck.CardCount == 0)) return;

            int cardTotal = 0;

            if (_Deck.Commander != null)
            {
                Commander.Card = _Deck.Commander;
                Button[] buttons = Commander.Children.OfType<Button>().ToArray();
                Button addBtn = buttons.Where(b => b.Content.ToString() == "Add").First();
                Button rmvBtn = buttons.Where(b => b.Content.ToString() == "Remove").First();

                addBtn.IsEnabled = false;
                addBtn.Visibility = Visibility.Collapsed;
                rmvBtn.IsEnabled = true;
                rmvBtn.Visibility = Visibility.Visible;

                cardTotal++;
            }

            int cardCount = _Deck.CardCount;
            int rows = (int)Math.Ceiling((cardCount + 1) / 7.0);
            Grid[] gridRows = new Grid[rows];

            foreach (Grid grid in DeckContainer.Children.OfType<Grid>().ToArray())
            {
                DeckContainer.Children.Remove(grid);
            }

            for (int r = 0; r < rows; r++) gridRows[r] = NewGridRow();

            int col = 0;
            int row = 0;
            foreach (Card card in _Deck.CardList)
            {
                CardControl newCard = NewCardControl();
                newCard.Card = card;
                Grid.SetColumn(newCard, col);
                gridRows[row].Children.Add(newCard);

                Button[] buttons = newCard.Children.OfType<Button>().ToArray();
                Button addBtn = buttons.Where(b => b.Content.ToString() == "Add").First();
                Button rmvBtn = buttons.Where(b => b.Content.ToString() == "Remove").First();

                addBtn.IsEnabled = false;
                addBtn.Visibility = Visibility.Collapsed;
                rmvBtn.IsEnabled = true;
                rmvBtn.Visibility = Visibility.Visible;

                NumberPicker counter = newCard.Children.OfType<NumberPicker>().First();
                if (Commander.Card == null || newCard.Card.AllowAnyAmount)
                {
                    counter.IsEnabled = true;
                    counter.Visibility = Visibility.Visible;
                }
                else if (Commander.Card != null)
                {
                    counter.IsEnabled = false;
                    counter.Visibility = Visibility.Collapsed;
                }
                counter.Max = card.AllowAnyAmount ? int.MaxValue : 4;
                counter.Value = card.Count.ToString();
                cardTotal += card.Count;

                if (col > (col + 1) % 7) row++;
                col = (col + 1) % 7;
            }

            CardControl finalCard = NewCardControl();
            Grid.SetColumn(finalCard, col);
            gridRows[row].Children.Add(finalCard);

            if (Commander != null && cardTotal >= 100)
            {
                finalCard.IsEnabled = false;
                finalCard.Visibility = Visibility.Collapsed;
            }
        }

        private int GetCardTotal()
        {
            int total = 0;
            foreach (CardControl cardCtrl in this.GetChildrenOfType<CardControl>()!)
            {
                if (cardCtrl.Card == null) continue;
                NumberPicker? counter = cardCtrl.Children.OfType<NumberPicker>().FirstOrDefault();
                total += counter == null ? 1 : int.Parse(counter.Value);
            }
            return total;
        }

        private void SaveDeck()
        {
            _Deck.ClearDeck();
            _Deck.Commander = Commander.Card;
            
            foreach (CardControl cardCtrl in DeckContainer.GetChildrenOfType<CardControl>())
            {
                if (cardCtrl.Card == null) continue;
                Card card = cardCtrl.Card;
                int count = card.Count;
                card.Count = 1;
                _Deck.Add(card, count);
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Helper.GetDocumentsFolder();
            sfd.Filter = "MTG Proxy Deck (*.mpd)|*.mpd|All Files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.ShowDialog();

            if (sfd.FileName != "") _Deck.Save(sfd.FileName);

            GoBack();
        }

        private void GoBack()
        {
            StartPage start = new StartPage();
            this.NavigationService.Navigate(start);
        }
    }
}
