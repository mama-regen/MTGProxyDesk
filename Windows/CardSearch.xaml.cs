using System.Windows;
using System.Windows.Controls;
using MTGProxyDesk.Classes;
using MTGProxyDesk.Controls;

namespace MTGProxyDesk.Windows
{    
    public partial class CardSearch : BaseWindow
    {
        private int? resultCard = null;
        public bool tokens { get; set; } = false;

        public CardSearch()
        {
            InitializeComponent();
            DataContext = this;
            CanClose = true;
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
            resultCard = await (tokens ? Card.SearchToken(searchName) : Card.SearchCard(searchName));
            if (resultCard == null)
            {
                ((Label)SearchText.Child).Content = "CARD NOT FOUND!";
                return;
            }

            cardButton.Card = resultCard!;
            SearchText.Visibility = Visibility.Collapsed;

            InitialSearch.CtrlVisibility = Visibility.Collapsed;
            SearchAgain.CtrlVisibility = Visibility.Visible;
            Add.CtrlVisibility = Visibility.Visible;
        }

        public void SelectCard(object sender, RoutedEventArgs e)
        {
            HeldCard.Set(resultCard!.Value, Enums.PlaySource.Deck);
            Close();
        }
    }
}
