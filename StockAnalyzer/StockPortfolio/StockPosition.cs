using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockPortfolio
{
    public class StockPositionStep
    {
        public StockPositionStep(DateTime startDate, int number)
        {
            this.StartDate = startDate;
            this.EndDate = DateTime.MaxValue;
            this.Number = number;
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Number { get; set; }
    }

    public class StockPosition
    {
        public delegate void PositionClosedHanler(StockPosition position);
        public event PositionClosedHanler OnPositionClosed;

        public bool IsShort { get { return OpenOrder.IsShortOrder; } }
        public string StockName { get { return OpenOrder.StockName; } }
        public DateTime OpenDate { get { return OpenOrder.ExecutionDate; } }
        public int Number { get; set; }

        public float AverageOpenPrice { get; private set; }

        public bool IsClosed { get { return this.Number == 0; } }
        public DateTime? CloseDate { get; set; }

        public float TotalReturn { get; private set; }

        public StockOrder OpenOrder { get; set; }
        public List<StockOrder> Orders { get; private set; }

        public float TotalFee { get { return this.Orders.Sum(o => o.Fee); } }

        protected List<StockPositionStep> steps;

        public StockPosition(StockOrder openOrder)
        {
            this.OpenOrder = openOrder;
            this.Number = openOrder.Number;
            this.Orders = new List<StockOrder>() { openOrder };
            this.TotalReturn = 0;
            this.AverageOpenPrice = openOrder.UnitCost;
            this.steps = new List<StockPositionStep>();
            this.steps.Add(new StockPositionStep(this.OpenDate, this.Number));
        }

        public void Add(StockOrder order)
        {
            #region SANITY CHECK
            if (this.StockName != order.StockName)
            {
                throw new ArgumentException("Cannot add order for " + order.StockName + " to a position for " + this.StockName);
            }
            if ((this.IsShort ^ order.IsShortOrder))
            {
                throw new ArgumentException("Cannot mix short and long orders !!! ");
            }
            if ((!order.IsBuyOrder()) && order.Number > this.Number)
            {
                throw new ArgumentException("Cannot sell more than owned !!! ");
            }
            #endregion

            if (order.IsBuyOrder())
            {
                // Apply buy order
                this.AverageOpenPrice = (this.AverageOpenPrice * this.Number + order.UnitCost * order.Number) / (this.Number + order.Number);
                this.Number += order.Number;
                this.Orders.Add(order);
                this.steps.Last().EndDate = order.ExecutionDate;
                this.steps.Add(new StockPositionStep(order.ExecutionDate, this.Number));
            }
            else
            {
                // Apply sell order
                this.Number -= order.Number;
                this.Orders.Add(order);

                this.steps.Last().EndDate = order.ExecutionDate;

                if (this.IsShort)
                {
                    this.TotalReturn -= (order.UnitCost - this.AverageOpenPrice) * order.Number;
                }
                else
                {
                    this.TotalReturn += (order.UnitCost - this.AverageOpenPrice) * order.Number;
                }
                if (this.Number == 0)
                {
                    this.CloseDate = order.ExecutionDate;
                    if (this.OnPositionClosed != null)
                    {
                        this.OnPositionClosed(this);
                    }
                }
                else
                {
                    this.steps.Add(new StockPositionStep(order.ExecutionDate, this.Number));
                }
            }
        }

        public float Value(float value)
        {
            if (this.IsClosed) throw new StockAnalyzerException("Cannot evaluate as position is closed");

            return value * this.Number;
        }

        public float Value(float value, DateTime date)
        {
            if (date < this.OpenDate) throw new StockAnalyzerException("Cannot evaluate as position is not opened yet");
            if (this.CloseDate != null && date > this.CloseDate) throw new StockAnalyzerException("Cannot evaluate as position is already closed");

            var step = this.steps.Single(s => s.StartDate <= date && date < s.EndDate);

            return value * step.Number;
        }

    }
}
