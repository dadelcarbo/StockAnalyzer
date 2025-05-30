﻿using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockScripting;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    public class SelectedAlertDef : NotifyPropertyChangedBase
    {
        public StockAlertDef AlertDef { get; set; }

        private bool isSelected;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }

    public class StockAlertManagerViewModel : NotifyPropertyChangedBase
    {
        public StockAlertManagerViewModel()
        {
            this.InReport = true;
            this.InAlert = true;
            this.CompleteBar = false;
            this.BrokenUp = true;
            this.alertType = AlertType.Group;
            this.allAlertDefs = StockAlertDef.AlertDefs;
            this.SelectedAlerts = StockAlertDef.AlertDefs.Select(a => new SelectedAlertDef { AlertDef = a, IsSelected = a.InReport }).ToList();
            this.Themes = StockAnalyzerForm.MainFrame.Themes.Append(string.Empty);
            this.Theme = StockAnalyzerForm.MainFrame.CurrentTheme;
            if (this.Theme.Contains("*"))
                this.Theme = this.Themes.FirstOrDefault();

            RunAlertEnabled = true;
            this.ProgressVisibility = Visibility.Collapsed;
        }

        internal void Init(StockAlertDef alertDef)
        {
            this.AlertId = alertDef.Id;
            this.InReport = alertDef.InReport;
            this.InAlert = alertDef.InAlert;
            this.CompleteBar = alertDef.CompleteBar;
            this.BarDuration = alertDef.BarDuration;
            this.Theme = alertDef.Theme;
            this.Stop = alertDef.Stop;
            this.Speed = alertDef.Speed;
            this.Script = string.IsNullOrEmpty(alertDef.Script) ? StockScript.Empty : StockScriptManager.Instance.StockScripts.FirstOrDefault(s => s.Name == alertDef.Script);
            this.Stok = alertDef.Stok;
            this.MinLiquidity = alertDef.MinLiquidity;
            switch (this.alertType)
            {
                case AlertType.Group:
                    this.Title = alertDef.Title;
                    this.Group = alertDef.Group;
                    this.TriggerName = alertDef.IndicatorFullName;
                    this.TriggerEvent = alertDef.EventName;
                    this.FilterName = alertDef.FilterFullName;
                    this.FilterEvent = alertDef.FilterEventName;
                    this.FilterDuration = alertDef.FilterDuration;
                    break;
                case AlertType.Stock:
                    this.StockName = alertDef.StockName;
                    this.TriggerName = alertDef.IndicatorFullName;
                    this.TriggerEvent = alertDef.EventName;
                    this.FilterName = alertDef.FilterFullName;
                    this.FilterEvent = alertDef.FilterEventName;
                    this.FilterDuration = alertDef.FilterDuration;
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

        private bool inReport;
        public bool InReport { get => inReport; set => SetProperty(ref inReport, value); }

        private bool inAlert;
        public bool InAlert { get => inAlert; set => SetProperty(ref inAlert, value); }

        private bool completeBar;
        public bool CompleteBar { get => completeBar; set => SetProperty(ref completeBar, value); }

        private string title;
        public string Title { get => title; set => SetProperty(ref title, value); }

        private BarDuration barDuration;
        public BarDuration BarDuration { get => barDuration; set => SetProperty(ref barDuration, value); }

        private string stockName;
        public string StockName { get => stockName; set => SetProperty(ref stockName, value); }

        private StockSerie.Groups group;
        public StockSerie.Groups Group { get => group; set => SetProperty(ref group, value); }

        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));

        private string theme;
        public string Theme { get => theme; set => SetProperty(ref theme, value); }


        public IEnumerable<string> Themes { get; set; }
        public IEnumerable<string> IndicatorNames { get; set; }
        public IEnumerable<string> StopNames => IndicatorNames.Where(i => i.StartsWith("TRAILSTOP|")).Select(i => i.Replace("TRAILSTOP|", ""));

        private string stop = "ROR(35)";
        public string Stop { get => stop; set => SetProperty(ref stop, value); }

        private string speed;
        public string Speed { get => speed; set => SetProperty(ref speed, value); }

        private StockScript script;
        public StockScript Script { get => script; set => SetProperty(ref script, value); }
        public IEnumerable<StockScript> Screeners => new List<StockScript>() { StockScript.Empty }.Union(StockScriptManager.Instance.StockScripts);

        private int stok;
        public int Stok { get => stok; set => SetProperty(ref stok, value); }

        private float minLiquidity;
        public float MinLiquidity { get => minLiquidity; set => SetProperty(ref minLiquidity, value); }

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
                            OnPropertyChanged("StopNames");
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
                            OnPropertyChanged("StopNames");
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

        private BarDuration filterDuration;
        public BarDuration FilterDuration { get => filterDuration; set => SetProperty(ref filterDuration, value); }
        #endregion

        public float Price { get; set; }
        public bool BrokenUp { get; set; }

        private readonly List<StockAlertDef> allAlertDefs;
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

        public ObservableCollection<StockAlertValue> Alerts { get; set; } = new ObservableCollection<StockAlertValue>();

        public IEnumerable<SelectedAlertDef> SelectedAlerts { get; set; }

        private int alertId = -1;
        public int AlertId
        {
            get => alertId;
            set { SetProperty(ref alertId, value); this.IsDeleteEnabled = this.alertId != -1; }
        }

        #region Add Alert Command
        private CommandBase addAlertCommand;

        public ICommand AddAlertCommand
        {
            get
            {
                addAlertCommand ??= new CommandBase(AddAlert);

                return addAlertCommand;
            }
        }

        private void AddAlert()
        {
            var alertDef = allAlertDefs.FirstOrDefault(a => a.Id == alertId);
            if (alertDef == null)
            {
                this.AlertId = allAlertDefs.Count() == 0 ? 1 : allAlertDefs.Max(a => a.Id) + 1;
                alertDef = new StockAlertDef()
                {
                    Id = this.alertId,
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
                    alertDef.FilterDuration = filterDuration;
                    break;
                case AlertType.Stock:
                    alertDef.StockName = this.StockName;
                    alertDef.IndicatorType = string.IsNullOrEmpty(triggerName) ? null : triggerName.Split('|')[0];
                    alertDef.IndicatorName = string.IsNullOrEmpty(triggerName) ? null : triggerName.Split('|')[1];
                    alertDef.EventName = triggerEvent;
                    alertDef.FilterType = string.IsNullOrEmpty(filterName) ? null : filterName.Split('|')[0];
                    alertDef.FilterName = string.IsNullOrEmpty(filterName) ? null : filterName.Split('|')[1];
                    alertDef.FilterEventName = filterEvent;
                    alertDef.FilterDuration = filterDuration;
                    break;
                case AlertType.Price:
                    alertDef.StockName = this.StockName;
                    alertDef.PriceTrigger = Price;
                    alertDef.TriggerBrokenUp = BrokenUp;
                    break;
                default:
                    break;
            }
            alertDef.MinLiquidity = this.MinLiquidity;
            alertDef.InReport = this.InReport;
            alertDef.InAlert = this.InAlert;
            alertDef.CompleteBar = this.CompleteBar;
            alertDef.BarDuration = this.BarDuration;
            alertDef.Theme = this.Theme;
            alertDef.CreationDate = DateTime.Now;
            alertDef.Stop = this.Stop;
            alertDef.Speed = this.Speed;
            alertDef.Script = this.Script?.Name;
            alertDef.Stok = this.Stok;

            this.OnPropertyChanged("AlertDefs");

            StockAlertDef.Save();
            var fileName = Path.Combine(Folders.Report, "LastGeneration.txt");
            if (File.Exists(fileName))
                File.Delete(fileName);

            this.SelectedAlerts = StockAlertDef.AlertDefs.Select(a => new SelectedAlertDef { AlertDef = a }).ToList();
            this.OnPropertyChanged("SelectedAlerts");
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
                deleteAlertCommand ??= new CommandBase(DeleteAlert);

                return deleteAlertCommand;
            }
        }

        private void DeleteAlert()
        {
            if (AlertId < 0)
                return;

            allAlertDefs.RemoveAll(a => a.Id == AlertId);
            this.OnPropertyChanged("AlertDefs");
            this.Clear();

            StockAlertDef.Save();
            var fileName = Path.Combine(Folders.Report, "LastGeneration.txt");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
        #endregion
        #region NEW ALERT COMMAND

        private CommandBase newAlertCommand;

        public ICommand NewAlertCommand
        {
            get
            {
                newAlertCommand ??= new CommandBase(NewAlert);

                return newAlertCommand;
            }
        }

        private void NewAlert()
        {
            this.Clear();
        }
        #endregion
        #region Run ALERT COMMAND

        private CommandBase runAlertCommand;

        public ICommand RunAlertCommand
        {
            get
            {
                runAlertCommand ??= new CommandBase(RunAlert);

                return runAlertCommand;
            }
        }

        public void RunAlert()
        {
            using var ml = new MethodLogger(this, true);
            Task.Run(() =>
            {
                try
                {
                    RunAlertEnabled = false;
                    this.Alerts.Clear();

                    var alertDefs = this.SelectedAlerts.Where(s => s.IsSelected).Select(s => s.AlertDef).ToList();

                    this.ProgressVisibility = Visibility.Visible;
                    this.ProgressValue = 0;
                    this.ProgressMax = alertDefs.Count;

                    foreach (var alertDef in alertDefs)
                    {
                        CurrentAlert = alertDef;
                        foreach (var alert in StockDictionary.Instance.MatchAlert(alertDef))
                        {
                            this.Alerts.Add(alert.GetAlertValue());
                        }
                        this.ProgressValue++;
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }

                RunAlertEnabled = true;
                CurrentAlert = null;
                this.ProgressValue = 0;
                this.ProgressVisibility = Visibility.Collapsed;
            });
        }
        public void RunFullAlert()
        {
            using var ml = new MethodLogger(this, true);
            RunAlertEnabled = false;
            Task.Run(() =>
            {
                try
                {
                    RunAlertEnabled = false;
                    this.Alerts.Clear();

                    var alertDefs = this.AlertDefs.Where(s => s.InReport || s.InAlert).ToList();

                    this.ProgressVisibility = Visibility.Visible;
                    this.ProgressValue = 0;
                    this.ProgressMax = alertDefs.Count;
                    foreach (var alertDef in alertDefs)
                    {
                        CurrentAlert = alertDef;
                        foreach (var alert in StockDictionary.Instance.MatchAlert(alertDef))
                        {
                            this.Alerts.Add(alert.GetAlertValue());
                        }
                        this.ProgressValue++;
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }

                RunAlertEnabled = true;
                CurrentAlert = null;
                this.ProgressValue = 0;
                this.ProgressVisibility = Visibility.Collapsed;

                var cac40 = StockDictionary.Instance["CAC40"];
                cac40.Initialise();
                File.WriteAllText(Path.Combine(Folders.Report, "LastGeneration.txt"), cac40.LastValue.DATE.ToString(CultureInfo.InvariantCulture));
            });
        }
        #endregion
        #region Select All COMMAND

        private ParamCommandBase<string> selectAllCommand;

        public ICommand SelectAllCommand
        {
            get
            {
                selectAllCommand ??= new ParamCommandBase<string>(SelectAll);
                return selectAllCommand;
            }
        }

        public void SelectAll(string selectParam)
        {
            switch (selectParam)
            {
                case "SelectAll":
                    foreach (var item in this.SelectedAlerts)
                    {
                        item.IsSelected = true;
                    }
                    break;
                case "UnselectAll":
                    foreach (var item in this.SelectedAlerts)
                    {
                        item.IsSelected = false;
                    }
                    break;
                default:
                    foreach (var item in this.SelectedAlerts)
                    {
                        item.IsSelected = item.AlertDef.BarDuration.ToString() == selectParam;
                    }
                    break;
            }
        }
        #endregion

        private void Clear()
        {
            this.AlertId = -1;
            this.InReport = true;
            this.InAlert = true;
            this.CompleteBar = false;
            this.Theme = StockAnalyzerForm.MainFrame.CurrentTheme;
            if (this.Theme.Contains("*"))
                this.Theme = this.Themes.FirstOrDefault();
            this.TriggerEvent = null;
            this.FilterEvent = null;
            this.FilterDuration = BarDuration.Daily;
        }

        private Visibility runAlertVisibility;
        public Visibility RunAlertVisibility { get => runAlertVisibility; }

        private bool runAlertEnabled;
        public bool RunAlertEnabled
        {
            get => runAlertEnabled;
            set
            {
                if (value != runAlertEnabled)
                {
                    SetProperty(ref runAlertVisibility, value ? Visibility.Visible : Visibility.Collapsed, "RunAlertVisibility");
                    SetProperty(ref runAlertEnabled, value);
                }
            }
        }

        private StockAlertDef currentAlert;
        public StockAlertDef CurrentAlert { get => currentAlert; set => SetProperty(ref currentAlert, value); }

        private ICommand generateReport;
        public ICommand GenerateReport => generateReport ??= new CommandBase(PerformGenerateReport);

        private void PerformGenerateReport()
        {
            StockAnalyzerForm.MainFrame.GenerateReport(this.BarDuration);
        }

        private int progressMax;
        public int ProgressMax { get => progressMax; set => SetProperty(ref progressMax, value); }

        private int progressValue;
        public int ProgressValue { get => progressValue; set => SetProperty(ref progressValue, value); }

        private Visibility progressVisibility;
        public Visibility ProgressVisibility { get => progressVisibility; set => SetProperty(ref progressVisibility, value); }
    }
}
