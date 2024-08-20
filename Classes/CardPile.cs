using MTGProxyDesk.Controls;
using MTGProxyDesk.Extensions;

namespace MTGProxyDesk.Classes
{
    public abstract class CardPile<T> where T : CardPile<T>
    {
        private static readonly Lazy<T> Init = new(() => (Activator.CreateInstance(typeof(T), true) as T)!);
        public static T Instance => Init.Value;

        protected List<Card> _cards = new List<Card>();
        public Card[] Cards { get => _cards.ToArray(); }

        protected Queue<int> _shuffle = new Queue<int>();
        public Card[] CardShuffle { get => _shuffle.Select(i => _cards[i]).ToArray(); }

        public Card? CardBuffer { get; set; }
        
        public virtual int CardCount { get => _shuffle.Count; }

        // Insert single instance of card at place [index] in shuffle
        protected virtual void InsertAt(Card card, int index) {
            card.Count = 1;
            int idx = _cards.IndexOf(card);
            if (idx == -1)
            {
                _cards.Add(card);
                idx = _cards.Count - 1;
            }

            Queue<int> newShuffle = new Queue<int>();

            for (int i = 0; i < index; i++) newShuffle.Enqueue(_shuffle.Dequeue());
            newShuffle.Enqueue(idx);
            while (_shuffle.Count > 0) newShuffle.Enqueue(_shuffle.Dequeue());

            _shuffle = newShuffle;
        }

        // Remove all instances of card from deck
        public virtual void RemoveCard(Card card) 
        {
            int idx = _cards.IndexOf(card);
            Queue<int> newShuffle = new Queue<int>();

            _cards.Remove(card);
            while (_shuffle.Count > 0)
            {
                int sIdx = _shuffle.Dequeue();
                if (sIdx == idx) continue;
                if (sIdx > idx) sIdx--;
                newShuffle.Enqueue(sIdx);
            }

            _shuffle = newShuffle;
        }

        // Remove card at place [index] in shuffle.
        public virtual void RemoveCard(int index)
        {
            int idx = _shuffle.ElementAt(index);
            Queue<int> newShuffle = new Queue<int>();

            if (_shuffle.Where(si => si == idx).Count() > 1) return;
            _cards.RemoveAt(idx);
            while (_shuffle.Count > 0)
            {
                int sIdx = _shuffle.Dequeue();
                if (sIdx == idx) continue;
                if (sIdx > idx) sIdx--;
                newShuffle.Enqueue(sIdx);
            }

            _shuffle = newShuffle;
        }

        // Add card at [count] amount to deck
        public virtual void AddCard(Card card) { 
            int cnt = card.Count;
            card.Count = 1;
            int idx = _cards.IndexOf(card);
            
            if (idx == -1)
            {
                _cards.Add(card);
                idx = _cards.Count - 1;
            }

            for (int i = 0; i < cnt; i++) _shuffle.Enqueue(idx);
        }

        // First index of card in shuffle
        public virtual int IndexOf(Card card) 
        {
            int idx = _cards.IndexOf(card);
            return _shuffle.IndexOf(idx);
        }

        public virtual void PlaceOnTop(Card card) { InsertAt(card, 0); }

        public virtual void PlaceOnBottom(Card card) 
        {
            if (!_cards.Contains(card)) _cards.Add(card);
            _shuffle.Enqueue(_cards.Count - 1);
        }

        public virtual void Clear()
        {
            _cards.Clear();
            _shuffle.Clear();
        }

        public void Shuffle() { _shuffle = _shuffle.Shuffle(); }
    }
}
