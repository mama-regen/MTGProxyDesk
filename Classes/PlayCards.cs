using MTGProxyDesk.Extensions;
using MTGProxyDesk.Windows;

namespace MTGProxyDesk.Classes
{
    public class Hand : CardPile
    {
        public Hand(PlayMat parent) : base(parent, typeof(HandDisplay)) { }

        protected override void InsertAt(int index, int where)
        {
            base.InsertAt(index, where);
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void RemoveCard(int index, bool all = false)
        {
            base.RemoveCard(index, all);
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void AddCard(int index, int amount = 1)
        {
            base.AddCard(index, amount);
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void PlaceOnTop(int index)
        {
            base.PlaceOnTop(index);
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void PlaceOnBottom(int index)
        {
            base.PlaceOnBottom(index);
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void PlaceAtRandom(int index)
        {
            base.PlaceAtRandom(index);
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void Clear() 
        {
            base.Clear();
            ((HandDisplay)Display!).DisplayHand();
        }

        public override void Shuffle() 
        {
            base.Shuffle();
            ((HandDisplay)Display!).DisplayHand();
        }
    }

    public class Exile : CardPile
    {
        public Exile(PlayMat parent) : base(parent, typeof(CardViewer)) { }
    }

    public class Graveyard : CardPile
    {
        public Graveyard(PlayMat parent) : base(parent, typeof(CardViewer)) { }
    }
}
