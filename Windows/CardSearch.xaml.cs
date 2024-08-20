using System.Windows;
using System.Windows.Controls;
using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;

namespace MTGProxyDesk
{    
    public partial class CardSearch : Window
    {
        private Card? resultCard;
        private MagicDeck magicDeck;

        public CardSearch()
        {
            InitializeComponent();
            DataContext = this;
            magicDeck = MagicDeck.Instance;
        }

        public async void SearchForCard(object sender, RoutedEventArgs e)
        {
            CardControl cardButton = (CardControl)FindName("CardDisplay");
            cardButton.Card = null;
            InitialSearch.CtrlVisibility = Visibility.Visible;
            SearchAgain.CtrlVisibility = Visibility.Collapsed;
            Add.CtrlVisibility = Visibility.Collapsed;

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
            SearchText.Visibility = Visibility.Collapsed;

            InitialSearch.CtrlVisibility = Visibility.Collapsed;
            SearchAgain.CtrlVisibility = Visibility.Visible;
            Add.CtrlVisibility = Visibility.Visible;
        }

        public void SelectCard(object sender, RoutedEventArgs e)
        {
            magicDeck.CardBuffer = resultCard!;
            Close();
        }
    }
}
