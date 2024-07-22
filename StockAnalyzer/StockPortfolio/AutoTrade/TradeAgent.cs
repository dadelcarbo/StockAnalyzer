using StockAnalyzer.StockClasses;
using StockAnalyzer.StockHelpers;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{

    public class TradeAgent
    {
        public delegate void AgentStateChangedHandler(TradeAgent sender);

        public event AgentStateChangedHandler StateChanged;

        public delegate void AgentPositionChangedHandler(TradeAgent sender);

        public event AgentPositionChangedHandler PositionChanged;

        public int Id { get; set; }
        public bool Ready { get; set; }
        public StockSerie StockSerie { get; set; }

        public StockPortfolio Portfolio { get; set; }

        public BarDuration BarDuration { get; set; }

        public ITradeStrategy Strategy { get; set; }

        StockTimer timer { get; set; }

        private bool running;
        public bool Running { get => running; set { if (value != running) { running = value; StateChanged?.Invoke(this); } } }

        public void Start()
        {
            this.Running = true;
            if (false && TradeEngine.IsTest)
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


                var startTime = new TimeSpan(8, 0, 0);
                var endTime = new TimeSpan(22, 00, 0);
                //var startTime = new TimeSpan(0, 0, 0);
                //var endTime = new TimeSpan(23, 59, 0);
                timer = StockTimer.CreateDurationTimer(this.BarDuration, startTime, endTime, TimerEllapsed);
            }
        }

        public void Stop()
        {
            if (timer != null)
            {
                StockTimer.Stop(timer);
                timer = null;

                this.Running = false;
            }
        }

        private TradePosition position;
        public TradePosition Position { get => position; set { if (position != value) { position = value; PositionChanged?.Invoke(this); } } }

        public List<TradePosition> Positions { get; private set; } = new List<TradePosition>();

        public List<TradeRequest> TradeRequests { get; private set; } = new List<TradeRequest>();

        public int index = -1;
        private void TimerEllapsed()
        {
            using MethodLogger ml = new MethodLogger(this, false);

            if (!Running)
                return;

            var previousDuration = StockSerie.BarDuration;
            try
            {
                lock (StockSerie)
                {
                    // Get StockSerie Data
                    StockSerie.BarDuration = this.BarDuration;

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
                        var tradeRequest = Strategy.TryToOpenPosition(StockSerie, this.BarDuration, index);
                        if (tradeRequest != null)
                        {
                            TradeRequests.Add(tradeRequest);
                            // Process buy order request
                            ProcessBuyRequest(tradeRequest);
                        }
                    }
                    else
                    {
                        var tradeRequest = Strategy.TryToClosePosition(StockSerie, this.BarDuration, index);
                        if (tradeRequest != null)
                        {
                            TradeRequests.Add(tradeRequest);

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

            tradeRequest.Qty = 1; ///§§§§
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
            }
            else
            {
                var position = this.Portfolio.Positions.FirstOrDefault(p => p.StockName == this.StockSerie.StockName);
                if (position != null)
                {
                    var orderIdString = Portfolio.SaxoClosePosition(position, OrderType.Market);
                    if (string.IsNullOrEmpty(orderIdString))
                    {
                        StockLog.Write($"{this} Saxo close position failed");
                        return;
                    }

                    var orderId = long.Parse(orderIdString);
                    var order = Portfolio.SaxoOrders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order == null)
                    {
                        StockLog.Write($"{this} Saxo order not found for {tradeRequest}");
                        return;
                    }

                    this.Position.CloseDate = order.ActivityTime;
                    this.Position.ActualCloseValue = order.ExecutionPrice;
                    this.Position.TheoriticalCloseValue = tradeRequest.Value;
                    this.Position = null;
                }
            }
        }

        public override string ToString()
        {
            return $"{Id}-{Strategy.Name}-{BarDuration}-{StockSerie.StockName}";
        }
    }
}
