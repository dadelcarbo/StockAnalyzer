using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public class AgentSimulationViewModel : NotifyPropertyChangedBase
    {
        public AgentSimulationViewModel()
        {
            this.performText = "Perform";
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            agent = Agents.FirstOrDefault();
            this.Duration = StockAnalyzerForm.MainFrame.BarDuration;
            this.Group = StockAnalyzerForm.MainFrame.Group;
            this.Accuracy = 20;
            this.Selector = "Coumpound";
        }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }
        public IList<StockBarDuration> Durations => StockBarDuration.Values;
        public StockBarDuration Duration { get; set; }

        public int Accuracy { get; set; }

        public List<string> Selectors => new List<string> { "Coumpound", "Average", "CumulGain", "WinRatio" };
        public string Selector { get; set; }

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
                    OnPropertyChanged("Parameters");
                }
            }
        }

        private Type agentType => typeof(IStockAgent).Assembly.GetType("StockAnalyzer.StockAgent.Agents." + agent + "Agent");
        public IEnumerable Parameters => ParameterViewModel.GetParameters(this.agentType);

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

        StockAgentEngine engine;
        public void Perform()
        {
            if (worker == null)
            {
                this.Report = "Performing";
                engine = new StockAgentEngine(agentType);
                worker = new BackgroundWorker();
                engine.Worker = worker;
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = true;
                worker.DoWork += RunAgentEngineOnGroup;
                worker.RunWorkerCompleted += (a, e) =>
                {
                    this.PerformText = "Perform";
                    this.ProgressValue = 0;
                    if (e.Cancelled)
                    {
                        this.Report = "Cancelled...";
                    }
                    else
                    {
                        this.Report = engine.Report;
                        string rpt = this.Group + "\t" + this.Duration + "\t" + this.Agent + "\t";
                        rpt += engine.BestTradeSummary.ToStats();
                        rpt += engine.BestAgent.GetParameterValues();
                        Clipboard.SetText(rpt);

                        this?.Completed();
                    }
                    this.worker = null;
                };

                this.PerformText = "Cancel";
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
                Func<StockTradeSummary, float> selector;
                switch (this.Selector)
                {
                    case "Coumpound":
                        selector = t => t.CompoundGain;
                        break;
                    case "Average":
                        selector = t => t.AvgGain;
                        break;
                    case "CumulGain":
                        selector = t => t.CumulGain;
                        break;
                    case "WinRatio":
                        selector = t => t.WinRatio;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid selector: " + this.Selector);
                }

                foreach (var serie in stockSeries)
                {
                    serie.BarDuration = this.Duration;
                }

                engine.GreedySelection(stockSeries, 20, this.Accuracy, selector);
                if (engine.BestTradeSummary == null)
                    return false;

                StockAnalyzerForm.MainFrame.BinckPortfolio = new StockPortfolio();
                foreach (var trade in engine.BestTradeSummary.Trades)
                {
                    // Create operations
                    StockAnalyzerForm.MainFrame.BinckPortfolio.AddOperation(StockOperation.FromSimu(trade.Serie.Keys.ElementAt(trade.EntryIndex), trade.Serie.StockName, StockOperation.BUY, 1, 1, !trade.IsLong));
                    StockAnalyzerForm.MainFrame.BinckPortfolio.AddOperation(StockOperation.FromSimu(trade.Serie.Keys.ElementAt(trade.ExitIndex), trade.Serie.StockName, StockOperation.SELL, 1, 1, !trade.IsLong));
                }
                // this.graphCloseControl.ForceRefresh();
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
            return true;
        }
    }
}
