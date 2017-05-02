using StockAnalyzer.StockClasses;
using System;
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

        private float drawdown;
        public float Drawdown
        {
            get { return drawdown; }
            set
            {
                if (drawdown != value)
                {
                    drawdown = value;
                    OnPropertyChanged("Drawdown");
                }
            }
        }

        private float maxDrawdown;
        public float MaxDrawdown
        {
            get { return maxDrawdown; }
            set { maxDrawdown = value; }
        }

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

        private StockDailyValue currentValue;
        public StockDailyValue CurrentValue
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
            get
            {
                if (this.currentValue == null) return 0;
                return (currentValue.CLOSE - OpenValue) * this.Number;
            }
        }

        public float AddedValuePercent
        {
            get
            {
                if (this.currentValue == null) return 0;
                else return openValue == 0f ? 0.0f : Math.Sign(this.Number) * (currentValue.CLOSE - OpenValue) / OpenValue;
            }
        }

        public void Close()
        {
            this.Number = 0;
            this.OpenValue = 0;
            this.Orders.Clear();
            this.StopOrder = null;
            this.TargetOrder = null;
            this.Drawdown = 0;
            this.MaxDrawdown = Math.Max(this.MaxDrawdown, this.Drawdown);
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
            if (this.Number > 0)
            {
                this.Drawdown = Math.Max((this.openValue - currentValue.LOW) / this.openValue, this.Drawdown);
            }
            else if (this.Number < 0)
            {
                this.Drawdown = Math.Max((currentValue.HIGH - this.openValue) / this.openValue, this.Drawdown);
            }
            if (StopOrder != null)
            {
                if (StopOrder.Value * this.Number >= currentValue.CLOSE * this.Number)
                {
                    if (this.OnStopTouched != null) this.OnStopTouched(StopOrder);
                    if (StopOrder.Number == this.Number)
                    {
                        addedValue = (currentValue.CLOSE - this.openValue) * this.number;
                        this.Close();
                    }
                    else
                    {
                        addedValue = (currentValue.CLOSE - this.openValue) * StopOrder.Number;
                        this.Number -= StopOrder.Number;
                        this.Orders.Remove(this.StopOrder);
                        this.StopOrder = null;
                    }
                    return addedValue;
                }
            }
            if (TargetOrder != null)
            {
                if (TargetOrder.Value * this.Number <= currentValue.CLOSE * this.Number)
                {
                    if (this.OnTargetTouched != null) this.OnTargetTouched(TargetOrder);
                    if (TargetOrder.Number == this.Number)
                    {
                        addedValue = (currentValue.CLOSE - this.openValue) * this.number;
                        this.Close();
                    }
                    else
                    {
                        addedValue = (currentValue.CLOSE - this.openValue) * TargetOrder.Number;
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
