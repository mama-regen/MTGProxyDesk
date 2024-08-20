using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace MTGProxyDesk.Classes
{
    public sealed class MagicDeck : CardPile<MagicDeck>
    {
        private string? _filePath;
        public string FilePath
        {
            get => _filePath ?? "";
        }

        private string? _name;
        public string Name
        {
            get => _name ?? "";
        }

        public int LastIdx { get; private set; } = -1;

        public Card? Commander;

        public Card[] CardsWithCount
        {
            get 
            {
                Dictionary<string, int> headCount = new Dictionary<string, int>();
                foreach (var card in CardShuffle) 
                {
                    if (headCount.Keys.Contains(card.Id)) headCount[card.Id]++;
                    else headCount[card.Id] = 1;
                }
                return Cards.Select(card =>
                {
                    card.Count = headCount[card.Id];
                    return card;

                }).ToArray()!;
            }
        }

        private MagicDeck() { }

        public void Load(string filePath, BackgroundWorker bgWorker)
        {
            Commander = null;
            _cards = new List<Card>();

            _filePath = filePath;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                byte[] flags = new byte[2];
                fs.Read(flags, 0, 2);

                bool isCommander = flags[0] != 0;
                int cardCount = flags[1];

                for (int cc = 0; cc < cardCount; cc++)
                {
                    float perc = cc * 100 / (float)cardCount;
                    bgWorker.ReportProgress((int)Math.Ceiling(perc));

                    byte[] idBytes = new byte[38];
                    fs.Read(idBytes, 0, 38);

                    string id = "";
                    for (int i = 0; i < 8; i++) id += (char)idBytes[i];
                    id += "-";
                    for (int i = 8; i < 12; i++) id += (char)idBytes[i];
                    id += "-";
                    for (int i = 12; i < 16; i++) id += (char)idBytes[i];
                    id += "-";
                    for (int i = 16; i < 20; i++) id += (char)idBytes[i];
                    id += "-";
                    for (int i = 20; i < 32; i++) id += (char)idBytes[i];

                    int count = idBytes[32];
                    bool anyAmount = idBytes[33] != 0;
                    int imgLen = (idBytes[34] << 24) | (idBytes[35] << 16) | (idBytes[36] << 8) | (idBytes[37]);

                    byte[] imgBytes = new byte[imgLen];
                    fs.Read(imgBytes, 0, imgLen);

                    bool isPng = true;
                    int[] pngHeader = new int[8] { 137, 80, 78, 71, 13, 10, 26, 10 };
                    for (int i = 0; i < 8; i++)
                    {
                        if (imgBytes[i] != pngHeader[i])
                        {
                            isPng = false;
                            break;
                        }
                    }

                    string imgPath = Path.Join(Helper.TempFolder, id + (isPng ? ".png" : ".jpg"));
                    File.WriteAllBytes(imgPath, imgBytes);

                    Card card = new Card(id, imgPath, 1, anyAmount);

                    if (isCommander && Commander == null) Commander = card;
                    else
                    {
                        _cards.Add(card);
                        for (int i = 0; i < count; i++) _shuffle.Enqueue(Cards.Count() - 1);
                    }
                }
            }
            _name = Path.GetFileName(filePath);

            Shuffle();
        }

        public Card Draw()
        {
            int nextIdx = _shuffle.Dequeue();
            LastIdx = nextIdx;
            return Cards[nextIdx]!;
        }

        public List<Card> Next(int howMany = 1)
        {
            List<Card> result = new List<Card>();
            for (int i = 0; i < Math.Min(howMany, _shuffle.Count); i++)
            {
                result.Add(Cards[_shuffle.ElementAt(i)]);
            }
            return result;
        }

        public void Save(string filePath)
        {
            Dictionary<string, (int, BitmapImage, bool)> headCount = new Dictionary<string, (int, BitmapImage, bool)>();
            if (Commander != null) headCount[Commander.Id] = (1, Commander.Image, false);
            foreach (Card? card in Cards)
            {
                if (card == null || card.Id == "") continue;
                if (headCount.ContainsKey(card.Id)) headCount[card.Id] = (
                    headCount[card.Id].Item1 + 1, 
                    headCount[card.Id].Item2,
                    headCount[card.Id].Item3
                );
                else headCount[card.Id] = (1, card.Image, card.AllowAnyAmount);
            }

            List<byte> buffer = new List<byte>();
            buffer.Add((byte)(Commander == null ? 0 : 1));
            buffer.Add((byte)headCount.Count());

            foreach (KeyValuePair<string, (int, BitmapImage, bool)> item in headCount)
            {
                string id = item.Key.Replace("-", "");
                int count = item.Value.Item1;
                BitmapImage img = item.Value.Item2;
                bool allowAny = item.Value.Item3;

                foreach (char c in id.ToCharArray()) buffer.Add((byte)c);
                buffer.Add((byte)count);
                buffer.Add((byte)(allowAny ? 1 : 0));

                Func<int, int, byte> MakeByte = (i, p) => (byte)((i >> (8 * p)) & 255);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    int imgSize = (int)ms.Length;
                    for (int i = 3; i >= 0; i--) buffer.Add(MakeByte(imgSize, i));
                    buffer = buffer.Concat(ms.ToArray()).ToList();
                }
            }

            if (!filePath.EndsWith(".mpd")) filePath += ".mpd";
            using (FileStream writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                writer.Write(buffer.ToArray(), 0, buffer.Count());
            }

            _name = Path.GetFileName(filePath);
        }

        public override void Clear()
        {
            base.Clear();
            Commander = null;
            _filePath = null;
            _name = null;
        }
    }
}
