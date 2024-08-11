﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MTGProxyDesk
{
    public partial class NumberPicker : UserControl, INotifyPropertyChanged
    {
        private Action<int, object> _OnChange;
        public Action<int, object> OnChange
        {
            get => _OnChange;
            set => _OnChange = value;
        }

        private int _Min = 1;
        public int Min
        {
            get => _Min;
            set
            {
                if (_Min != value)
                {
                    _Min = value;
                    _Value = Math.Max(value, _Value);
                }
            }
        }

        private int _Max = 4;
        public int Max
        {
            get => _Max;
            set
            {
                if (_Max != value)
                {
                    _Max = value;
                    _Value = Math.Min(value, _Value);
                }
            }
        }

        private int _Value = 1;
        public string Value
        {
            get => _Value.ToString();
            set
            {
                try
                {
                    int newValue = int.Parse(value);
                    _Value = Math.Max(Min, Math.Min(Max, newValue));
                    this.OnPropertyChanged("Value");
                    this.OnChange(_Value, this);
                } catch { }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public NumberPicker()
        {
            InitializeComponent();
            this.DataContext = this;
            _OnChange = (int _, object __) => { };
        }

        public void Add(int howMany = 1)
        {
            Value = Math.Min(_Value + howMany, Max).ToString();
        }

        public void Add(object sender, RoutedEventArgs e)
        {
            Add();
        }

        public void Subtract(int howMany = 1)
        {
            Value = Math.Max(_Value - howMany, Min).ToString();
        }

        public void Subtract(object sender, RoutedEventArgs e)
        {
            Subtract();
        }

        private void OnPropertyChanged(string propertyName)
        {
            var prop = this.PropertyChanged;
            if (prop != null) prop(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
