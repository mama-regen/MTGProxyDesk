using System.Text.RegularExpressions;
using System.Windows.Media;

namespace MTGProxyDesk.Extensions
{
    public static class StringExt
    {
        public static string GetFileType(this string filePath)
        {
            Regex reg = new Regex("[^a-z]", RegexOptions.IgnoreCase);
            return reg.Split(filePath.Split('.').Last()).First();
        }

        public static SolidColorBrush AsBrush(this string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(color)!;
        }

        public static Color AsColor(this string color)
        {
            return (Color)ColorConverter.ConvertFromString(color);
        }
    }
}
