namespace MTGProxyDesk
{
    public static class Constants
    {
        public static Dictionary<string, string> Colors = new Dictionary<string, string>
        {
            { "Background1", "#000000" },
            { "Background2", "#333333" },
            { "Foreground1", "#ffffff" },
            { "Foreground2", "#b4b4b4" },
            { "Red", "#ff0000" },
            { "Green", "#00ff00" },
            { "Blue", "#0000ff" },
            { "White", "#ffffff" },
            { "Black", "#000000" },
            { "RedTrue", "#faa890" },
            { "GreenTrue", "#9bd2ab" },
            { "BlueTrue", "#a9e1f8" },
            { "WhiteTrue", "#fefcd6" },
            { "BlackTrue", "#ccc2c1" }
        };

        public static double Version = 0.3;

        public static string UserAgent = "MTGProxyDesk/" + Version.ToString();
    }
}
