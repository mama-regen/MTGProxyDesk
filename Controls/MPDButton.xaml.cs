using System.ComponentModel;
using System.Windows;

namespace MTGProxyDesk.Controls
{
    public partial class MPDButton : MPDControl, INotifyPropertyChanged
    {
        public static readonly RoutedEvent MPDClickEvent = EventManager.RegisterRoutedEvent(
            name: "ClickEvent",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(MPDButton)
        );

        public event RoutedEventHandler ClickEvent
        {
            add { AddHandler(MPDClickEvent, value); }
            remove { RemoveHandler(MPDClickEvent, value); }
        }

        void RaiseCustomRoutedEvent()
        {
            RoutedEventArgs routedEventArgs = new(routedEvent: MPDClickEvent);
            RaiseEvent(routedEventArgs);
        }

        private Action<object, RoutedEventArgs> _click = (_, __) => { };
        public Action<object, RoutedEventArgs> Click
        {
            get => _click;
            set
            {
                _click = (_, __) =>
                {
                    RaiseCustomRoutedEvent();
                    value(_, __);
                };
            }
        }

        private string _content = "";
        public override string TextContent
        {
            get => base.TextContent;
            set 
            {
                base.TextContent = value;
                OnPropertyChanged("TextContent");
            }
        }

        public override Visibility CtrlVisibility
        {
            get => (Visibility)GetValue(VisibilityProperty);
            set
            {
                if (Visibility != value)
                {
                    SetValue(VisibilityProperty, value);
                    OnPropertyChanged("CtrlVisibility");
                    OnPropertyChanged("Visibility");
                }
            }
        }

        public CornerRadius CornerRadius { get; set; } = new CornerRadius(2);

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public MPDButton() : base()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void ClickHandler(object sender, RoutedEventArgs e)
        {
            _click(sender, e);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
