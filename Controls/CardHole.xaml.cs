using MTGProxyDesk.Classes;
using MTGProxyDesk.Extensions;
using MTGProxyDesk.Windows;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MTGProxyDesk.Controls
{
    public partial class CardHole : MPDControl, INotifyPropertyChanged
    {
        private bool locked = false;
        private bool phased = false;

        private bool _tapped = false;
        public bool Tapped
        {
            get => _tapped;
            set
            {
                if (_tapped != value)
                {
                    _tapped = value;
                    OnPropertyChanged("TapVisibility");
                    OnPropertyChanged("UntapVisibility");
                }
            }
        }

        public Visibility TapVisibility 
        {
            get => _tapped ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility UntapVisibility { 
            get => _tapped ? Visibility.Visible : Visibility.Collapsed; 
        }

        private Card? _assignedCard = null;
        public Card? AssignedCard
        {
            get => _assignedCard;
            set
            {
                if (_assignedCard != value)
                {
                    _assignedCard = value;
                    CardControl? cardCtrl = this.GetChildOfType<CardControl>();
                    if (cardCtrl != null) { cardCtrl.Card = value; }
                    if (_assignedCard == null) CardVisible = Visibility.Collapsed;
                    else CardVisible = Visibility.Visible;
                    OnPropertyChanged("AssignedCard");
                }
            }
        }

        private Visibility _cardVisible = Visibility.Collapsed;
        public Visibility CardVisible
        {
            get => _cardVisible;
            set
            {
                if (_cardVisible != value)
                {
                    _cardVisible = value;
                    OnPropertyChanged("CardVisible");
                }
            }
        }

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
            get => _assignedCard == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public SolidColorBrush HoverColor
        {
            get => _assignedCard == null ? (SolidColorBrush)FindResource("FG3") : Brushes.Transparent;
        }

        public virtual event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public CardHole()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void AssignCard(object sender, RoutedEventArgs e)
        {
            if (PlayMat.Instance.HeldCard.Card == null || locked)
            {
                locked = false;
                return;
            }
            if (AssignedCard != null && PlayMat.Instance.HeldCard.Card.Id != AssignedCard.Id) return;

            if (AssignedCard == null) AssignedCard = PlayMat.Instance.HeldCard.Card;
            else AssignedCard.Count++;

            if (AssignedCard.Count > 1) TextContent = AssignedCard.Count.ToString();

            PlayMat.Instance.HeldCard.Card = null;
            PlayMat.Instance.HeldCard.Visibility = Visibility.Collapsed;

            OnPropertyChanged("HoverColor");
            OnPropertyChanged("LabelVisibility");
        }

        public void RemoveCard()
        {
            AssignedCard = null;
            Tapped = false;
            TextContent = "";

            OnPropertyChanged("HoverColor");
            OnPropertyChanged("LabelVisibility");
        }

        public void MoveToGraveyard(object sender, RoutedEventArgs e)
        {
            CardControl? cardCtrl = this.GetChildOfType<CardControl>();
            if (cardCtrl == null || cardCtrl.Card == null) return;
            
            Graveyard.Instance.AddCard(cardCtrl.Card);

            if (cardCtrl.Card.Count == 1) RemoveCard();
            else
            {
                cardCtrl.Card.Count--;
                TextContent = cardCtrl.Card.Count > 1 ? cardCtrl.Card.Count.ToString() : "";
            }
            PlayMat.Instance.UpdateGraveyard();
        }

        public void MoveToExile(object sender, RoutedEventArgs e)
        {
            CardControl? cardCtrl = this.GetChildOfType<CardControl>();
            if (cardCtrl == null || cardCtrl.Card == null) return;

            Exile.Instance.AddCard(cardCtrl.Card);

            if (cardCtrl.Card.Count == 1) RemoveCard();
            else
            {
                cardCtrl.Card.Count--;
                TextContent = cardCtrl.Card.Count > 1 ? cardCtrl.Card.Count.ToString() : "";
            }
            PlayMat.Instance.UpdateExile();
        }

        public void MoveToHand(object sender, RoutedEventArgs e)
        {
            CardControl? cardCtrl = this.GetChildOfType<CardControl>();
            if (cardCtrl == null || cardCtrl.Card == null) return;

            Hand.Instance.AddCard(cardCtrl.Card);

            if (cardCtrl.Card.Count == 1) RemoveCard();
            else
            {
                cardCtrl.Card.Count--;
                TextContent = cardCtrl.Card.Count > 1 ? cardCtrl.Card.Count.ToString() : "";
            }
            HandDisplay.Instance.DisplayHand();
        }

        public void MoveCard(object sender, RoutedEventArgs e)
        {
            CardControl? cardCtrl = this.GetChildOfType<CardControl>();
            if (cardCtrl == null || cardCtrl.Card == null) return;

            locked = true;

            PlayMat.Instance.HeldCard.Card = cardCtrl.Card;
            PlayMat.Instance.HeldCard.Visibility = Visibility.Visible;

            RemoveCard();
        }

        public void TapCard()
        {
            Tapped = true;
        }

        public void TapCard(object sender, RoutedEventArgs e)
        {
            TapCard();
        }

        public void UntapCard()
        {
            Tapped = false;
        }

        public void UntapCard(object sender, RoutedEventArgs e)
        {
            UntapCard();
        }

        public void PhaseOut()
        {
            if (phased) return;
            locked = true;
            phased = true;
            Opacity = 0.4;
            IsHitTestVisible = false;
        }

        public void PhaseIn()
        {
            if (!phased) return;
            locked = false;
            phased = false;
            Opacity = 1;
            IsHitTestVisible = true;
        }

        public void SetButtons(params MPDButton[] buttons)
        {
            CardControl child = this.GetChildOfType<CardControl>()!;
            child.Children.Clear();
            foreach (MPDButton button in buttons)
            {
                child.Children.Add(button);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
