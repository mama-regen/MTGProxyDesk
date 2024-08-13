using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTGProxyDesk
{    
    public partial class CardSearch : Window
    {
        private Card? resultCard;
        private MagicDeck magicDeck;

        private SolidColorBrush _bg1 = Constants.Colors["Background1"].AsBrush();
        public SolidColorBrush BG1 { get => _bg1; }

        private SolidColorBrush _bg2 = Constants.Colors["Background2"].AsBrush();
        public SolidColorBrush BG2 { get => _bg2; }

        private SolidColorBrush _fg1 = Constants.Colors["Foreground1"].AsBrush();
        public SolidColorBrush FG1 { get => _fg1; }

        private SolidColorBrush _fg2 = Constants.Colors["Foreground2"].AsBrush();
        public SolidColorBrush FG2 { get => _fg2; }

        public CardSearch()
        {
            InitializeComponent();
            this.DataContext = this;
            magicDeck = MagicDeck.Instance;
        }

        public async void SearchForCard(object sender, RoutedEventArgs e)
        {
            CardControl cardButton = (CardControl)this.FindName("CardDisplay");
            cardButton.Card = null;
            InitialSearch.Visibility = Visibility.Visible;
            SearchAgain.Visibility = Visibility.Hidden;
            Add.Visibility = Visibility.Hidden;

            ((Label)SearchText.Child).Content = "SEARCHING...";
            SearchText.Visibility = Visibility.Visible;
            string searchName = CardSearchText.Text;
            resultCard = await Card.SearchCard(searchName);
            if (resultCard == null)
            {
                ((Label)SearchText.Child).Content = "CARD NOT FOUND!";
                return;
            }

            cardButton.Card = resultCard;
            SearchText.Visibility = Visibility.Hidden;

            InitialSearch.Visibility = Visibility.Hidden;
            SearchAgain.Visibility = Visibility.Visible;
            Add.Visibility = Visibility.Visible;
        }

        public void SelectCard(object sender, RoutedEventArgs e)
        {
            magicDeck.CardBuffer = resultCard!;
            this.Close();
        }
    }
}
