using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MTGProxyDesk
{
    [ContentProperty(nameof(Children))]
    public partial class CardControl : UserControl, INotifyPropertyChanged
    {
        private BitmapImage? defaultImage = null;

        private SolidColorBrush _bg1 = Constants.Colors["Background1"].AsBrush();
        public SolidColorBrush BG1 { get => _bg1; }

        private SolidColorBrush _bg2 = Constants.Colors["Background2"].AsBrush();
        public SolidColorBrush BG2 { get => _bg2; }

        private SolidColorBrush _fg1 = Constants.Colors["Foreground1"].AsBrush();
        public SolidColorBrush FG1 { get => _fg1; }

        private SolidColorBrush _fg2 = Constants.Colors["Foreground2"].AsBrush();
        public SolidColorBrush FG2 { get => _fg2; }

        private Card? _Card;
        public Card? Card
        {
            get { return _Card; }
            set
            {
                if (value == null)
                {
                    _Card = null;
                    SetToDefaultImage();
                }
                else
                {
                    _Card = value;
                    this.OnPropertyChanged("Card");
                    CardImage = _Card.Image;
                }
            }
        }

        public int Count
        {
            get => _Card == null ? 0 : _Card!.Count;
            set
            {
                if (_Card != null) _Card.Count = value;
            }
        }

        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(CardControl),
            new PropertyMetadata()
        );

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }

        private StackPanel? _Menu
        {
            get { return this.GetChildOfType<StackPanel>(); }
        }

        public Brush HoverColor
        {
            get { return _Menu == null || _Menu.GetChildOfType<Button>() == null ? Brushes.Transparent : Brushes.White; }
        }

        private BitmapSource? _CardImage;
        public BitmapSource? CardImage
        {
            get { return this._CardImage; }
            set
            {
                this._CardImage = value;
                this.OnPropertyChanged("CardImage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public CardControl()
        {
            InitializeComponent();
            this.DataContext = this;
            Children = ChildContainer.Children;
            SetToDefaultImage();
            HideMenu();
        }

        public void ShowMenu(object sender, RoutedEventArgs e)
        {
            ShowMenu();
        }

        public void ShowMenu() 
        {
            if (_Menu != null) { 
                _Menu.Visibility = Visibility.Visible;
                DependencyObject[] children = _Menu.GetChildrenOfType<DependencyObject>();
                foreach (DependencyObject child in children)
                {
                    var visibleProp = child.GetType().GetProperty("Visibility");
                    if (visibleProp == null) continue;
                    var activeProp = child.GetType().GetProperty("IsEnabled");
                    if (activeProp != null)
                    {
                        visibleProp.SetValue(child, (bool)activeProp.GetValue(child)! ? Visibility.Visible : Visibility.Hidden);
                    } else
                    {
                        visibleProp.SetValue(child, Visibility.Visible);
                    }
                }
            }
        }

        public void HideMenu(object sender, RoutedEventArgs e)
        {
            HideMenu();
        }

        public void HideMenu()
        {
            if (_Menu != null) { _Menu.Visibility = Visibility.Hidden; }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = this.PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetToDefaultImage()
        {
            if (defaultImage == null)
            {
                defaultImage = new BitmapImage(
                    new Uri(
                        @"pack://application:,,,/" + 
                        Assembly.GetCallingAssembly().GetName().Name + 
                        ";component/img/card_back.png", 
                        UriKind.Absolute
                    )
                );
            }
            CardImage = defaultImage;
        }
    }
}
