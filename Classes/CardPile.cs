using MTGProxyDesk.Enums;
using MTGProxyDesk.Extensions;
using MTGProxyDesk.Windows;

namespace MTGProxyDesk.Classes
{
    public abstract class CardPile
    {
        public virtual PlayMat? Parent { get; protected set; }

        public virtual BaseWindow? Display { get; private set; } = null;

        public Queue<int> CardOrder { get; protected set; } = new Queue<int>();
        public Card[] Cards
        {
            get => CardOrder.Select((i) =>
            {
                Card card = CardStock.Get(i)!;
                return card;
            }).ToArray();
        }
        
        public virtual int CardCount { get => CardOrder.Count; }

        public CardPile(PlayMat? parent = null, Type? displayType = null)
        {
            Parent = parent;
            if (displayType != null) Display = (BaseWindow)Activator.CreateInstance(displayType, this)!;
        }

        protected virtual void InsertAt(int index, int where) {
            Queue<int> newShuffle = new Queue<int>();

            for (int i = 0; i < where; i++) newShuffle.Enqueue(CardOrder.Dequeue());
            newShuffle.Enqueue(index);
            while (CardOrder.Count > 0) newShuffle.Enqueue(CardOrder.Dequeue());

            CardOrder = newShuffle;
        }

        protected virtual void InsertAt(Card card, int where)
        {
            InsertAt(CardStock.IndexOf(card), where);
        }

        public virtual void RemoveCard(int index, bool all = false)
        {
            Queue<int> newShuffle = new Queue<int>();

            bool removed = false;
            while (CardOrder.Count > 0)
            {
                int idx = CardOrder.Dequeue();
                if ((!all || !removed) && idx == index)
                {
                    removed = true;
                    continue;
                }
                newShuffle.Enqueue(idx);
            }
            CardOrder = newShuffle;
        }

        public virtual void RemoveCard(Card card, bool all = false)
        {
            RemoveCard(CardStock.IndexOf(card), all);
        }

        public virtual void AddCard(int index, int amount = 1)
        {
            for (int i = 0; i < amount; i++) CardOrder.Enqueue(index);
        }

        public virtual void AddCard(Card card) {
            int count = card.Count;
            card.Count = 1;

            int index = CardStock.IndexOf(card);
            if (index == -1) CardStock.Add(card);
            index = CardStock.IndexOf(card);

            AddCard(index, count);
        }

        public virtual int IndexOf(int index) 
        {
            return CardOrder.IndexOf(index);
        }

        public virtual void PlaceOnTop(int index) { InsertAt(index, 0); }

        public virtual void PlaceOnTop(Card card) { PlaceOnTop(CardStock.IndexOf(card)); }

        public virtual void PlaceOnBottom(int index) { CardOrder.Enqueue(index); }

        public virtual void PlaceOnBottom(Card card) { PlaceOnBottom(CardStock.IndexOf(card)); }

        public virtual void PlaceAtRandom(int index)
        {
            Random rand = new Random();
            InsertAt(index, rand.Next(1, CardOrder.Count - 1));
        }

        public virtual void PlaceAtRandom(Card card) { PlaceAtRandom(CardStock.IndexOf(card)); }

        public virtual void Clear() { CardOrder.Clear(); }

        public virtual void Shuffle() { CardOrder = CardOrder.Shuffle(); }
    }
}
