using System.ComponentModel;
using System.Windows;

namespace MTGProxyDesk.Windows
{
    public abstract class BaseWindow : Window
    {
        public static Action BringToFront { get; private set; } = () => { };
        public bool CanClose { get; set; } = false;

        public BaseWindow()
        {
            BringToFront = () =>
            {
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CanClose) 
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                return;
            }

            base.OnClosing(e);
        }
    }
}
