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

        private bool _tapped = false;
        public bool Tapped
        {
            get => _tapped;
            set
            {
                if (_tapped != value)
                {
                    TapVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                    UnTapVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public static readonly DependencyProperty TapVisibilityProperty = DependencyProperty.Register(
            "TapVisibility",
            typeof(Visibility),
            typeof(CardControl),
            new PropertyMetadata(Visibility.Visible)
        );

        public static readonly DependencyProperty UnTapVisibilityProperty = DependencyProperty.Register(
            "UnTapVisibility",
            typeof(Visibility),
            typeof(CardControl),
            new PropertyMetadata(Visibility.Collapsed)
        );

        public Visibility TapVisibility 
        { 
            get => (Visibility)GetValue(TapVisibilityProperty);
            set
            {
                if (TapVisibility != value)
                {
                    SetValue(TapVisibilityProperty, value);
                    _tapped = value == Visibility.Collapsed;
                    OnPropertyChanged("TapVisibility");
                }
            }
        }
        public Visibility UnTapVisibility { 
            get => (Visibility)GetValue(UnTapVisibilityProperty);
            set
            {
                if (UnTapVisibility != value)
                {
                    SetValue(UnTapVisibilityProperty, value);
                    _tapped = value == Visibility.Visible;
                    OnPropertyChanged("UnTapVisibility");
                }
            }
        }

        private int? _Card;
        public int? Card
        {
            get { return _Card; }
            set
            {
                if (value == null)
                {
                    _Card = null;
                    SetToDefaultImage();
                    return;
                }

                _Card = value;
                OnPropertyChanged("Card");
                Card? cardActual = CardStock.Get(_Card!.Value);
                if (cardActual == null) SetToDefaultImage();
                else CardImage = new ImageBrush(cardActual!.Image);
            }
        }

        private int _count = 0;
        public int Count
        {
            get => _Card == null ? 0 : _count;
            set
            {
                if (_Card != null) _count = value;
            }
        }

        public bool Assignable { get; set; } = true;

        private bool _defaultImg = true;
        public bool DefaultImageOnEmpty 
        {
            get => _defaultImg;
            set
            {
                _defaultImg = value;
                defaultImage = null;
                if (_Card == null) SetToDefaultImage(); 
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
            Card? heldCard = HeldCard.Get();
            if (heldCard == null)
            {
                if (Card != null)
                {
                    PlayMat? playMat = this.GetAncestorOfType<PlayMat>();
                    MPDButton? btn = this.GetChildrenOfType<MPDButton>().Where(b => b.Uid == "CmdBtn").FirstOrDefault();

                    if (playMat == null || !playMat.IsCommander)
                    {
                        if (btn != null) btn.Visibility = Visibility.Collapsed;
                        ShowMenu();
                        return;
                    }

                    Card? cmdMaybe = CardStock.Get(playMat!.CommanderControl.Card);
                    Card? crdMaybe = CardStock.Get(Card);

                    if (
                        crdMaybe != null &&
                        cmdMaybe != null &&
                        crdMaybe!.Id == cmdMaybe!.Id &&
                        btn != null
                    ) btn.Visibility = Visibility.Visible;
                    else if (btn != null) btn.Visibility = Visibility.Collapsed;

                    ShowMenu();
                }
                else if (DefaultImageOnEmpty) ShowMenu();
                return;
            }

            if (!Assignable) return;

            if (Card != null)
            {
                if (Card == CardStock.IndexOf(heldCard))
                {
                    Count++;
                    TextContent = Count.ToString();
                }
                return;
            }

            Card = CardStock.IndexOf(heldCard);
            HeldCard.Set(null);
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

        public void PhaseIn()
        {
            Opacity = 1.0;
            IsHitTestVisible = true;
        }

        public void PhaseOut()
        {
            HideMenu();
            Opacity = 0.6;
            IsHitTestVisible = false;
        }

        public void Tap()
        {
            Tapped = true;
        }

        public void UnTap()
        {
            Tapped = false;
        }

        private void SetToDefaultImage()
        {
            if (defaultImage == null)
            {
                defaultImage = DefaultImageOnEmpty ?
                    new BitmapImage(
                        new Uri(
                            @"pack://application:,,,/" +
                            Assembly.GetCallingAssembly().GetName().Name +
                            ";component/img/card_back.png",
                            UriKind.Absolute
                        )
                    ) :
                    BitmapImage.Create(
                        1, 1, 96, 96,
                        PixelFormats.Bgra32, null,
                        new Byte[] { 0, 0, 0, 0 }, 4
                    ) as BitmapImage;
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
