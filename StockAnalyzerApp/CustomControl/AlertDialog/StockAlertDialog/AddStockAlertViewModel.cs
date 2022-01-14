using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockViewableItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    public class AddStockAlertViewModel : NotifyPropertyChangedBase
    {
        public AddStockAlertViewModel()
        {
            this.Active = true;
            this.BrokenUp = true;
            this.alertType = AlertType.Group;
            this.allAlertDefs = StockAlertConfig.AllAlertDefs;
            this.Themes = StockAnalyzerForm.MainFrame.Themes.Append(string.Empty);
            this.Theme = StockAnalyzerForm.MainFrame.CurrentTheme;
            if (this.Theme.Contains("*"))
                this.Theme = this.Themes.FirstOrDefault();
        }

        internal void Init(StockAlertDef alertDef)
        {
            this.AlertId = alertDef.Id;
            this.Active = alertDef.Active;
            this.BarDuration = alertDef.BarDuration;
            this.Theme = alertDef.Theme;
            switch (this.alertType)
            {
                case AlertType.Group:
                    this.Title = alertDef.Title;
                    this.Group = alertDef.Group;
                    this.TriggerName = alertDef.IndicatorFullName;
                    this.TriggerEvent = alertDef.EventName;
                    this.FilterName = alertDef.FilterFullName;
                    this.FilterEvent = alertDef.FilterEventName;
                    break;
                case AlertType.Stock:
                    this.StockName = alertDef.StockName;
                    this.TriggerName = alertDef.IndicatorFullName;
                    this.TriggerEvent = alertDef.EventName;
                    this.FilterName = alertDef.FilterFullName;
                    this.FilterEvent = alertDef.FilterEventName;
                    break;
                case AlertType.Price:
                    this.StockName = alertDef.StockName;
                    this.Price = alertDef.PriceTrigger;
                    this.BrokenUp = alertDef.TriggerBrokenUp;
                    break;
                default:
                    break;
            }
        }

        private bool active;
        public bool Active { get => active; set => SetProperty(ref active, value); }

        private string title;
        public string Title { get => title; set => SetProperty(ref title, value); }

        private StockBarDuration barDuration;
        public StockBarDuration BarDuration { get => barDuration; set => SetProperty(ref barDuration, value); }

        private string stockName;
        public string StockName { get => stockName; set => SetProperty(ref stockName, value); }

        private StockSerie.Groups group;
        public StockSerie.Groups Group { get => group; set => SetProperty(ref group, value); }

        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));

        private string theme;
        public string Theme { get => theme; set => SetProperty(ref theme, value); }


        public IEnumerable<string> Themes { get; set; }
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
                        if (!IndicatorNames.Contains(value))
                        {
                            IndicatorNames = IndicatorNames.Prepend(value);
                            OnPropertyChanged("IndicatorNames");
                        }
                        var viewableSeries = StockViewableItemsManager.GetViewableItem(this.triggerName);

                        this.TriggerEvents = (viewableSeries as IStockEvent)?.EventNames;
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
                        if (!IndicatorNames.Contains(value))
                        {
                            IndicatorNames = IndicatorNames.Prepend(value);
                            OnPropertyChanged("IndicatorNames");
                        }
                        var viewableSeries = StockViewableItemsManager.GetViewableItem(this.filterName);

                        this.FilterEvents = (viewableSeries as IStockEvent)?.EventNames;
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

        private List<StockAlertDef> allAlertDefs;
        public IEnumerable<StockAlertDef> AlertDefs => allAlertDefs?.Where(a => a.Type == this.AlertType);

        private AlertType alertType;
        public AlertType AlertType
        {
            get => alertType;
            set
            {
                if (alertType != value)
                {
                    AlertId = -1;
                    alertType = value;
                    OnPropertyChanged("AlertDefs");
                }
            }
        }

        private int alertId2 = -1;
        private int AlertId
        {
            get => alertId2;
            set { this.alertId2 = value; this.IsDeleteEnabled = this.alertId2 != -1; }
        }

        #region Add Alert Command
        private CommandBase addAlertCommand;

        public ICommand AddAlertCommand
        {
            get
            {
                if (addAlertCommand == null)
                {
                    addAlertCommand = new CommandBase(AddAlert);
                }

                return addAlertCommand;
            }
        }

        private void AddAlert()
        {
            var alertDef = allAlertDefs.FirstOrDefault(a => a.Id == alertId2);
            if (alertDef == null)
            {
                alertDef = new StockAlertDef()
                {
                    Id = allAlertDefs.Max(a => a.Id) + 1,
                    Type = this.alertType
                };
                this.allAlertDefs.Insert(0, alertDef);
            }
            else
            {
                if (alertDef.Type != alertType)
                {
                    MessageBox.Show("Invalid alert type:" + alertDef.Type);
                    return;
                }
            }
            switch (this.alertType)
            {
                case AlertType.Group:
                    alertDef.Title = this.Title;
                    alertDef.Group = this.Group;
                    alertDef.IndicatorType = string.IsNullOrEmpty(triggerName) ? null : triggerName.Split('|')[0];
                    alertDef.IndicatorName = string.IsNullOrEmpty(triggerName) ? null : triggerName.Split('|')[1];
                    alertDef.EventName = triggerEvent;
                    alertDef.FilterType = string.IsNullOrEmpty(filterName) ? null : filterName.Split('|')[0];
                    alertDef.FilterName = string.IsNullOrEmpty(filterName) ? null : filterName.Split('|')[1];
                    alertDef.FilterEventName = filterEvent;
                    break;
                case AlertType.Stock:
                    alertDef.StockName = this.StockName;
                    alertDef.IndicatorType = string.IsNullOrEmpty(triggerName) ? null : triggerName.Split('|')[0];
                    alertDef.IndicatorName = string.IsNullOrEmpty(triggerName) ? null : triggerName.Split('|')[1];
                    alertDef.EventName = triggerEvent;
                    alertDef.FilterType = string.IsNullOrEmpty(filterName) ? null : filterName.Split('|')[0];
                    alertDef.FilterName = string.IsNullOrEmpty(filterName) ? null : filterName.Split('|')[1];
                    alertDef.FilterEventName = filterEvent;
                    break;
                case AlertType.Price:
                    alertDef.StockName = this.StockName;
                    alertDef.PriceTrigger = Price;
                    alertDef.TriggerBrokenUp = BrokenUp;
                    break;
                default:
                    break;
            }
            alertDef.Active = this.Active;
            alertDef.BarDuration = this.BarDuration;
            alertDef.Theme = this.Theme;
            alertDef.CreationDate = DateTime.Now;

            this.OnPropertyChanged("AlertDefs");
            this.AlertId = -1;
        }

        #endregion

        #region Delete Alert Command

        private bool isDeleteEnabled;
        public bool IsDeleteEnabled { get => isDeleteEnabled; set => SetProperty(ref isDeleteEnabled, value); }

        private CommandBase deleteAlertCommand;

        public ICommand DeleteAlertCommand
        {
            get
            {
                if (deleteAlertCommand == null)
                {
                    deleteAlertCommand = new CommandBase(DeleteAlert);
                }

                return deleteAlertCommand;
            }
        }

        private void DeleteAlert()
        {
            if (AlertId < 0)
                return;
            var alertDef = allAlertDefs.RemoveAll(a => a.Id == AlertId);
            this.OnPropertyChanged("AlertDefs");
            this.AlertId = -1;
        }


        #endregion
    }
}
