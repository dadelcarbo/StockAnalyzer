using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
            this.Accuracy = 20;
            this.Selector = "ExpectedGain";

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            agent = Agents.FirstOrDefault();
            this.Duration = StockAnalyzerForm.MainFrame.BarDuration;
            this.Group = StockAnalyzerForm.MainFrame.Group;
        }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }
        public IList<StockBarDuration> Durations => StockBarDuration.Values;
        public StockBarDuration Duration { get; set; }

        public int Accuracy { get; set; }
        public int MaxPosition
        {
            get { return StockPortfolio.MaxPositions; }
            set { StockPortfolio.MaxPositions = value; }
        }
        public List<string> Selectors => new List<string> { "WinTradeRatio", "WinLossRatio", "ExpectedGain" };
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
        public IEnumerable Parameters => ParameterRangeViewModel.GetParameters(this.agentType);

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
                engine.BestAgentDetected += (bestAgent) =>
                {
                    string msg = bestAgent.TradeSummary.ToLog() + Environment.NewLine;
                    msg += bestAgent.ToLog() + Environment.NewLine;
                    this.Report = msg;
                };
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
                    else
                    {
                        var report = engine.Report;
                        this.Report = report;
                        string rpt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\t" + this.Selector + "\t" + this.Group + "\t" + this.Duration + "\t" + this.Agent + "\t" + this.MaxPosition + "\t";
                        rpt += engine.BestTradeSummary.ToStats();
                        rpt += engine.BestAgent.GetParameterValues();
                        Clipboard.SetText(rpt);

                        using (var sr = new StreamWriter(Path.Combine(Settings.Default.RootFolder, "AgentReport.tsv"), true))
                        {
                            sr.WriteLine(rpt);
                        }

                        this?.Completed();
                    }
                    this.worker = null;
                };

                this.PerformText = "Cancel";
                StockAnalyzerForm.TimerSuspended = true;
                this.Report = "Initializing series...";
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
                    case "WinTradeRatio":
                        selector = t => t.WinTradeRatio;
                        break;
                    case "WinLossRatio":
                        selector = t => t.CumulGain;
                        break;
                    case "ExpectedGain":
                        selector = t => t.ExpectedReturn;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid selector: " + this.Selector);
                }

                engine.GreedySelection(stockSeries, this.Duration, 20, this.Accuracy, selector);
                if (engine.BestTradeSummary == null)
                    return false;
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
                return false;
            }
            return true;
        }
    }
}
