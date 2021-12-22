using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockAgent.Agents;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public class PortfolioSimulationViewModel : NotifyPropertyChangedBase
    {
        public PortfolioSimulationViewModel()
        {
            this.performText = "Perform";
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            this.Duration = StockAnalyzerForm.MainFrame.BarDuration;
            this.Group = StockAnalyzerForm.MainFrame.Group;
            this.PositionManagement = new PositionManagement() { MaxPositions = 10, PortfolioInitialBalance = 10000, PortfolioRisk = 1, StopATR = 2, Rank = "ROC(50)" };
            this.EntryType = "Indicator";
            this.EntryIndicator = "EMA(30)";
            this.EntryEvent = "CrossAbove";

            this.ExitType = "Indicator";
            this.ExitIndicator = "EMA(30)";
            this.ExitEvent = "CrossBelow";

            this.FilterType = "Indicator";
            this.FilterIndicator = "EMA(200)";
            this.FilterEvent = "PriceAbove";
        }

        public PositionManagement PositionManagement { get; set; }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }
        public IList<StockBarDuration> Durations => StockBarDuration.Values;
        public StockBarDuration Duration { get; set; }

        public static List<string> IndicatorTypes => StockViewableItemsManager.IndicatorTypes;

        private string entryType;
        public string EntryType { get { return entryType; } set { if (value != entryType) { entryType = value; this.EntryIndicator = null; OnPropertyChanged("EntryType"); } } }
        private string entryIndicator;
        public string EntryIndicator
        {
            get { return entryIndicator; }
            set
            {
                if (value != entryIndicator)
                {
                    entryIndicator = value;
                    IStockViewableSeries viewableSeries = StockViewableItemsManager.GetViewableItem(this.entryType.ToUpper() + "|" + this.entryIndicator);
                    if (viewableSeries != null)
                    {
                        this.EntryEvents = (viewableSeries as IStockEvent).EventNames;
                        this.EntryEvent = this.EntryEvents.FirstOrDefault();
                    }
                    else
                    {
                        this.EntryEvents = null;
                        this.EntryEvent = null;
                    }
                    OnPropertyChanged("EntryIndicator");
                }
            }
        }
        private string entryEvent;
        public string EntryEvent { get { return entryEvent; } set { if (value != entryEvent) { entryEvent = value; OnPropertyChanged("EntryEvent"); } } }
        private IEnumerable<string> entryEvents;
        public IEnumerable<string> EntryEvents { get { return entryEvents; } set { if (value != entryEvents) { entryEvents = value; OnPropertyChanged("EntryEvents"); } } }

        private string filterType;
        public string FilterType { get { return filterType; } set { if (value != filterType) { filterType = value; this.FilterIndicator = null; OnPropertyChanged("FilterType"); } } }
        private string filterIndicator;
        public string FilterIndicator
        {
            get { return filterIndicator; }
            set
            {
                if (value != filterIndicator)
                {
                    filterIndicator = value;
                    IStockViewableSeries viewableSeries = StockViewableItemsManager.GetViewableItem(this.filterType.ToUpper() + "|" + this.filterIndicator);
                    if (viewableSeries != null)
                    {
                        this.FilterEvents = (viewableSeries as IStockEvent).EventNames;
                        this.FilterEvent = this.FilterEvents.FirstOrDefault();
                    }
                    else
                    {
                        this.FilterEvents = null;
                        this.FilterEvent = null;
                    }
                    OnPropertyChanged("FilterIndicator");
                }
            }
        }
        private string filterEvent;
        public string FilterEvent { get { return filterEvent; } set { if (value != filterEvent) { filterEvent = value; OnPropertyChanged("FilterEvent"); } } }
        private IEnumerable<string> filterEvents;
        public IEnumerable<string> FilterEvents { get { return filterEvents; } set { if (value != filterEvents) { filterEvents = value; OnPropertyChanged("FilterEvents"); } } }

        private string exitType;
        public string ExitType { get { return exitType; } set { if (value != exitType) { exitType = value; this.ExitIndicator = null; OnPropertyChanged("ExitType"); } } }
        private string exitIndicator;
        public string ExitIndicator
        {
            get { return exitIndicator; }
            set
            {
                if (value != exitIndicator)
                {
                    exitIndicator = value;
                    IStockViewableSeries viewableSeries = StockViewableItemsManager.GetViewableItem(this.exitType.ToUpper() + "|" + this.exitIndicator);
                    if (viewableSeries != null)
                    {
                        this.ExitEvents = (viewableSeries as IStockEvent).EventNames;
                        this.ExitEvent = this.ExitEvents.FirstOrDefault();
                    }
                    else
                    {
                        this.ExitEvents = null;
                        this.ExitEvent = null;
                    }
                    OnPropertyChanged("ExitIndicator");
                }
            }
        }
        private string exitEvent;
        public string ExitEvent { get { return exitEvent; } set { if (value != exitEvent) { exitEvent = value; OnPropertyChanged("ExitEvent"); } } }
        private IEnumerable<string> exitEvents;
        public IEnumerable<string> ExitEvents { get { return exitEvents; } set { if (value != exitEvents) { exitEvents = value; OnPropertyChanged("ExitEvents"); } } }

        private string regimeIndice;
        public string RegimeIndice { get { return regimeIndice; } set { if (value != regimeIndice) { regimeIndice = value; OnPropertyChanged("RegimeIndice"); } } }

        private string regimePeriod;
        public string RegimePeriod { get { return regimePeriod; } set { if (value != regimePeriod) { regimePeriod = value; OnPropertyChanged("RegimePeriod"); } } }

        public void Cancel()
        {
            if (worker != null)
            {
                worker.CancelAsync();
                worker = null;
            }
        }
        public List<string> Agents => StockAgentBase.GetAgentNames();

        private Type agentType => typeof(IStockAgent).Assembly.GetType("StockAnalyzer.StockAgent.Agents.PortfolioAgent");
        public IEnumerable<ParameterViewModel> Parameters { get; private set; }

        string report;
        public string Report
        {
            get => report;
            set
            {
                if (report != value)
                {
                    report = value;
                    OnPropertyChanged("Report");
                }
            }
        }

        string log;
        public string Log
        {
            get => log;
            set
            {
                if (log != value)
                {
                    log = value;
                    OnPropertyChanged("Log");
                }
            }
        }

        EquityValue[] equityCurve;
        public EquityValue[] EquityCurve
        {
            get => equityCurve;
            set
            {
                if (equityCurve != value)
                {
                    equityCurve = value;
                    OnPropertyChanged("EquityCurve");
                }
            }
        }

        private StockTradeSummary tradeSummary;
        public StockTradeSummary TradeSummary
        {
            get => tradeSummary;
            set
            {
                if (tradeSummary != value)
                {
                    tradeSummary = value;
                    OnPropertyChanged("TradeSummary");
                }
            }
        }

        int progressValue;
        public int ProgressValue
        {
            get => progressValue;
            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    OnPropertyChanged("ProgressValue");
                }
            }
        }

        private string performText;
        public string PerformText
        {
            get { return performText; }
            set
            {
                if (performText != value)
                {
                    performText = value;
                    OnPropertyChanged("PerformText");
                }
            }
        }

        public string DisplayIndicator { get; internal set; }

        BackgroundWorker worker = null;

        StockPortfolioAgentEngine engine;
        public void Perform()
        {
            if (worker == null)
            {
                this.Report = "Performing";

                worker = new BackgroundWorker();
                engine = new StockPortfolioAgentEngine();

                engine.Worker = worker;
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = true;
                worker.DoWork += RunAgentEngineOnGroup;
                worker.RunWorkerCompleted += (a, e) =>
                {
                    StockAnalyzerForm.TimerSuspended = false;
                    this.PerformText = "Perform";
                    this.ProgressValue = 0;
                    if (e.Cancelled)
                    {
                        this.Report += Environment.NewLine + "Cancelled...";
                    }
                    this.worker = null;
                    this.EquityCurve = engine.EquityCurve;
                };

                this.PerformText = "Cancel";
                StockAnalyzerForm.TimerSuspended = true;
                worker.RunWorkerAsync();
            }
            else
            {
                worker.CancelAsync();
            }
        }
        private void RunAgentEngineOnGroup(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = StockAnalyzerForm.EnglishCulture;
            Thread.CurrentThread.CurrentCulture = StockAnalyzerForm.EnglishCulture;
            engine.ProgressChanged += (s, evt) =>
            {
                this.ProgressValue = evt.ProgressPercentage;
            };

            if (this.RunAgentEngine(StockAnalyzerForm.MainFrame.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.Group) && s.Initialise())))
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
        private bool RunAgentEngine(IEnumerable<StockSerie> stockSeries)
        {
            try
            {
                var agents = new List<IStockPortfolioAgent>();
                foreach (var serie in stockSeries)
                {
                    var agent = new PortfolioAgent()
                    {
                        EntryType = this.EntryType,
                        EntryIndicator = this.EntryIndicator,
                        EntryEvent = this.EntryEvent,
                        FilterType = this.FilterType,
                        FilterIndicator = this.FilterIndicator,
                        FilterEvent = this.FilterEvent,
                        ExitType = this.ExitType,
                        ExitIndicator = this.ExitIndicator,
                        ExitEvent = this.ExitEvent,
                        RankIndicator = this.PositionManagement.Rank
                    };
                    if (agent.Initialize(serie, this.Duration, this.PositionManagement.StopATR))
                    {
                        agents.Add(agent);
                    }
                }
                engine.PerformPortfolio(agents, 20, this.Duration, this.PositionManagement);

                if (worker.CancellationPending)
                    return false;

                this.TradeSummary = engine.TradeSummary;

                string msg = tradeSummary.ToLog(this.Duration) + Environment.NewLine;
                msg += engine.ToLog(this.Duration) + Environment.NewLine;
                msg += "NB Series: " + stockSeries.Count() + Environment.NewLine;
                msg += Environment.NewLine + "Opened position: " + Environment.NewLine;

                this.Report = msg;
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
                this.Report = ex.Message;
            }
            return true;
        }
    }
}
