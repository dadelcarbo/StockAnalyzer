using StockAnalyzer.Portofolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockPortfolio
{
    public class StockPosition2
    {
        public delegate void PositionClosedHanler(StockPosition2 position);
        public event PositionClosedHanler OnPositionClosed;

        public bool IsShort { get { return OpenOrder.IsShortOrder; } }
        public string StockName { get { return OpenOrder.StockName; } }
        public DateTime OpenDate { get { return OpenOrder.ExecutionDate; } }
        public int Size { get; set; }

        public float AverageOpenPrice { get; private set; }

        public bool IsClosed { get { return this.Size == 0; } }
        public DateTime? CloseDate { get; set; }

        public float TotalReturn { get; private set; }

        public StockOrder OpenOrder { get; set; }
        public List<StockOrder> Orders { get; private set; }

        public float TotalFee { get { return this.Orders.Sum(o => o.Fee); } }

        public StockPosition2(StockOrder openOrder)
        {
            this.OpenOrder = openOrder;
            this.Size = openOrder.Number;
            this.Orders = new List<StockOrder>() { openOrder };
            this.TotalReturn = 0;
            this.AverageOpenPrice = openOrder.UnitCost;
        }

        public void AddOrder(StockOrder order)
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
            if ((!order.IsBuyOrder()) && order.Number > this.Size)
            {
                throw new ArgumentException("Cannot sell more than owned !!! ");
            }
            #endregion

            if (order.IsBuyOrder())
            {
                // Apply buy order
                this.AverageOpenPrice = (this.AverageOpenPrice * this.Size + order.UnitCost * order.Number) / (this.Size + order.Number);
                this.Size += order.Number;
                this.Orders.Add(order);
            }
            else
            {
                // Apply sell order
                this.Size -= order.Number;
                this.Orders.Add(order);

                if (this.IsShort)
                {
                    this.TotalReturn -= (order.UnitCost - this.AverageOpenPrice) * order.Number;
                }
                else
                {
                    this.TotalReturn += (order.UnitCost - this.AverageOpenPrice) * order.Number;
                }
                if (this.Size == 0)
                {
                    this.CloseDate = order.ExecutionDate;
                    if (this.OnPositionClosed != null)
                    {
                        this.OnPositionClosed(this);
                    }
                }
            }
        }
    }
}
