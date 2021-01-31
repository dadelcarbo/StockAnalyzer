using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class BinckPortfolioViewModel : NotifyPropertyChangedBase
    {
        public StockPortfolio Portfolio { get; set; }
        public BinckPortfolioViewModel(StockPortfolio p)
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

        //[Property]
        //public DateTime CreationDate { get => portfolio.CreationDate; set => portfolio.CreationDate = value; }

        public List<StockTradeOperation> TradeOperations => Portfolio.TradeOperations;

        public IEnumerable<StockPositionViewModel> OpenedPositions
        {
            get
            {
                var positions = Portfolio.OpenedPositions.OrderBy(p => p.StockName).Select(p => new StockPositionViewModel(p, this)).ToList();
                float val = Portfolio.Balance;
                foreach (var pos in positions)
                {
                    float value = StockPortfolio.PriceProvider.GetClosingPrice(pos.StockName, DateTime.Now, StockAnalyzer.StockClasses.BarDuration.Daily);
                    if (value == 0.0f) // if price is not found use open price
                    {
                        val += pos.EntryQty * pos.EntryValue;
                        pos.LastValue = pos.EntryValue;
                    }
                    else
                    {
                        if (pos.Leverage != 1) // if underlying price is found manage leverage
                        {
                            var underlyingOpenValue = StockPortfolio.PriceProvider.GetClosingPrice(pos.StockName, pos.ExitDate.Value, StockAnalyzer.StockClasses.BarDuration.Daily);
                            var variation = pos.Leverage * (value - underlyingOpenValue) / underlyingOpenValue;
                            value = pos.EntryValue * (1 + variation);
                        }
                        val += pos.EntryQty * value;
                        pos.LastValue = value;
                    }
                }
                this.Value = val;
                OnPropertyChanged(nameof(Value));
                return positions;
            }
        }

        public float Value { get; set; }
    }
}