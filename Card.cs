using MtgApiManager.Lib.Model;
using Newtonsoft.Json;
using System.IO;
using System.IO.Enumeration;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace MTGProxyDesk
{
    public class Card
    {
        private class ImageUris
        {
            public string? Small { get; set; }
            public string? Normal { get; set; }
            public string? Large { get; set; }
            public string? Png { get; set; }
            public string? Art_Crop { get; set; }
            public string? Border_Crop { get; set; }
        }

        private class Legalities
        {
            public string Standard { get; set; } = "";
            public string Future { get; set; } = "";
            public string Historic { get; set; } = "";
            public string Timeless { get; set; } = "";
            public string Gladiator { get; set; } = "";
            public string Pioneer { get; set; } = "";
            public string Explorer { get; set; } = "";
            public string Modern { get; set; } = "";
            public string Legacy { get; set; } = "";
            public string Pauper { get; set; } = "";
            public string Vintage { get; set; } = "";
            public string Penny { get; set; } = "";
            public string Commander { get; set; } = "";
            public string Oathbreaker { get; set; } = "";
            public string StandardBrawl { get; set; } = "";
            public string Brawl { get; set; } = "";
            public string Alchemy { get; set; } = "";
            public string PauperCommander { get; set; } = "";
            public string Duel { get; set; } = "";
            public string OldSchool { get; set; } = "";
            public string PreModern { get; set; } = "";
            public string PreEDH { get; set; } = "";
        }

        private class Preview
        {
            public string? Source { get; set; }
            public string? Source_Uri { get; set; }
            public DateTime? Previewed_At { get; set; }
        }

        private class Prices
        {
            public string? USD { get; set; }
            public string? USD_Foil { get; set; }
            public string? USD_Etched { get; set; }
            public string? EUR { get; set; }
            public string? EUR_Foil { get; set; }
            public string? TIX { get; set; }
        }

        private class RelatedUris
        {
            public string? Gatherer { get; set; }
            public string? Tcgplayer_Infinite_Articles { get; set; }
            public string? Tcgplayer_Infinite_Decks { get; set; }
            public string? EDHrec { get; set; }
        }

        private class PurchaseUris
        {
            public string? Tcgplayer { get; set; }
            public string? Cardmarket { get; set; }
            public string? Cardhoarder { get; set; }
        }

        private class _Card
        {
            public string Object { get; set; } = "card";
            public string Id { get; set; } = "";
            public string Oracle_Id { get; set; } = "";
            public int[]? Multiverse_Ids { get; set; }
            public int? Mtgo_Id { get; set; }
            public int? Tcgplayer_Id { get; set; }
            public int? Cardmarket_Id { get; set; }
            public string Name { get; set; } = "";
            public string Lang { get; set; } = "";
            public DateTime Released_At { get; set; }
            public string Uri { get; set; } = "";
            public string Scryfall_Uri { get; set; } = "";
            public string Layout { get; set; } = "";
            public bool Highres_Image { get; set; }
            public string Image_Status { get; set; } = "";
            public ImageUris Image_Uris { get; set; }
            public string ManaCost { get; set; } = "";
            public float Cmc { get; set; }
            public string Type_Line { get; set; } = "";
            public string Oracle_Text { get; set; } = "";
            public string? Power { get; set; }
            public string? Toughness { get; set; }
            public string[] Colors { get; set; }
            public string[] ColorIdentity { get; set; }
            public string[] Keywords { get; set; }
            public string[] Produced_Mana { get; set; }
            public Legalities Legalities { get; set; }
            public string[] Games { get; set; }
            public bool Reserved { get; set; }
            public bool Foil { get; set; }
            public bool NonFoil { get; set; }
            public string[]? Finishes { get; set; }
            public bool Oversized { get; set; }
            public bool Promo { get; set; }
            public bool Reprint { get; set; }
            public bool Variation { get; set; }
            public string SetId { get; set; } = "";
            public string Set { get; set; } = "";
            public string Set_Name { get; set; } = "";
            public string Set_Type { get; set; } = "";
            public string Set_Uri { get; set; } = "";
            public string Set_Search_Uri { get; set; } = "";
            public string Scryfall_Set_Uri { get; set; } = "";
            public string Rulings_Url { get; set; } = "";
            public string Prints_Search_Uri { get; set; } = "";
            public string Collector_Number { get; set; } = "";
            public bool Digital { get; set; }
            public string Rarity { get; set; } = "";
            public string? Flavor_Text { get; set; }
            public string Card_Back_Id { get; set; } = "";
            public string Artist { get; set; } = "";
            public string[] Artist_Ids { get; set; }
            public string Illustration_Id { get; set; } = "";
            public string Border_Color { get; set; } = "";
            public string Frame { get; set; } = "";
            public string[]? Frame_Effects { get; set; }
            public string? Security_Stamp { get; set; }
            public bool Full_Art { get; set; }
            public bool Textless { get; set; }
            public bool Booster { get; set; }
            public bool Story_Spotlight { get; set; }
            public int? EDHrec_Rand { get; set; }
            public Preview? Preview { get; set; }
            public Prices Prices { get; set; }
            public RelatedUris Related_Uris { get; set; }
            public PurchaseUris Purchase_Uris { get; set; }

            public _Card()
            {
                Image_Uris = new ImageUris();
                Legalities = new Legalities();
                Prices = new Prices();
                Related_Uris = new RelatedUris();
                Purchase_Uris = new PurchaseUris();

                Colors = new string[0];
                ColorIdentity = new string[0];
                Keywords = new string[0];
                Produced_Mana = new string[0];
                Games = new string[0];
                Artist_Ids = new string[0];
            }
        }

        private class _CardSearch
        {
            public string Object { get; set; } = "";
            public int Total_Cards { get; set; } = 0;
            public bool Has_More { get; set; } = false;
            public _Card[] Data { get; set; }

            public _CardSearch() {
                Data = new _Card[0];
            }
        }

        public string Id { get; private set; }
        public BitmapImage Image { get; private set; }
        public bool AllowAnyAmount { get; private set; } = false;
        private int _Count = 1;
        public int Count
        {
            get => _Count;
            set => _Count = Math.Max(0, Math.Min(AllowAnyAmount ? int.MaxValue : 4, value));
        }
       
        private Card(_Card card)
        {
            Id = card.Id!;

            string ext = ".jpg";
            if (card.Image_Uris.Png != null) ext = ".png";
            string filepath = Path.Join(Path.GetTempPath(), "mtg_prox_desk", card.Id + ext);

            Image = Helper.DownloadImage(new Uri(card.Image_Uris.Png ?? card.Image_Uris.Large ?? card.Image_Uris.Normal!), filepath);
            AllowAnyAmount = card.Type_Line.ToLower().Contains("basic land") || card.Oracle_Text.ToLower().Contains("a deck can have any number of cards named " + card.Name.ToLower());
        }

        public Card(string id, string localImagePath, int count, bool anyAmount = false)
        {
            Id = id;
            Image = Helper.LoadBitmap(localImagePath);
            Count = count;
            AllowAnyAmount = anyAmount;
        }

        public static async Task<Card?> SearchCard(string name)
        {
            string? response = await MakeRequest("named?include_extras=false&fuzzy=" + name.Replace(" ", "+"));
            if (response == null) return null;
            return new Card(JsonConvert.DeserializeObject<_Card>(response));
        }

        public static async Task<Card?> SearchToken(string name)
        {
            string? response = await MakeRequest("search?include_extras=true&q=t%3Atoken%20" + name.Replace(" ", "+"));
            if (response == null) return null;
            IEnumerable<_Card> filtered = JsonConvert.DeserializeObject<_CardSearch>(response).Data.Where(c => c.Name.ToLower() == name.ToLower());
            if (filtered.Count() == 0) return null;
            return new Card(filtered.First());
        }

        public static async Task<Card[]> SearchTokens(string name)
        {
            string? response = await MakeRequest("search?include_extras=true&q=t%3Atoken%20" + name.Replace(" ", "+"));
            if (response == null) return new Card[0];
            return JsonConvert.DeserializeObject<_CardSearch>(response).Data.Select(c => new Card(c)).ToArray();
        }

        public static async Task<Card?> GetCard(string id)
        {
            string? response = await MakeRequest(id);
            if (response == null) return null;
            return new Card(JsonConvert.DeserializeObject<_Card>(response));
        }

        public static async Task<(BitmapImage, string)?> GetRandomArt(string fileName = "temp") { return await GetRandomArt(fileName,5); }
        private static async Task<(BitmapImage, string)?> GetRandomArt(string fileName, int attempts)
        {
            string? response = await MakeRequest("search?q=new%3Aart&format=image&version=art_crop");
            if (response == null)
            {
                if (attempts == 0) return null;
                return await GetRandomArt(fileName, attempts - 1);
            }
            Random rand = new Random();
            _Card[] data = JsonConvert.DeserializeObject<_CardSearch>(response).Data;

            Func<_Card?> pick = () =>
            {
                _Card select = data.ElementAt(rand.Next(0, data.Length - 1));
                if (select.Image_Uris != null && select.Image_Uris.Art_Crop != null) return select;
                return null;
            };

            _Card? chosen = null;
            while (chosen == null) chosen = pick();
            return (
                Helper.DownloadImage(new Uri(chosen.Image_Uris!.Art_Crop!), Path.Join(Path.GetTempPath(), "mtg_prox_desk", fileName + ".png")),
                chosen.Artist
            );
        }

        private static async Task<string?> MakeRequest(string query)
        {
            const string URL = "https://api.scryfall.com/";

            string? response = null;
            Func<string, Task> func = async (_query) =>
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgent);

                string q = (string.IsNullOrWhiteSpace(_query) || _query.Trim()[0] == '?' ? "?" : "&");
                if (!_query.Contains("format")) q += "version=png&format=json";

                string searchQuery = "cards/" + _query + q;

                HttpResponseMessage _response = client.GetAsync(searchQuery).Result;
                if (_response.IsSuccessStatusCode) response = await _response.Content.ReadAsStringAsync();
            };

            var wrapper = func.Debounce(750);
            await wrapper(query);

            return response;
        }
    }
}
