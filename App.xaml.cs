using MTGProxyDesk.Windows;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MTGProxyDesk
{
    public partial class App : Application
    {
        public static (BitmapImage Image, string Artist)? StartBG { get; private set; } = null;

        protected override async void OnStartup(StartupEventArgs e)
        {
            StartBG = await Card.GetRandomArt();

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
