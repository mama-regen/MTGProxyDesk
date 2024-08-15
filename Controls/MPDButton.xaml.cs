using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using MTGProxyDesk.Constants;
using MTGProxyDesk.Extensions;

namespace MTGProxyDesk.Controls
{
    public partial class MPDButton : MPDControl, INotifyPropertyChanged
    {
        private Action<object, RoutedEventArgs> _click = (_, __) => { };
        public Action<object, RoutedEventArgs> Click
        {
            get => _click;
            set => _click = value;
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
