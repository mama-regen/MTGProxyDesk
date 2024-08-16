using MTGProxyDesk.Controls;
using MTGProxyDesk.Enums;

namespace MTGProxyDesk.Classes
{
    public sealed class Exile : CardPile<Exile> { }

    public sealed class Graveyard : CardPile<Graveyard>
    {
        public void AddToHand(Card card)
        {
            RemoveCard(_cards.IndexOf(card));
            Hand.Instance.AddCard(card);
        }

        public void ExileCard(Card card)
        {
            RemoveCard(_cards.IndexOf(card));
            Exile.Instance.AddCard(card);
        }

        public void AddToDeck(Card card, DeckPlacement placement)
        {
            RemoveCard(_cards.IndexOf(card));
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
