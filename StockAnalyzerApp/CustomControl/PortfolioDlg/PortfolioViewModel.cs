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
        public PortfolioViewModel(StockPortfolio p)
        {
            Portfolio = p;
        }

        [Property]
        public string Name { get => Portfolio.Name; set => Portfolio.Name = value; }

        [Property]
        public float StartBalance { get => Portfolio.InitialBalance; set => Portfolio.InitialBalance = value; }

        [Property]
        public int MaxPositions { get => Portfolio.MaxPositions; set => Portfolio.MaxPositions = value; }

        [Property]
        public float Balance { get => Portfolio.Balance; set => Portfolio.Balance = value; }

        [Property]
        public float MaxRisk { get => Portfolio.MaxRisk; set => Portfolio.MaxRisk = value; }

        [Property]
        public DateTime CreationDate { get => Portfolio.CreationDate; set => Portfolio.CreationDate = value; }

        public List<StockTradeOperation> TradeOperations => Portfolio.TradeOperations;

        public IEnumerable<StockPositionViewModel> OpenedPositions => Portfolio.OpenedPositions.OrderBy(p => p.StockName).Select(p => new StockPositionViewModel(p, this));

        public IEnumerable<StockPositionViewModel> ClosedPositions
        {
            get
            {
                var positions = Portfolio.Positions.Where(p => p.IsClosed).OrderBy(p => p.StockName).Select(p => new StockPositionViewModel(p, this));
                return positions;
            }
        }

        public float Value => Portfolio.Balance + this.OpenedPositions.Select(p => p.EntryQty * p.LastValue).Sum();

        public float RiskFreeValue => Portfolio.Balance + this.OpenedPositions.Select(p => p.EntryQty * p.TrailStop).Sum();

        public bool IsDirty { get; set; }
    }
}