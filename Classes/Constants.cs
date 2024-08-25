using System.Reflection;

namespace MTGProxyDesk.Constants
{
    public static class BuildInfo
    {
        public static double Version = 2.0;
        public static string Name = "MTGProxyDesk";
        public static string UserAgent { get => Name + "/" + Version; }
    }
}
