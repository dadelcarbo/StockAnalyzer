using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.SimulationDlgs.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerSettings;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    public class AgentSimulationViewModel : NotifyPropertyChangedBase
    {
        public AgentSimulationViewModel()
        {
            this.performText = "Perform";
            this.Selector = "ExpectedGainPerBar";

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            agent = Agents.FirstOrDefault();
            entryStop = EntryStops.FirstOrDefault();
            TrailStop = TrailStops.FirstOrDefault();

            this.BarDuration = new StockBarDuration(StockAnalyzerForm.MainFrame.ViewModel.BarDuration);
            this.Group = StockAnalyzerForm.MainFrame.Group;
        }
        public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));
        public StockSerie.Groups Group { get; set; }

        private StockBarDuration barDuration;
        public StockBarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

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
        #region TrailStop
        public List<string> TrailStops => StockTrailStopManager.GetTrailStopList();

        private string trailStop;
        public string TrailStop
        {
            get => trailStop;
            set
            {
                if (trailStop != value)
                {
                    trailStop = value;
                    OnPropertyChanged("TrailStopParameters");
                    OnPropertyChanged("TrailStopDescription");
                }
            }
        }

        public string TrailStopDescription => StockTrailStopManager.CreateTrailStop(trailStop).Definition;

        public IEnumerable<ParameterRangeViewModel> TrailStopParameters => ParameterRangeViewModel.GetTrailStopParameters(trailStop);
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
                engine = new StockAgentEngine(agentType, entryStopType);
                engine.BestAgentDetected += (bestAgent, bestEntryStop) =>
                {
                    this.BestAgent = bestAgent;
                    this.BestEntryStop = bestEntryStop;
                    string msg = bestAgent.TradeSummary.ToLog(this.BarDuration) + Environment.NewLine;
                    msg += bestAgent.ToLog() + Environment.NewLine;
                    msg += bestEntryStop.ToLog() + Environment.NewLine;
                    this.Report = msg;
                };
                engine.AgentPerformed += (agent, entryStop) =>
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
                            rpt += engine.BestAgent.GetParameterValues();

                            // Update Simu Portfolio
                            StockPortfolio.SimulationPortfolio.MaxPositions = StockDictionary.Instance.Values.Count(x => x.BelongsToGroup(this.Group));
                            StockPortfolio.SimulationPortfolio.InitFromTradeSummary(this.TradeSummary.Trades);
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

                engine.GreedySelection(stockSeries, new StockBarDuration(this.BarDuration), 20, selector);
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
