using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

        private bool CanClose { get; set; } = false;

        private HandDisplay()
        {
            InitializeComponent();
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

                for (int j = i * 8; j < Math.Min((i + 1) * 8, cards.Length); j++)
                {
                    CardControl cardCtrl = new CardControl();
                    cardCtrl.Card = cards[j];
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
    }
}
