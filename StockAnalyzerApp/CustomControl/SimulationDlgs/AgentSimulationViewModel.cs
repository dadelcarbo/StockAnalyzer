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
        }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }
        public IList<StockBarDuration> Durations => StockBarDuration.Values;
        public StockBarDuration Duration { get; set; }

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

        public string Report => engine?.Report;

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

        BackgroundWorker worker = null;

        StockAgentEngine engine;
        public void Perform()
        {
            if (worker == null)
            {
                engine = new StockAgentEngine(agentType);
                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = true;
                worker.DoWork += RunAgentEngineOnGroup;
                worker.RunWorkerCompleted += (a, b) =>
                {
                    Clipboard.SetText(this.Report);
                    this.worker = null;
                    this.ProgressValue = 0;
                    this.PerformText = "Perform"; 
                };

                this.PerformText = "Cancel";
                worker.RunWorkerAsync();
            }
            else
            {
                worker.CancelAsync();
                worker = null;
                this.PerformText = "Perform";
                this.ProgressValue = 0;
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

            this.RunAgentEngine(StockAnalyzerForm.MainFrame.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.Group) && s.Initialise()));
        }

        private void RunAgentEngine(IEnumerable<StockSerie> stockSeries)
        {
            try
            {
                foreach (var serie in stockSeries)
                {
                    serie.BarDuration = this.Duration;
                }

                engine.GreedySelection(stockSeries, 20);

                StockAnalyzerForm.MainFrame.BinckPortfolio = new StockPortfolio();
                foreach (var trade in engine.BestTradeSummary.Trades)
                {
                    // Create operations
                    StockAnalyzerForm.MainFrame.BinckPortfolio.AddOperation(StockOperation.FromSimu(trade.Serie.Keys.ElementAt(trade.EntryIndex), trade.Serie.StockName, StockOperation.BUY, 1, 1, !trade.IsLong));
                    StockAnalyzerForm.MainFrame.BinckPortfolio.AddOperation(StockOperation.FromSimu(trade.Serie.Keys.ElementAt(trade.ExitIndex), trade.Serie.StockName, StockOperation.SELL, 1, 1, !trade.IsLong));
                }

                OnPropertyChanged("Report");
                // this.graphCloseControl.ForceRefresh();
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
        }
    }
}
