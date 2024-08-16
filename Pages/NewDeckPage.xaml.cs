using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using MTGProxyDesk.Constants;
using System.ComponentModel;

namespace MTGProxyDesk
{
    public partial class NewDeckPage : Page, INotifyPropertyChanged
    {
        private MagicDeck _Deck;

        private Visibility _countVisibility = Visibility.Collapsed;
        public Visibility CountVisibility
        {
            get => _countVisibility;
            set
            {
                _countVisibility = value;
                OnPropertyChanged("CountVisibility");
            }
        }

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

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
                MPDButton addBtn = GetAddButton(Commander);
                MPDButton rmvBtn = GetRemoveButton(Commander);

                addBtn.CtrlVisibility = Visibility.Collapsed;
                rmvBtn.CtrlVisibility = Visibility.Visible;

                ShowHideAddCard(GetCardTotal() < 100);

                foreach (Grid row in DeckContainer.Children.OfType<Grid>())
                {
                    foreach (CardControl ctrl in row.Children.OfType<CardControl>())
                    {
                        if (ctrl.Card == null || ctrl.Card.AllowAnyAmount) continue;
                        NumberPicker counter = ctrl.Children.OfType<NumberPicker>().First();
                        counter.Value = "1";
                        counter.CtrlVisibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void RemoveCommander(object sender, RoutedEventArgs e)
        {
            Commander.Card = null;

            MPDButton addBtn = GetAddButton(Commander);
            MPDButton rmvBtn = GetRemoveButton(Commander);

            addBtn.CtrlVisibility = Visibility.Visible;
            rmvBtn.CtrlVisibility = Visibility.Collapsed;

            foreach (CardControl cardCtrl in DeckContainer.GetChildrenOfType<CardControl>())
            {
                if (cardCtrl.Card == null) continue;
                NumberPicker nmbPck = cardCtrl.Children.OfType<NumberPicker>().First();
                nmbPck.CtrlVisibility = Visibility.Visible;
            }

            ShowHideAddCard(true);
        }

        public async void AddCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            cardCtrl.HideMenu();
            Card selection = await GetCardSelection();
            if (selection == null) return;
            CardControl? exists = DeckContainer.GetChildrenOfType<CardControl>()!.Where(
                ctrl => ctrl.Card != null && ctrl.Card.Id == selection.Id
            ).FirstOrDefault();
            if (exists != null)
            {
                exists.Children.OfType<NumberPicker>().First().Add();
                return;
            }
            cardCtrl.Card = selection;

            CardControl newCardCtrl = NewCardControl();

            MPDButton addBtn = GetAddButton(cardCtrl);
            MPDButton rmvBtn = GetRemoveButton(cardCtrl);

            addBtn.CtrlVisibility = Visibility.Collapsed;
            rmvBtn.CtrlVisibility = Visibility.Visible;

            NumberPicker counter = cardCtrl.Children.OfType<NumberPicker>().First();
            if (Commander.Card == null || cardCtrl.Card.AllowAnyAmount)
            {
                counter.CtrlVisibility = Visibility.Visible;
                
            } else if (Commander.Card != null)
            {
                counter.CtrlVisibility = Visibility.Collapsed;
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

            ShowHideAddCard(Commander == null || GetCardTotal() < 100);
        }

        public void RemoveCard(object sender, RoutedEventArgs e)
        {
            CardControl cardCtrl = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;

            cardCtrl.Card = null;

            CardControl[] ctrls = DeckContainer.GetChildrenOfType<CardControl>()!.ToArray();

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

            ShowHideAddCard(true);
        }

        public void UpdateCardCount(int value, object sender)
        {
            CardControl parent = ((DependencyObject)sender).GetAncestorOfType<CardControl>()!;
            Card? card = parent.Card;
            if (card == null) return;
            card.Count = value;

            int currentCount = GetCardTotal();
            if (Commander.Card != null)
            {
                int diff = currentCount - 100;
                if (diff > 0) card.Count -= diff;
            }

            if (card.Count != value) ((NumberPicker)sender).Value = card.Count.ToString();

            ShowHideAddCard(Commander!.Card == null || GetCardTotal() < 100);
        }

        public void SaveDeck(object sender, RoutedEventArgs e)
        {
            SaveDeck();
        }

        public void GoBack(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void ShowHideAddCard(bool show)
        {
            Grid lastGrid = DeckContainer.Children.OfType<Grid>().Last();
            if (show)
            {
                if (lastGrid.Children.OfType<CardControl>().Where(c => c.Card == null).Count() > 0) return;
                CardControl addCard = NewCardControl();
                if (lastGrid.Children.OfType<CardControl>().Count() >= 7)
                {
                    lastGrid = NewGridRow();
                }
                int col = lastGrid.Children.OfType<CardControl>().Count();
                Grid.SetColumn(addCard, col);
                lastGrid.Children.Add(addCard);
                return;
            }

            CardControl[] rmvCards = lastGrid.Children.OfType<CardControl>().Where(c => c.Card == null).ToArray();
            if (rmvCards.Count() != 0) foreach (CardControl ctrl in rmvCards) lastGrid.Children.Remove(ctrl);
            if (lastGrid.Children.OfType<CardControl>().Count() == 0) DeckContainer.Children.Remove(lastGrid);
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
            var MakeButton = (string label, int row, bool hidden, Action<object, RoutedEventArgs> func) =>
            {
                MPDButton btn = new MPDButton();
                btn.Margin = new Thickness(30, 10, 30, 0);
                btn.TextContent = label;
                btn.Click = func;
                btn.Tag = label;
                if (hidden) btn.CtrlVisibility = Visibility.Collapsed;
                Grid.SetRow(btn, row);
                return btn;
            };

            MPDButton addBtn = MakeButton("Add", 1, false, AddCard);
            MPDButton rmvBtn = MakeButton("Remove", 2, true, RemoveCard);
            NumberPicker nmbPck = new NumberPicker();
            nmbPck.HorizontalAlignment = HorizontalAlignment.Center;
            nmbPck.Min = 1;
            nmbPck.Max = (cardCtrl.Card == null || !cardCtrl.Card.AllowAnyAmount) ? 4 : int.MaxValue;
            nmbPck.OnChange = UpdateCardCount;
            nmbPck.CtrlVisibility = Visibility.Collapsed;

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
                MPDButton addBtn = GetAddButton(Commander);
                MPDButton rmvBtn = GetRemoveButton(Commander);

                addBtn.CtrlVisibility = Visibility.Collapsed;
                rmvBtn.CtrlVisibility = Visibility.Visible;

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
            foreach (Card card in _Deck.CardsWithCount)
            {
                CardControl newCard = NewCardControl();
                newCard.Card = card;
                Grid.SetColumn(newCard, col);
                gridRows[row].Children.Add(newCard);

                MPDButton addBtn = GetAddButton(newCard);
                MPDButton rmvBtn = GetRemoveButton(newCard);

                addBtn.CtrlVisibility = Visibility.Collapsed;
                rmvBtn.CtrlVisibility = Visibility.Visible;

                NumberPicker counter = newCard.Children.OfType<NumberPicker>().First();
                if (Commander.Card == null || newCard.Card.AllowAnyAmount)
                {
                    counter.CtrlVisibility = Visibility.Visible;
                }
                else if (Commander.Card != null)
                {
                    counter.CtrlVisibility = Visibility.Collapsed;
                }
                counter.Max = card.AllowAnyAmount ? int.MaxValue : 4;
                counter.Value = card.Count.ToString();
                cardTotal += card.Count;

                if (col > (col + 1) % 7) row++;
                col = (col + 1) % 7;
            }

            ShowHideAddCard(Commander == null || cardTotal < 100);
        }

        private int GetCardTotal()
        {
            int total = Commander.Card == null ? 0 : 1;
            foreach (Grid row in DeckContainer.Children.OfType<Grid>())
            {
                foreach (CardControl card in row.Children.OfType<CardControl>())
                {
                    if (card.Card == null) continue;
                    total += Math.Max(card.Count, 1);
                }
            }
            CardCount.Content = total;
            if (Commander.Card == null && total >= 100)
            {
                Commander.CtrlVisibility = Visibility.Collapsed;
                CommanderLabel.Visibility = Visibility.Collapsed;
                CountVisibility = Visibility.Visible;
            }
            else
            {
                Commander.CtrlVisibility = Visibility.Visible;
                CommanderLabel.Visibility = Visibility.Visible;
                CountVisibility = Visibility.Collapsed;
            }
            return total;
        }

        private void SaveDeck()
        {
            _Deck.Clear();
            _Deck.Commander = Commander.Card;
            
            foreach (CardControl cardCtrl in DeckContainer.GetChildrenOfType<CardControl>())
            {
                if (cardCtrl.Card == null) continue;
                _Deck.AddCard(cardCtrl.Card);
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
            NavigationService.Navigate(start);
        }

        private MPDButton GetAddButton(DependencyObject obj)
        {
            return TryGetButton(obj, "Add");
        }

        private MPDButton GetRemoveButton(DependencyObject obj)
        {
            return TryGetButton(obj, "Remove");
        }

        private MPDButton TryGetButton(DependencyObject obj, string tag)
        {
            IEnumerable<MPDButton> buttons = obj.GetChildrenOfType<MPDButton>();
            if (buttons.Count() == 0) buttons = ((UIElementCollection)obj.GetType().GetProperty("Children")!.GetValue(obj)!).OfType<MPDButton>();
            MPDButton? btn = buttons.FirstOrDefault(b => b.Content.ToString() == tag);
            if (btn == null) btn = buttons.FirstOrDefault(b => b.TextContent == tag);
            if (btn == null) btn = tag == "Add" ? buttons.First() : buttons.Last();
            return btn;
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
