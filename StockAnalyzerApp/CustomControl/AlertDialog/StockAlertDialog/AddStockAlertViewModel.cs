using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    public class AddStockAlertViewModel : NotifyPropertyChangedBase
    {
        public AddStockAlertViewModel()
        {
            this.BrokenUp = true;
            this.alertType = AlertType.Group;
            this.alertDefs = StockAlertConfig.AlertConfigs.SelectMany(l => l.AlertDefs);
        }

        public StockBarDuration BarDuration { get; set; }

        public string StockName { get; set; }
        public StockSerie.Groups Group { get; set; }
        public string Theme { get; set; }
        public IList<string> Themes { get; set; }
        public IEnumerable<string> IndicatorNames { get; set; }

        #region Trigger
        private string triggerName;
        public string TriggerName
        {
            get => triggerName;
            set
            {
                if (triggerName != value)
                {
                    triggerName = value;
                    if (!string.IsNullOrEmpty(triggerName))
                    {
                        var viewableSeries = StockViewableItemsManager.GetViewableItem(this.triggerName);

                        this.TriggerEvents = (viewableSeries as IStockEvent).EventNames;
                        this.TriggerEvent = this.TriggerEvents?[0];
                    }
                    else
                    {
                        this.TriggerEvents = null;
                        this.TriggerEvent = null;
                    }
                    OnPropertyChanged("TriggerEvents");
                    OnPropertyChanged("TriggerName");
                }
            }
        }

        public string[] TriggerEvents { get; set; }

        private string triggerEvent;

        public string TriggerEvent
        {
            get { return triggerEvent; }
            set
            {
                if (value != triggerEvent)
                {
                    triggerEvent = value;
                    OnPropertyChanged("TriggerEvent");
                }
            }
        }
        #endregion

        #region Filter
        private string filterName;
        public string FilterName
        {
            get => filterName;
            set
            {
                if (filterName != value)
                {
                    filterName = value;
                    if (!string.IsNullOrEmpty(filterName))
                    {
                        var viewableSeries = StockViewableItemsManager.GetViewableItem(this.filterName);

                        this.FilterEvents = (viewableSeries as IStockEvent).EventNames;
                        this.FilterEvent = this.FilterEvents?[0];
                    }
                    else
                    {
                        this.FilterEvents = null;
                        this.FilterEvent = null;
                    }
                    OnPropertyChanged("FilterEvents");
                    OnPropertyChanged("FilterName");
                }
            }
        }

        public string[] FilterEvents { get; set; }

        private string filterEvent;

        public string FilterEvent
        {
            get { return filterEvent; }
            set
            {
                if (value != filterEvent)
                {
                    filterEvent = value;
                    OnPropertyChanged("FilterEvent");
                }
            }
        }
        #endregion

        public float Price { get; set; }
        public bool BrokenUp { get; set; }

        private IEnumerable<StockAlertDef> alertDefs;
        public IEnumerable<StockAlertDef> AlertDefs => alertDefs?.Where(a => a.Type == this.AlertType);

        private AlertType alertType;
        public AlertType AlertType
        {
            get => alertType;
            set
            {
                if (alertType != value)
                {
                    alertType = value;
                    OnPropertyChanged("AlertDefs");
                }
            }
        }

        internal void CreateAlert(AlertType alertType)
        {
            var alertDef = new StockAlertDef()
            {
                BarDuration = this.BarDuration,
                CreationDate = DateTime.Now
            };
            string[] fields;
            switch (alertType)
            {
                case AlertType.Group:
                    alertDef.Group = this.Group;
                    alertDef.BarDuration = this.BarDuration;
                    fields = this.TriggerName.Split('|');
                    alertDef.IndicatorType = fields[0];
                    alertDef.IndicatorName = fields[1];
                    alertDef.EventName = this.TriggerEvent;
                    break;
                case AlertType.Stock:
                    fields = this.TriggerName.Split('|');
                    alertDef.IndicatorType = fields[0];
                    alertDef.IndicatorName = fields[1];
                    alertDef.EventName = this.TriggerEvent;
                    alertDef.StockName = this.StockName;
                    break;
                case AlertType.Price:
                    alertDef.PriceTrigger = this.Price;
                    alertDef.TriggerBrokenUp = this.BrokenUp;
                    break;
            }
        }
    }
}
