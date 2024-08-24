using MTGProxyDesk.Enums;
using MTGProxyDesk.Extensions;

namespace MTGProxyDesk.Classes
{
    public abstract class CardPile<T> where T : CardPile<T>
    {
        protected virtual PlaySource? PlaySource { get => null; }

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
            card.PlaySource = PlaySource;
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
            int sub = 0;
            bool removed = false;
            int idxOf = _cards.IndexOf(card);
            if (_shuffle.Where(s => s == idxOf).Count() == 1) 
            {
                sub = 1;
                _cards.Remove(card);
            }
            Queue<int> newShuffle = new Queue<int>();
            while (_shuffle.Count() > 0)
            {
                int sIdx = _shuffle.Dequeue();
                if (sIdx == idxOf && !removed)
                {
                    removed = true;
                    continue;
                }
                if (sIdx > idxOf) sIdx -= sub;
                newShuffle.Enqueue(sIdx);
            }

            _shuffle = newShuffle;
        }

        // Add card at [count] amount to deck
        public virtual void AddCard(Card card) {
            card.PlaySource = PlaySource;
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
            card.PlaySource = PlaySource;
            if (!_cards.Contains(card)) _cards.Add(card);
            _shuffle.Enqueue(_cards.Count - 1);
        }

        public virtual void PlaceAtRandom(Card card)
        {
            Random rand = new Random();
            InsertAt(card, rand.Next(1, CardCount - 1));
        }

        public virtual void Clear()
        {
            _cards.Clear();
            _shuffle.Clear();
        }

        public void Shuffle() { _shuffle = _shuffle.Shuffle(); }
    }
}
