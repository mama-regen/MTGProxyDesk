using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MTGProxyDesk.Classes;
using MTGProxyDesk.Extensions;

namespace MTGProxyDesk.Controls
{
    [ContentProperty(nameof(Children))]
    public partial class CardControl : MPDControl, INotifyPropertyChanged
    {
        private BitmapImage? defaultImage = null;

        public Visibility TextVisibility { get; private set; } = Visibility.Visible;

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
                    OnPropertyChanged("Card");
                    CardImage = new ImageBrush(_Card.Image);
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

        public Brush HoverColor
        {
            get { return Menu == null || Menu.GetChildOfType<MPDButton>() == null ? Brushes.Transparent : (LinearGradientBrush)FindResource("WUBRG_MTG_Diag"); }
        }

        private ImageBrush? _CardImage;
        public ImageBrush? CardImage
        {
            get { return _CardImage; }
            set
            {
                _CardImage = value;
                OnPropertyChanged("CardImage");
            }
        }

        private Visibility _visibility;
        public override Visibility CtrlVisibility
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    Visibility = value;
                    OnPropertyChanged("CtrlVisibility");
                    OnPropertyChanged("Visibility");
                }
            }
        }

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public CardControl()
        {
            InitializeComponent();
            DataContext = this;
            Children = Menu.Children;
            SetToDefaultImage();
            HideMenu();
        }

        public void HandleClick(object sender, RoutedEventArgs e)
        {
            if (Hand.Instance.CardBuffer == null)
            {
                ShowMenu();
                return;
            }

            if (Card == null)
            {
                Card = Hand.Instance.CardBuffer;
                Hand.Instance.CardBuffer = null;
                return;
            }

            if (Hand.Instance.CardBuffer!.Id != Card.Id) return;

            Count += Hand.Instance.CardBuffer.Count;
            Hand.Instance.CardBuffer = null;
        }

        public void ShowMenu(object sender, RoutedEventArgs e)
        {
            ShowMenu();
        }

        public void ShowMenu() 
        {
            if (Menu == null) return;
            Menu.Visibility = Visibility.Visible;
            if (Menu.GetChildOfType<MPDButton>() == null) return;
            TextVisibility = Visibility.Collapsed;
            OnPropertyChanged("TextVisibility");
        }

        public void HideMenu(object sender, RoutedEventArgs e)
        {
            HideMenu();
        }

        public void HideMenu()
        {
            if (Menu == null) return;
            Menu.Visibility = Visibility.Hidden;
            TextVisibility = Visibility.Visible;
            OnPropertyChanged("TextVisibility");
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
            CardImage = new ImageBrush(defaultImage);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
