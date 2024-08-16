using MTGProxyDesk.Controls;
using MTGProxyDesk.Enums;

namespace MTGProxyDesk.Classes
{
    public sealed class Hand : CardPile<Hand>
    {
        public void DiscardCard(Card card)
        {
            card.Count = 1;
            _cards.Remove(card);
            Graveyard.Instance.AddCard(card);
        }
        
        public void ExileCard(Card card) 
        {
            card.Count = 1;
            _cards.Remove(card);
            Exile.Instance.AddCard(card);
        }

        public void AddToDeck(Card card, DeckPlacement placement)
        {
            card.Count = 1;
            _cards.Remove(card);
            switch (placement)
            {
                case DeckPlacement.Top:
                    MagicDeck.Instance.PlaceOnTop(card);
                    break;
                case DeckPlacement.Bottom:
                    MagicDeck.Instance.PlaceOnBottom(card);
                    break;
                default:
                    //MagicDeck.Instance.PlaceRandom(card);
                    break;
            }
        }
    }
}
