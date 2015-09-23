using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using StockAnalyzer.StockClasses;
using StockAnalyzerSettings.Properties;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
    public class RiskCalculatorViewModel : INotifyPropertyChanged
    {
        public RiskCalculatorViewModel()
        {
            buy = -1;
            amount = 999;
            this.Period = 6;
        }

        private StockSerie stockSerie;

        public int Period { get; set; }

        public StockSerie StockSerie
        {
            get { return stockSerie; }
            set
            {
                stockSerie = value;
                buy = this.stockSerie.Values.Last().CLOSE;

                IStockIndicator trailHRSR = stockSerie.GetIndicator("TRAILHLSR(" + Period + ")");
                FloatSerie supportSerie = trailHRSR.Series[0];
                FloatSerie resistanceSerie = trailHRSR.Series[1];

                stop = supportSerie.LastNonNaN;
                target1 = resistanceSerie.LastNonNaN;
                target2 = target1 + target1 - buy;
                
                NotifyAllChanged();
            }
        }

        public string StockName { get { return stockSerie.StockName; }}

        public int Qty
        {
            get { return (int)Math.Floor(Amount / Buy); }
        }

        public float PortofolioValue { get { return Settings.Default.PortofolioValue; } set { Settings.Default.PortofolioValue = value; NotifyAllChanged(); } }

        private float buy;
        public float Buy
        {
            get { return buy; }
            set
            {
                buy = value;
                NotifyAllChanged();
            }
        }

        private float stop;
        public float Stop { get { return stop; } set { stop = value; NotifyAllChanged(); } }

        private float target1;
        public float Target1 { get { return target1; } set { target1 = value; NotifyAllChanged(); } }

        private float target2;
        public float Target2 { get { return target2; } set { target2 = value; NotifyAllChanged(); } }

        private float amount;
        public float Amount { get { return amount; } set { amount = value; NotifyAllChanged(); } }

        public float Fee
        {
            get
            {
                if (Amount < 1000) return 2.5f;
                else return 5.0f;
            }
        }

        public float StopValue {get { return Qty*(stop - buy) - 2*Fee; }}

        public float StockRisk {
            get { return StopValue/(buy*Qty); }
        }
        public float PortfolioRisk { get { return StopValue / PortofolioValue; } }


        public float StockGain1 { get { return Qty * (target1 - buy) - 2*Fee; } }

        public float StockReturn1
        {
            get { return StockGain1 / (buy * Qty); }
        }

        public float PortfolioReturn1 { get { return StockGain1 / PortofolioValue; } }

        public float StockGain2 { get { return Qty * (target2 - buy) - 2*Fee; } }

        public float StockReturn2
        {
            get { return StockGain2 / (buy * Qty); }
        }

        public float PortfolioReturn2 { get { return StockGain2 / PortofolioValue; } }
    

        public void NotifyAllChanged()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            if (this.PropertyChanged != null)
            {
                foreach (PropertyInfo property in properties)
                {
                    this.OnPropertyChanged(property.Name);
                }
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
