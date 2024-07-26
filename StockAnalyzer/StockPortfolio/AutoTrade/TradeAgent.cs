using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{

    public class TradeAgent
    {
        public delegate void AgentStateChangedHandler(TradeAgent sender);

        public event AgentStateChangedHandler StateChanged;

        public delegate void AgentPositionChangedHandler(TradeAgent sender);

        public event AgentPositionChangedHandler PositionChanged;

        public TradeAgent(TradeAgentDef agentDef)
        {
            this.AgentDef = agentDef;
            if (StockDictionary.Instance.ContainsKey(agentDef.StockName))
            {
                this.StockSerie = StockDictionary.Instance[agentDef.StockName];
            }
            this.Strategy = TradeStrategyManager.CreateInstance("TrailAtr");
            this.Portfolio = StockPortfolio.Portfolios.FirstOrDefault(p => p.Name == agentDef.PortfolioName);
        }

        public TradeAgentDef AgentDef { get; private set; }

        [JsonIgnore]
        public StockSerie StockSerie { get; set; }

        [JsonIgnore]
        public StockPortfolio Portfolio { get; set; }

        public ITradeStrategy Strategy { get; set; }

        private TradePosition position;
        public TradePosition Position { get => position; set { if (position != value) { position = value; PositionChanged?.Invoke(this); } } }

        public ObservableCollection<TradePosition> Positions { get; private set; } = new ObservableCollection<TradePosition>();

        public ObservableCollection<TradeRequest> TradeRequests { get; private set; } = new ObservableCollection<TradeRequest>();


        private bool running;
        public bool Running { get => running; set { if (value != running) { running = value; StateChanged?.Invoke(this); } } }
        StockTimer timer { get; set; }

        public void Start()
        {
            if (this.Running)
                return;

            this.Running = true;
            if (TradeEngine.IsTest)
            {
                index = 10;

                var startTime = new TimeSpan(0, 0, 0);
                var endTime = new TimeSpan(23, 59, 0);
                timer = StockTimer.CreatePeriodicTimer(startTime, endTime, new TimeSpan(0, 0, 0, 0, 100), TimerEllapsed);
            }
            else
            {
                // Check is opened position for this instrument.
                var saxoPosition = this.Portfolio.SaxoGetPosition(StockSerie);
                if (saxoPosition != null)
                {
                    var positionId = long.Parse(saxoPosition.PositionId);
                    this.Position = this.Positions.FirstOrDefault(p => p.Id == positionId);
                    if (this.Position == null)
                    {
                        this.Position = new TradePosition
                        {
                            Id = long.Parse(saxoPosition.PositionId),
                            OpenDate = saxoPosition.PositionBase.ExecutionTimeOpen,
                            ActualOpenValue = saxoPosition.PositionBase.OpenPrice,
                            TheoriticalOpenValue = saxoPosition.PositionBase.OpenPrice,
                            Qty = (int)saxoPosition.PositionBase.Amount,
                            StockSerie = this.StockSerie
                            //Stop = tradeRequest.Stop
                        };
                        this.Positions.Add(this.Position);
                    }
                }


                var startTime = new TimeSpan(8, 0, 0);
                var endTime = new TimeSpan(23, 00, 0);
                //var startTime = new TimeSpan(0, 0, 0);
                //var endTime = new TimeSpan(23, 59, 0);
                timer = StockTimer.CreateDurationTimer(this.AgentDef.BarDuration, startTime, endTime, TimerEllapsed);
            }
        }

        public void Stop()
        {
            if (!this.Running)
                return;

            if (timer != null)
            {
                StockTimer.Stop(timer);
                timer = null;

                this.Running = false;
            }

            // Save result for analysis purpose
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters = { new JsonStringEnumConverter() },
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            var folder = Path.Combine(Folders.AutoTrade, "Runs", DateTime.Now.ToString("yyMMdd"));
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, this + "-" + DateTime.Now.ToString("yyMMdd_hhmmss")), JsonSerializer.Serialize(this, options));
        }

        public int index = -1;
        private void TimerEllapsed()
        {
            using MethodLogger ml = new MethodLogger(this, true);

            if (!Running)
                return;

            var previousDuration = StockSerie.BarDuration;
            try
            {
                lock (StockSerie)
                {
                    // Get StockSerie Data
                    StockDataProviderBase.DownloadSerieData(StockSerie);
                    StockSerie.BarDuration = this.AgentDef.BarDuration;

                    if (TradeEngine.IsTest)
                    {
                        index++;
                        if (index > StockSerie.LastIndex)
                        {
                            StockLog.Write($"{this} Test Agent Completed");
                            this.Stop();
                            return;
                        }
                    }

                    // Check Orders
                    if (Position == null)
                    {
                        var tradeRequest = Strategy.TryToOpenPosition(StockSerie, this.AgentDef.BarDuration, index);
                        if (tradeRequest != null)
                        {
                            TradeRequests.Add(tradeRequest);
                            // Process buy order request
                            ProcessBuyRequest(tradeRequest);
                        }
                    }
                    else
                    {
                        var tradeRequest = Strategy.TryToClosePosition(StockSerie, this.AgentDef.BarDuration, index);
                        if (tradeRequest != null)
                        {
                            TradeRequests.Add(tradeRequest);

                            if (TradeEngine.IsTest)
                            {
                                ProcessSellRequest(tradeRequest);
                                return;
                            }

                            var saxoPosition = this.Portfolio.SaxoGetPosition(this.Position.Id);
                            if (saxoPosition != null)
                            {
                                // Process sell order request
                                ProcessSellRequest(tradeRequest);
                            }
                            else
                            {
                                StockLog.Write($"{this} No position on ${StockSerie.StockName} found. ");
                                this.Position = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Auto trade error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            StockSerie.BarDuration = previousDuration;
        }

        private void ProcessBuyRequest(TradeRequest tradeRequest)
        {
            // Position size for trade risk
            var ptfRisk = this.Portfolio.RiskFreeValue * this.Portfolio.AutoTradeRisk;
            var unitRisk = tradeRequest.Value - tradeRequest.Stop;
            if (unitRisk == 0)
            {
                StockLog.Write($"{this} Buy Request rejected, Stop equals to Entry =>{tradeRequest.Qty}@{tradeRequest.Value}");
                return;
            }
            tradeRequest.Qty = (int)Math.Floor(ptfRisk / unitRisk);
            if (tradeRequest.Qty <= 0)
            {
                StockLog.Write($"{this} Buy Request rejected, too risky =>{tradeRequest.Qty}@{tradeRequest.Value}");
                return;
            }

            // Position size for maximum allocation
            var maxAllowed = this.Portfolio.RiskFreeValue * this.Portfolio.MaxPositionSize;
            if (tradeRequest.Qty * tradeRequest.Value > maxAllowed)
            {
                tradeRequest.Qty = (int)Math.Floor(maxAllowed / tradeRequest.Value);
            }
            if (tradeRequest.Qty <= 0)
            {
                StockLog.Write($"{this} Buy Request rejected, position too large => MaxAllowed{maxAllowed} Value:{tradeRequest.Value}");
                return;
            }

            // tradeRequest.Qty = TradeEngine.IsTest ? tradeRequest.Qty : 1; ///§§§§
            StockLog.Write($"{this} Buy Request=>{tradeRequest.Qty}@{tradeRequest.Value}");
            if (TradeEngine.IsTest)
            {
                this.Position = new TradePosition
                {
                    OpenDate = DateTime.Now,
                    ActualOpenValue = tradeRequest.Value,
                    TheoriticalOpenValue = tradeRequest.Value,
                    Qty = tradeRequest.Qty,
                    Stop = tradeRequest.Stop,
                    StockSerie = tradeRequest.StockSerie
                };
                this.Positions.Add(this.Position);
            }
            else
            {
                var orderId = Portfolio.SaxoBuyOrder(StockSerie, OrderType.Market, tradeRequest.Qty, tradeRequest.Stop);
                if (orderId == 0)
                {
                    StockLog.Write($"{this} Saxo Buy failed");
                    return;
                }

                var saxoPosition = this.Portfolio.SaxoGetPositions()?.FirstOrDefault(p => p.PositionBase.SourceOrderId == orderId.ToString());
                if (saxoPosition == null)
                {
                    StockLog.Write($"{this} Saxo position not found for {StockSerie.StockName}");
                    return;
                }

                //var order = Portfolio.SaxoOrders.FirstOrDefault(o => o.OrderId == orderId);
                //if (order == null)
                //{
                //    StockLog.Write($"{this} Saxo order not found for {tradeRequest}");
                //    return;
                //}

                this.Position = new TradePosition
                {
                    Id = long.Parse(saxoPosition.PositionId),
                    OpenDate = saxoPosition.PositionBase.ExecutionTimeOpen,
                    ActualOpenValue = saxoPosition.PositionBase.OpenPrice,
                    TheoriticalOpenValue = tradeRequest.Value,
                    Qty = tradeRequest.Qty,
                    Stop = tradeRequest.Stop,
                    StockSerie = tradeRequest.StockSerie
                };
                this.Positions.Add(this.Position);
            }
        }

        private void ProcessSellRequest(TradeRequest tradeRequest)
        {
            if (this.Position == null)
                return;

            tradeRequest.Qty = Position.Qty; // Force closing full position.
            StockLog.Write($"{this} Sell Request=>{tradeRequest.Qty}@{tradeRequest.Value}");

            if (TradeEngine.IsTest)
            {
                this.Position.CloseDate = DateTime.Now;
                this.Position.ActualCloseValue = tradeRequest.Value;
                this.Position.TheoriticalCloseValue = tradeRequest.Value;
                this.Position = null;
                return;
            }

            var position = this.Portfolio.SaxoGetPosition(this.Position.Id);
            if (position != null)
            {
                var orderId = Portfolio.SaxoClosePosition(this.Position.Id);
                if (orderId == 0)
                {
                    StockLog.Write($"{this} Saxo close position failed");
                    return;
                }

                int nbTries = 10;
                int attempt = 0;
                OrderActivity orderActivity = null;
                while (++attempt <= nbTries) // loop waiting for order to be executed.
                {
                    orderActivity = Portfolio.SaxoGetOrderActivities(orderId).FirstOrDefault(o => o.Status == "FinalFill");
                    if (orderActivity == null)
                    {
                        StockLog.Write($"{this} Saxo order {orderId} not found: Attempt {attempt}/{nbTries} for {tradeRequest}");
                        Task.Delay(1000).Wait();
                    }
                    else
                    {
                        break;
                    }
                }
                if (orderActivity != null) // Closing order successfully executed
                {
                    this.Position.CloseDate = orderActivity.ActivityTime;
                    this.Position.ActualCloseValue = orderActivity.ExecutionPrice;
                    this.Position.TheoriticalCloseValue = tradeRequest.Value;
                }
                else
                {
                    var positionCheck = this.Portfolio.SaxoGetPosition(this.Position.Id);
                    if (position == null) // Ok position is closed
                    {
                        this.Position.CloseDate = DateTime.Now;
                        this.Position.ActualCloseValue = tradeRequest.Value;
                        this.Position.TheoriticalCloseValue = tradeRequest.Value;
                    }
                    else // 
                    {
                        StockLog.Write($"{this} Agent stop as position closing failed after {nbTries} tries: {tradeRequest}");
                        this.Stop();

                        MessageBox.Show($"Agent {this} is unable to close position", "Auto trade error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            this.Position = null;
        }

        public override string ToString()
        {
            var running = Running ? "Running" : "Stopped";
            return $"{AgentDef.Id}-{Strategy.Name}-{AgentDef.BarDuration}-{StockSerie.StockName}-{running}";
        }
    }
}
