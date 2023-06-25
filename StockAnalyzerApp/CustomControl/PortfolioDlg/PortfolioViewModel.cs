using StockAnalyzer;
using StockAnalyzer.StockPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public class PortfolioViewModel : NotifyPropertyChangedBase
    {
        public StockPortfolio Portfolio { get; set; }
        public PortfolioViewModel(StockPortfolio portfolio)
        {
            Portfolio = portfolio;

            this.OpenedPositions = new List<StockPositionBaseViewModel>();

            foreach (var pos in portfolio.Positions)
            {
                this.OpenedPositions.Add(new StockPositionBaseViewModel(pos, this));
            }

            this.Orders = portfolio.SaxoOrders.Select(o => new OrderViewModel(o));
            this.ActivityOrders = portfolio.ActivityOrders.Select(o => new ActivityOrderViewModel(o) { Name = portfolio.GetInstrument(o.Uic)?.Description });

        }

        [Property(null, "1-General")]
        public DateTime CreationDate { get => Portfolio.CreationDate; set => Portfolio.CreationDate = value; }
        [Property(null, "1-General")]
        public float StartBalance { get => Portfolio.InitialBalance; set => Portfolio.InitialBalance = value; }
        [Property(null, "1-General")]
        public string Name { get => Portfolio.Name; set => Portfolio.Name = value; }
        [Property(null, "1-General")]
        public float Balance => Portfolio.Balance;
        [Property(null, "1-General")]
        public float Value => Portfolio.TotalValue;

        [Property(null, "2-Risk")]
        public int MaxPositions { get => Portfolio.MaxPositions; set => Portfolio.MaxPositions = value; }
        [Property("P2", "2-Risk")]
        public float MaxRisk { get => Portfolio.MaxRisk; set => Portfolio.MaxRisk = value; }
        [Property("P2", "2-Risk")]
        public float MaxPositionSize { get => Portfolio.MaxPositionSize; set => Portfolio.MaxPositionSize = value; }


        [Property(null, "3-Saxo")]
        public bool IsSaxoSimu { get => Portfolio.IsSaxoSimu; set => Portfolio.IsSaxoSimu = value; }
        [Property(null, "3-Saxo")]
        public string SaxoAccountId { get => Portfolio.SaxoAccountId; set => Portfolio.SaxoAccountId = value; }
        [Property(null, "3-Saxo")]
        public string SaxoClientId { get => Portfolio.SaxoClientId; set => Portfolio.SaxoClientId = value; }
        [Property(null, "3-Saxo")]
        public DateTime SyncDate => Portfolio.LastSyncDate;

        [Property(null, "4-Extra")]
        public bool IsSimu { get => Portfolio.IsSimu; set => Portfolio.IsSimu = value; }


        public IEnumerable<StockOpenedOrder> OpenedOrders => Portfolio.GetActiveOrders().OrderByDescending(o => o.CreationDate);
        public IEnumerable<StockTradeOperation> TradeOperations => Portfolio.TradeOperations.OrderByDescending(o => o.Date);

        public IList<StockPositionBaseViewModel> OpenedPositions { get; private set; }

        public IEnumerable<StockPositionBaseViewModel> ClosedPositions => Portfolio.ClosedPositions.Where(p => p.IsClosed).OrderByDescending(p => p.ExitDate).Select(p => new StockPositionBaseViewModel(p, this));

        public float RiskFreeValue => Portfolio.Balance + this.OpenedPositions.Select(p => p.EntryQty * p.TrailStop).Sum();

        public bool IsDirty { get; set; }
        public IEnumerable<OrderViewModel> Orders { get; private set; }
        public IEnumerable<ActivityOrderViewModel> ActivityOrders { get; private set; }
    }
}