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
    }
}
