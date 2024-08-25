using System.ComponentModel;
using System.IO;

namespace MTGProxyDesk.Classes
{
    public sealed class MagicDeck : CardPile
    {
        private string _filePath = "";
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                Name = Path.GetFileName(_filePath);
            }
        }

        public string Name { get; private set; } = "";

        public int LastIdx { get; private set; } = -1;

        public int? Commander { get; set; } = null;

        public Dictionary<int, int> CardCounts
        {
            get
            {
                Dictionary<int, int> headCount = new Dictionary<int, int>();
                if (Commander != null) headCount[Commander.Value] = 1;
                foreach (int cardIdx in CardOrder)
                {
                    if (!headCount.ContainsKey(cardIdx)) headCount[cardIdx] = 0;
                    headCount[cardIdx]++;
                }
                return headCount;
            }
        }

        public MagicDeck() { }

        public void SetParent(PlayMat parent) { Parent = parent; }

        public void Load(string filePath, BackgroundWorker bgWorker)
        {
            Commander = null;

            FilePath = filePath;

            CardOrder = CardStock.Load(filePath, bgWorker);
            if (CardStock.IsCommander) Commander = CardOrder.Dequeue();

            Shuffle();
        }

        public int Draw()
        {
            return CardOrder.Dequeue();
        }

        public int[] Next(int howMany = 1)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < Math.Min(howMany, CardOrder.Count); i++)
            {
                result.Add(CardOrder.ElementAt(i));
            }
            return result.ToArray();
        }

        public void Save(string filePath)
        {
            Dictionary<int, int> headCount = new Dictionary<int, int>();
            if (Commander != null) headCount[Commander.Value] = 1;
            foreach (int cardIdx in  CardOrder)
            {
                if (!headCount.ContainsKey(cardIdx)) headCount[cardIdx] = 0;
                headCount[cardIdx]++;
            }

            CardStock.Save(filePath, headCount);

            FilePath = filePath;
        }

        public override void Clear()
        {
            base.Clear();
            Commander = null;
            FilePath = "";
        }
    }
}
