using MTGProxyDesk.Enums;

namespace MTGProxyDesk.Classes
{
    public sealed class HeldCard
    {
        private static HeldCard? _instance = null;
        private static object _lock = new object();
        private static HeldCard Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null) _instance = new HeldCard();
                    return _instance;
                }
            }
        }

        private int Card { get; set; } = -1;
        private PlaySource _source { get; set; } = PlaySource.Deck;

        private HeldCard() { }

        public static Card? Get() 
        {
            if (Instance.Card < 0) return null;
            return CardStock.Get(Instance.Card);
        }

        public static PlaySource Source { get => Instance._source; }

        public static void Set(int index, PlaySource source)
        {
            Instance.Card = index;
            Instance._source = source;
        }

        public static void Set(Card? card = null, PlaySource? source = null)
        {
            Instance.Card = card == null ? -1 : CardStock.IndexOf(card);
            Instance._source = source == null ? PlaySource.Deck : source!.Value;
        }
    }
}
