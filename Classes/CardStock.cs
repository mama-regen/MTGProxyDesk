using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace MTGProxyDesk.Classes
{
    public sealed class CardStock
    {
        private static CardStock? _instance = null;
        private static object _lock = new object();
        private static CardStock Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CardStock();
                    }
                    return _instance;
                }
            }
        }

        private List<Card> Cards { get; set; }
        private bool _isCommander { get; set; } = false;
        public static bool IsCommander
        {
            get => Instance._isCommander;
        }

        private CardStock() { Cards = new List<Card>(); }

        public static int Add(Card card)
        {
            int idx = IndexOf(card);
            if (idx >= 0) return idx;

            Instance.Cards.Add(card);
            return Instance.Cards.Count - 1;
        }

        public static Card? Get(int idx)
        {
            if (idx < 0 || idx >= Instance.Cards.Count) return null;
            return Instance.Cards[idx];
        }

        public static Card? Get(int? idx)
        {
            return Get(idx ?? -1);
        }

        public static int IndexOf(Card card)
        {
            for (int i = 0; i < Instance.Cards.Count; i++)
            {
                Card? check = Get(i);
                if (check != null && card.Id == check.Id) return i;
            }
            return -1;
        }

        public static Queue<int> Load(string filePath, BackgroundWorker bgWorker)
        {
            Queue<int> ids = new Queue<int>();

            int[] PngHeader = new int[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                byte[] flags = new byte[2];
                fs.Read(flags, 0, 2);

                Instance._isCommander = flags[0] != 0;
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
                    for (int i = 0; i < 8; i++)
                    {
                        if (imgBytes[i] != PngHeader[i])
                        {
                            isPng = false;
                            break;
                        }
                    }

                    string imgPath = Path.Join(Helper.TempFolder, id + (isPng ? ".png" : ".jpg"));
                    File.WriteAllBytes(imgPath, imgBytes);
                    Card card = new Card(id, imgPath, 1, anyAmount);

                    Instance.Cards.Add(card);
                    for (int i = 0; i < count; i++) ids.Enqueue(Instance.Cards.Count() - 1);
                }
            }

            return ids;
        }

        public static void Save(string filePath, Dictionary<int, int> cardCount)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add((byte)(IsCommander ? 0 : 1));
            buffer.Add((byte)cardCount.Count());

            foreach (KeyValuePair<int, int> item in cardCount)
            {
                Card card = Get(item.Key)!;
                char[] id = card.Id.Replace("-", "").ToCharArray();

                foreach (char c in id) buffer.Add((byte)c);
                buffer.Add((byte)item.Value);
                buffer.Add((byte)(card.AllowAnyAmount ? 1 : 0));

                Func<int, int, byte> MakeByte = (i, p) => (byte)((i >> (8 * p)) & 255);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(card.Image));
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
        }
    }
}
