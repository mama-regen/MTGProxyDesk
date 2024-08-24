using MTGProxyDesk.Enums;

namespace MTGProxyDesk.Classes
{
    public sealed class Exile : CardPile<Exile> 
    {
        protected override PlaySource? PlaySource { get => Enums.PlaySource.Exile; }
    }

    public sealed class Graveyard : CardPile<Graveyard>
    {
        protected override PlaySource? PlaySource { get => Enums.PlaySource.Graveyard; }

        public void AddToHand(Card card)
        {
            RemoveCard(card);
            Hand.Instance.AddCard(card);
        }

        public void ExileCard(Card card)
        {
            RemoveCard(card);
            Exile.Instance.AddCard(card);
        }

        public void AddToDeck(Card card, DeckPlacement placement)
        {
            RemoveCard(card);
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
