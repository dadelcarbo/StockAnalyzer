using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockPortfolio;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public class AgentSimulationViewModel : NotifyPropertyChangedBase
    {
        public AgentSimulationViewModel()
        {
            this.performText = "Perform";
            this.Selector = "ExpectedGainPerBar";

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            Agent = Agents.FirstOrDefault();
            EntryStop = EntryStops.FirstOrDefault();
            EntryTarget = EntryTargets.FirstOrDefault();

            this.BarDuration = StockAnalyzerForm.MainFrame.ViewModel.BarDuration;
            this.Group = StockAnalyzerForm.MainFrame.Group;
        }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }

        private BarDuration barDuration;
        public BarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

        public List<string> Selectors => new List<string> { "RiskRewardRatio", "ExpectedGainPerBar", "ExpectedGain", "Kelly %", "WinTradeRatio", "WinLossRatio", "TotalGain" };
        public string Selector { get; set; }

        public void Cancel()
        {
            if (worker != null)
            {
                worker.CancelAsync();
                worker = null;
            }
        }

        #region Agent
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
                    OnPropertyChanged("AgentParameters");
                    OnPropertyChanged("AgentDescription");
                }
            }
        }

        public string AgentDescription => StockAgentBase.CreateInstance(this.agent).Description;

        private Type agentType => typeof(IStockAgent).Assembly.GetType("StockAnalyzer.StockAgent.Agents." + agent + "Agent");
        public IEnumerable<ParameterRangeViewModel> AgentParameters => ParameterRangeViewModel.GetParameters(this.agentType);
        #endregion

        #region EntryStop
        public List<string> EntryStops => StockEntryStopBase.GetEntryStopNames();

        private string entryStop;
        public string EntryStop
        {
            get => entryStop;
            set
            {
                if (entryStop != value)
                {
                    entryStop = value;
                    OnPropertyChanged("EntryStopParameters");
                    OnPropertyChanged("EntryStopDescription");
                }
            }
        }

        private Type entryStopType => StockEntryStopBase.GetType(entryStop);
        public string EntryStopDescription => StockEntryStopBase.CreateInstance(entryStop)?.Description;
        public IEnumerable<ParameterRangeViewModel> EntryStopParameters => ParameterRangeViewModel.GetParameters(entryStopType);
        #endregion

        #region EntryTarget
        public List<string> EntryTargets => StockEntryTargetBase.GetEntryTargetNames();

        private string entryTarget;
        public string EntryTarget
        {
            get => entryTarget;
            set
            {
                if (entryTarget != value)
                {
                    entryTarget = value;
                    OnPropertyChanged("EntryTargetParameters");
                    OnPropertyChanged("EntryTargetDescription");
                }
            }
        }

        private Type entryTargetType => StockEntryTargetBase.GetType(entryTarget);
        public string EntryTargetDescription => StockEntryTargetBase.CreateInstance(entryTarget)?.Description;
        public IEnumerable<ParameterRangeViewModel> EntryTargetParameters => ParameterRangeViewModel.GetParameters(entryTargetType);
        #endregion

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
        string stats;
        public string Stats
        {
            get => stats;
            set
            {
                if (stats != value)
                {
                    stats = value;
                    OnPropertyChanged("Stats");
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

        StockAgentEngine engine;

        private IStockAgent bestAgent;
        public IStockAgent BestAgent
        {
            get => bestAgent;
            set
            {
                if (bestAgent != value)
                {
                    bestAgent = value;
                    OnPropertyChanged("BestAgent");
                }
            }
        }
        private IStockEntryStop bestEntryStop;
        public IStockEntryStop BestEntryStop
        {
            get => bestEntryStop;
            set
            {
                if (bestEntryStop != value)
                {
                    bestEntryStop = value;
                    OnPropertyChanged("BestEntryStop");
                }
            }
        }
        private IStockEntryTarget bestEntryTarget;
        public IStockEntryTarget BestEntryTarget
        {
            get => bestEntryTarget;
            set
            {
                if (bestEntryTarget != value)
                {
                    bestEntryTarget = value;
                    OnPropertyChanged("BestEntryTarget");
                }
            }
        }

        public void Perform()
        {
            if (worker == null)
            {
                this.Report = "Performing";

                this.Stats = this.Group.ToString() + "\t" + this.BarDuration.ToString() + "\t" + this.Agent + Environment.NewLine + Environment.NewLine;
                if (this.AgentParameters.Count() == 0)
                {
                    this.Stats += "WinRatio\tRiskRewardRatio\tExpectedGainPerBar\t" + Environment.NewLine;
                }
                else
                {
                    this.Stats += "WinRatio\tRiskRewardRatio\tExpectedGainPerBar\t" + this.AgentParameters.Select(p => p.Name).Aggregate((i, j) => i + "\t" + j) + Environment.NewLine;
                }
                engine = new StockAgentEngine(agentType, entryStopType, entryTargetType);
                engine.BestAgentDetected += (bestAgent, bestEntryStop, bestEntryTarget) =>
                {
                    this.BestAgent = bestAgent;
                    this.BestEntryStop = bestEntryStop;
                    this.BestEntryTarget = bestEntryTarget;
                    string msg = bestAgent.TradeSummary.ToLog(this.BarDuration) + Environment.NewLine;
                    msg += bestAgent.ToLog() + Environment.NewLine;
                    msg += bestEntryStop.ToLog() + Environment.NewLine;
                    msg += bestEntryTarget.ToLog() + Environment.NewLine;
                    this.Report = msg;
                };
                engine.AgentPerformed += (agent, entryStop, entryTarget) =>
                {
                    this.Stats += (agent.TradeSummary.WinTradeRatio.ToString("P2") + "\t" + agent.TradeSummary.RiskRewardRatio.ToString("#.##") + "\t" + agent.TradeSummary.ExpectedGainPerBar.ToString("P3") + "\t" + agent.ToParamValueString() + Environment.NewLine)
                    .Replace(".", ",");
                };
                worker = new BackgroundWorker();
                engine.Worker = worker;
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = true;
                worker.DoWork += RunAgentEngineOnGroup;
                worker.RunWorkerCompleted += (a, e) =>
                {
                    try
                    {
                        StockTimer.TimerSuspended = false;
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
                            this.TradeSummary = engine.BestTradeSummary;
                            string rpt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\t" + this.Selector + "\t" + this.Group + "\t" + this.BarDuration + "\t" + this.Agent + "\t";
                            rpt += engine.BestTradeSummary.ToStats();
                            rpt += EntryStop + "\t";
                            rpt += EntryTarget + "\t";
                            rpt += engine.BestAgent.GetParameterValues();

                            // Update Simu Portfolio
                            StockPortfolio.SimulationPortfolio.MaxPositions = StockDictionary.Instance.Values.Count(x => x.BelongsToGroup(this.Group));
                            StockPortfolio.SimulationPortfolio.InitPositionFromTradeSummary(this.TradeSummary.Trades);
                            StockAnalyzerForm.MainFrame.Portfolio = StockPortfolio.SimulationPortfolio;

                            using (var sr = new StreamWriter(Path.Combine(Folders.PersonalFolder, "AgentReport.tsv"), true))
                            {
                                sr.WriteLine(rpt);
                            }

                            this?.Completed();
                        }
                    }
                    catch (Exception ex)
                    {
                        StockLog.Write(ex);
                    }
                    this.worker = null;
                };

                this.PerformText = "Cancel";
                StockTimer.TimerSuspended = true;
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
            try
            {
                StockDataProviderBase.IntradayDownloadSuspended = true;
                Thread.CurrentThread.CurrentUICulture = StockAnalyzerForm.EnglishCulture;
                Thread.CurrentThread.CurrentCulture = StockAnalyzerForm.EnglishCulture;
                engine.ProgressChanged += (s, evt) =>
                {
                    this.ProgressValue = evt.ProgressPercentage;
                };

                var series = StockAnalyzerForm.MainFrame.StockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(this.Group));
                if (this.RunAgentEngine(series))
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            finally
            {
                StockDataProviderBase.IntradayDownloadSuspended = false;
            }
        }

        private bool RunAgentEngine(IEnumerable<StockSerie> stockSeries)
        {
            try
            {
                Func<StockTradeSummary, float> selector;
                switch (this.Selector)
                {
                    case "RiskRewardRatio":
                        selector = t => t.RiskRewardRatio;
                        break;
                    case "WinTradeRatio":
                        selector = t => t.WinTradeRatio;
                        break;
                    case "WinLossRatio":
                        selector = t => t.WinLossRatio;
                        break;
                    case "TotalGain":
                        selector = t => t.CumulGain;
                        break;
                    case "ExpectedGain":
                        selector = t => t.ExpectedReturn;
                        break;
                    case "ExpectedGainPerBar":
                        selector = t => t.ExpectedGainPerBar;
                        break;
                    case "Kelly %":
                        selector = t => t.ExpectedGainPerBar;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid selector: " + this.Selector);
                }

                engine.GreedySelection(stockSeries, this.BarDuration, 20, selector);
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
