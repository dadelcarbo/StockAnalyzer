using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public class PortfolioSimulationViewModel : NotifyPropertyChangedBase
    {
        public PortfolioSimulationViewModel()
        {
            this.performText = "Perform";
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            this.Agent = Agents.FirstOrDefault();
            this.Duration = StockAnalyzerForm.MainFrame.BarDuration;
            this.Group = StockAnalyzerForm.MainFrame.Group;
        }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }
        public IList<StockBarDuration> Durations => StockBarDuration.Values;
        public StockBarDuration Duration { get; set; }
        public int MaxPosition
        {
            get { return StockPortfolio.SimulationPortfolio.MaxPositions; }
            set { StockPortfolio.SimulationPortfolio.MaxPositions = value; }
        }
        public void Cancel()
        {
            if (worker != null)
            {
                worker.CancelAsync();
                worker = null;
            }
        }
        public List<string> Agents => StockAgentBase.GetAgentNames();

        private string agent;
        public string Agent
        {
            get => agent;
            set
            {
                if (agent != value)
                {
                    agent = value;
                    this.Parameters = ParameterViewModel.GetParameters(this.agentType).ToList();
                    OnPropertyChanged("Parameters");
                }
            }
        }

        private Type agentType => typeof(IStockAgent).Assembly.GetType("StockAnalyzer.StockAgent.Agents." + agent + "Agent");
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

        public Action Completed { get; internal set; }

        BackgroundWorker worker = null;

        StockPortfolioAgentEngine engine;
        public void Perform()
        {
            if (worker == null)
            {
                this.Report = "Performing";
                engine = new StockPortfolioAgentEngine(agentType, this.Parameters.Select(p => new StockAgentParam(p.GetProperty(), p.Value)));

                worker = new BackgroundWorker();
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
                engine.PerformPortfolio(stockSeries, 20, this.Duration, this.MaxPosition);

                if (worker.CancellationPending)
                    return false;

                this.TradeSummary = engine.TradeSummary;

                string msg = tradeSummary.ToLog() + Environment.NewLine;
                msg += engine.ToLog() + Environment.NewLine;
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
