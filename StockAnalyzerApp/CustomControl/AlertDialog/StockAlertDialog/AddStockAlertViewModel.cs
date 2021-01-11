using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    public class AddStockAlertViewModel : NotifyPropertyChangedBase
    {
        private string indicatorName;

        public AddStockAlertViewModel()
        {
            this.BrokenUp = true;
        }

        public string Group { get; set; }
        public string StockName { get; set; }
        public StockBarDuration BarDuration { get; set; }

        public string IndicatorName
        {
            get => indicatorName;
            set
            {
                if (indicatorName != value)
                {
                    indicatorName = value;
                    var viewableSeries = StockViewableItemsManager.GetViewableItem(this.indicatorName);

                    this.Events = (viewableSeries as IStockEvent).EventNames;
                    this.Event = this.Events?[0];

                    OnPropertyChanged("Events");
                    OnPropertyChanged("IndicatorName");
                }
            }
        }
        public IList<string> IndicatorNames { get; set; }

        public string[] Events { get; set; }

        private string eventName;

        public string Event
        {
            get { return eventName; }
            set
            {
                if (value != eventName)
                {
                    eventName = value;
                    OnPropertyChanged("Event");
                }
            }
        }

        public float Price { get; set; }
        public bool BrokenUp { get; set; }

    }
}
