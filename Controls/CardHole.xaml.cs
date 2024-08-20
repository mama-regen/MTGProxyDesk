using MTGProxyDesk.Classes;
using MTGProxyDesk.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTGProxyDesk.Controls
{
    public partial class CardHole : MPDControl, INotifyPropertyChanged
    {
        private Card? assignedCard;

        private Visibility _visibility = Visibility.Visible;
        public override Visibility CtrlVisibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                OnPropertyChanged("CtrlVisibility");
            }
        }

        public Visibility LabelVisibility
        {
            get => assignedCard == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public SolidColorBrush HoverColor
        {
            get => assignedCard == null ? (SolidColorBrush)FindResource("FG3") : Brushes.Transparent;
        }

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public CardHole()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool AssignCard()
        {
            if (assignedCard != null) return false;
            assignedCard = Hand.Instance.CardBuffer;
            Hand.Instance.CardBuffer = null;

            CardControl cardControl = new CardControl();
            cardControl.Card = assignedCard;

            StackPanel container = this.GetChildOfType<StackPanel>()!;
            container.Children.Clear();
            container.Children.Add(cardControl);
            OnPropertyChanged("HoverColor");
            OnPropertyChanged("LabelVisibility");
            return true;
        }

        public bool RemoveCard()
        {
            if (assignedCard == null) return false;
            Hand.Instance.CardBuffer = assignedCard;
            assignedCard = null;
            this.GetChildOfType<StackPanel>()!.Children.Clear();
            OnPropertyChanged("HoverColor");
            OnPropertyChanged("LabelVisibility");
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
