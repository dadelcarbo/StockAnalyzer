using StockAnalyzer.Portofolio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels
{
    public enum OrderCategory
    {
        Stop,
        Target
    };
    public enum OrderType
    {
        Long,
        Short
    };
    public class OrderViewModel : INotifyPropertyChanged
    {
        public OrderViewModel() { }

        public OrderViewModel(int number, float openValue, OrderType type, OrderCategory category)
        {
            this.number = number;
            this.value = openValue;
            this.type = type;
            this.category = category;
        }

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                if (number != value)
                {
                    number = value;
                    OnPropertyChanged("Number");
                }
            }
        }

        private float value;
        public float Value
        {
            get { return value; }
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        private OrderType type;
        public OrderType Type
        {
            get { return type; }
            set { type = value; }
        }

        private OrderCategory category;
        public OrderCategory Category
        {
            get { return category; }
            set { category = value; }
        }

        #region InotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
