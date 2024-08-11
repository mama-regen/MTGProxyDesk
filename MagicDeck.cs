using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace MTGProxyDesk
{
    public sealed class MagicDeck
    {
        private static MagicDeck? _instance;
        private static readonly object _lock = new object();

        private List<Card?> Cards;
        private Queue<int> Shuffle;
        private int _LastIdx = 0;

        public Card? Commander;
        public Card? CardBuffer;
        
        public int LastIdx
        {
            get { return _LastIdx; }
        }

        private MagicDeck() 
        {
            Cards = new List<Card?>();
            Shuffle = new Queue<int>();
        }

        public static MagicDeck Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null) _instance = new MagicDeck();
                    return _instance;
                }
            }
        }

        public void Load(string filePath)
        {
            Commander = null;
            Cards = new List<Card?>();

            string temp_filePath = Path.Join(Path.GetTempPath(), "mtg_prox_desk");
            if (!Path.Exists(temp_filePath)) Directory.CreateDirectory(temp_filePath);
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                byte[] flags = new byte[2];
                fs.Read(flags, 0, 2);

                bool isCommander = flags[0] != 0;
                int cardCount = flags[1];

                for (int cc = 0; cc < cardCount; cc++)
                {
                    byte[] idBytes = new byte[37];
                    fs.Read(idBytes, 0, 37);

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
                    int imgLen = (idBytes[33] << 24) | (idBytes[34] << 16) | (idBytes[35] << 8) | (idBytes[36]);

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

                    string imgPath = Path.Join(temp_filePath, id + (isPng ? ".png" : ".jpg"));
                    File.WriteAllBytes(imgPath, imgBytes);

                    Card card = new Card(id, imgPath, count);

                    if (isCommander && Commander == null) Commander = card;
                    else
                    {
                        Cards.Append(card);
                        Shuffle.Enqueue(Cards.Count() - 1);
                    }
                }
            }

            Shuffle.Shuffle();
        }

        public void ShuffleDeck() { Shuffle.Shuffle(); }

        public Card Draw()
        {
            int nextIdx = Shuffle.Dequeue();
            _LastIdx = nextIdx;
            return Cards[nextIdx]!;
        }

        public List<Card> Next(int howMany = 1)
        {
            List<Card> result = new List<Card>();
            for (int i = 0; i < howMany; i++)
            {
                if (Cards[i] == null)
                {

                }
                result.Append(Cards[i]);
            }
            return result;
        }

        public int IndexOf(Card card)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i] != null && card.Id == Cards[i]!.Id) return i;
            }
            return -1;
        }

        public void PlaceOnTop(int idx)
        {
            Queue<int> newShuffle = new Queue<int>();
            newShuffle.Enqueue(idx);
            for (int i = 0; i < Shuffle.Count; i++) newShuffle.Enqueue(Shuffle.Dequeue());
            Shuffle = newShuffle;
        }

        public void PlaceOnTop(Card card)
        {
            int idx = IndexOf(card);
            PlaceOnTop(idx);
        }

        public void PlaceOnBottom(int idx)
        {
            Shuffle.Enqueue(idx);
        }

        public void PlaceOnBottom(Card card)
        {
            int idx = IndexOf(card);
            Shuffle.Enqueue(idx);
        }

        public void Add(Card card, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Cards.Add(card);
                Shuffle.Enqueue(Cards.Count - 1);
            }
        }

        public void Remove(int idx)
        {
            Cards[idx] = null;
            Queue<int> newShuffle = new Queue<int>();
            for (int i = 0; i < Shuffle.Count; i++)
            {
                int x = Shuffle.Dequeue();
                if (x != idx) newShuffle.Enqueue(x);
            }
            Shuffle = newShuffle;
        }

        public void Remove(Card card, int count = 0)
        {
            List<int> removeIdx = new List<int>();
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i] != null && Cards[i]!.Id == card.Id)
                {
                    Cards[i] = null;
                    removeIdx.Add(i);
                    if (count > 0 && removeIdx.Count() >= count) break;
                }
            }
            Queue<int> newShuffle = new Queue<int>();
            for (int i = 0; i < Shuffle.Count; i++)
            {
                int x = Shuffle.Dequeue();
                if (!removeIdx.Contains(x)) newShuffle.Enqueue(x);
            }
            Shuffle = newShuffle;
        }

        public void Save(string filePath)
        {
            Dictionary<string, (int, string)> headCount = new Dictionary<string, (int, string)>();
            if (Commander != null) headCount[Commander.Id] = (1, Commander.LocalImagePath);
            foreach (Card? card in Cards)
            {
                if (card == null || card.Id == "") continue;
                if (headCount.ContainsKey(card.Id)) headCount[card.Id] = (
                    headCount[card.Id].Item1 + 1, 
                    headCount[card.Id].Item2
                );
                else headCount[card.Id] = (1, card.LocalImagePath);
            }

            List<byte> buffer = new List<byte>();
            buffer.Append((byte)(Commander == null ? 0 : 1));
            buffer.Append((byte)(headCount.Count() + (Commander == null ? 0 : 1)));

            foreach (KeyValuePair<string, (int, string)> item in headCount)
            {
                string id = item.Key.Replace("-", "");
                int count = item.Value.Item1;
                string imgPath = item.Value.Item2;

                foreach (char c in id.ToCharArray()) buffer.Append((byte)c);
                buffer.Append((byte)count);

                Func<int, int, byte> MakeByte = (i, p) => (byte)((i >> (8 * p)) & 255);

                using (FileStream stream = new FileStream(imgPath, FileMode.Open))
                {
                    int imgLen = (int)stream.Length;
                    for (int i = 3; i >= 0; i--) buffer.Append(MakeByte(imgLen, i));
                    byte[] bits = new byte[imgLen];
                    stream.Read(bits, 0, imgLen);
                    buffer.Concat(bits);
                }
            }

            if (!filePath.EndsWith(".mpd")) filePath += ".mpd";
            using (FileStream writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                writer.Write(buffer.ToArray(), 0, buffer.Count());
            }
        }
    }

    public static class Ext
    {
        private static Random rng = new Random();
        public static Queue<int> Shuffle(this IEnumerable<int> idx)
        {
            List<int> list = idx.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            Queue<int> result = new Queue<int>();
            foreach (int i in list) result.Enqueue(i);
            return result;
        }
    }
}
