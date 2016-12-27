using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels
{
    public class PositionViewModel : INotifyPropertyChanged
    {
        public delegate void StopTouchedHandler(OrderViewModel order);
        public event StopTouchedHandler OnStopTouched;

        public delegate void TargetTouchedHandler(OrderViewModel order);
        public event TargetTouchedHandler OnTargetTouched;

        public delegate void PositionClosedHandler();
        public event PositionClosedHandler OnPositionClosed;

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                if (number != value)
                {
                    number = value;
                    if (number == 0)
                    {
                        this.Close();
                    }
                    OnPropertyChanged("Number");
                }
            }
        }

        private float openValue;
        public float OpenValue
        {
            get { return openValue; }
            set
            {
                if (value != openValue)
                {
                    this.openValue = value;
                    OnPropertyChanged("OpenValue");
                    OnPropertyChanged("AddedValue");
                    OnPropertyChanged("AddedValuePercent");
                }
            }
        }

        private float currentValue;

        public ObservableCollection<OrderViewModel> Orders { get; set; }
        public OrderViewModel StopOrder { get; set; }
        public OrderViewModel TargetOrder { get; set; }

        public PositionViewModel()
        {
            this.Orders = new ObservableCollection<OrderViewModel>();
        }

        public void AddStop(float value)
        {
            this.StopOrder = new OrderViewModel()
            {
                Number = this.number,
                Category = OrderCategory.Stop,
                Type = this.Number > 0 ? OrderType.Long : OrderType.Short,
                Value = value
            };
            this.Orders.Add(this.StopOrder);
        }
        public void AddTarget(float value, int qty)
        {
            this.TargetOrder = new OrderViewModel()
            {
                Number = qty,
                Category = OrderCategory.Target,
                Type = this.Number > 0 ? OrderType.Long : OrderType.Short,
                Value = value
            };
            this.Orders.Add(this.TargetOrder);
        }

        public float CurrentValue
        {
            get { return currentValue; }
            set
            {
                if (value != currentValue)
                {
                    this.currentValue = value;
                    OnPropertyChanged("CurrentValue");
                    OnPropertyChanged("AddedValue");
                    OnPropertyChanged("AddedValuePercent");
                }
            }
        }

        public float AddedValue
        {
            get { return (CurrentValue - OpenValue) * this.Number; }
        }
        public float AddedValuePercent
        {
            get { return CalculateAddedValuePercent(this.currentValue); }
        }
        private float CalculateAddedValuePercent(float value)
        {
            return openValue == 0f ? 0.0f : (value - OpenValue) / OpenValue;
        }

        public void Close()
        {
            this.Number = 0;
            this.OpenValue = 0;
            this.Orders.Clear();
            this.StopOrder = null;
            this.TargetOrder = null;
            if (this.OnPositionClosed != null)
            {
                this.OnPositionClosed();
            }
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

        public float ValidatePosition()
        {
            float addedValue = 0f;
            if (StopOrder != null)
            {
                if (StopOrder.Value * this.Number >= currentValue * this.Number)
                {
                    if (this.OnStopTouched!=null) this.OnStopTouched(StopOrder);
                    if (StopOrder.Number == this.Number)
                    {
                        addedValue = (currentValue - this.openValue) * this.number;
                        this.Close();
                    }
                    else
                    {
                        addedValue = (currentValue - this.openValue) * StopOrder.Number;
                        this.Number -= StopOrder.Number;
                        this.Orders.Remove(this.StopOrder);
                        this.StopOrder = null;
                    }
                    return addedValue;
                }
            }
            if (TargetOrder != null)
            {
                if (TargetOrder.Value * this.Number <= currentValue * this.Number)
                {
                    if (this.OnTargetTouched!=null) this.OnTargetTouched(TargetOrder);
                    if (TargetOrder.Number == this.Number)
                    {
                        addedValue = (currentValue - this.openValue) * this.number;
                        this.Close();
                    }
                    else
                    {
                        addedValue = (currentValue - this.openValue) * TargetOrder.Number;
                        this.Number -= TargetOrder.Number;
                        this.Orders.Remove(this.TargetOrder);
                        this.TargetOrder = null;
                    }
                    return addedValue;
                }
            }
            return 0.0f;
        }
    }
    #endregion
}
