using System.Windows;
using System.Windows.Controls;

namespace MTGProxyDesk
{    
    public partial class CardSearch : Window
    {
        private Card? resultCard;
        private MagicDeck magicDeck;

        public CardSearch()
        {
            InitializeComponent();
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
