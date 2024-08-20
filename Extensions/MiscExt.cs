using System.Reflection;

namespace MTGProxyDesk.Extensions
{
    public static class MiscExt
    {
        public static int IndexOf<T>(this IEnumerable<T> items, T search)
        {
            for (int i = 0; i < items.Count(); i++)
            {
                if (EqualityComparer<T>.Default.Equals(items.ElementAt(i), search)) return i;
            }
            return -1;
        }

        public static Func<T, Task> Debounce<T>(this Func<T, Task> func, int milliseconds = 300)
        {
            CancellationTokenSource? cts = null;

            return (arg) =>
            {
                cts?.Cancel();
                cts = new CancellationTokenSource(milliseconds);

                return Task.Delay(milliseconds, cts.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully) func(arg);
                }, TaskScheduler.Default);
            };
        }

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

        private static void Copy<T>(T orig, T copy)
        {
            PropertyInfo[] props = orig!.GetType().GetProperties();
            FieldInfo[] fields = orig!.GetType().GetFields();
            foreach (PropertyInfo prop in props) prop.SetValue(copy, prop.GetValue(orig));
            foreach (FieldInfo field in fields) field.SetValue(copy, field.GetValue(orig));
        }

        public static T Copy<T>(this T orig) where T : new()
        {
            T copy = new();
            Copy(orig, copy);
            return (T)copy;
        }

        public static Card Copy(this Card orig, int? newCount = null)
        {
            string path = orig.Image.BaseUri.OriginalString;
            Card copy = new Card(orig.Id, path, newCount == null ? orig.Count : newCount.Value, orig.AllowAnyAmount);
            return copy;
        }
    }
}
