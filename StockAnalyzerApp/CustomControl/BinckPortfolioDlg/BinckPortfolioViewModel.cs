using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class BinckPortfolioViewModel : NotifyPropertyChangedBase
    {
        public BinckPortfolioViewModel()
        {
        }

        public List<StockPortfolio> Portfolios => StockPortfolio.Portfolios;

        public StockPortfolio Portfolio
        {
            get { return StockAnalyzerForm.MainFrame.BinckPortfolio; }
            set
            {
                if (StockAnalyzerForm.MainFrame.BinckPortfolio != value)
                {
                    StockAnalyzerForm.MainFrame.BinckPortfolio = value;

                    OnPropertyChanged(nameof(OpenedPositions));
                    OnPropertyChanged(nameof(Portfolio));
                }
            }
        }

        public IEnumerable<StockPositionViewModel> OpenedPositions
        {
            get
            {
                var positions = Portfolio.Positions.Where(p => !p.IsClosed).OrderBy(p => p.StockName).Select(p => new StockPositionViewModel(p, this)).ToList();
                float val = this.Portfolio.Balance;
                foreach (var pos in positions)
                {
                    float value = StockPortfolio.PriceProvider.GetClosingPrice(pos.StockName, DateTime.Now, StockAnalyzer.StockClasses.BarDuration.Daily);
                    if (value == 0.0f) // if price is not found use open price
                    {
                        val += pos.Qty * pos.OpenValue;
                        pos.LastValue = pos.OpenValue;
                    }
                    else
                    {
                        if (pos.Leverage != 1) // if underlying price is found manage leverage
                        {
                            var underlyingOpenValue = StockPortfolio.PriceProvider.GetClosingPrice(pos.StockName, pos.StartDate, StockAnalyzer.StockClasses.BarDuration.Daily);
                            var variation = pos.Leverage * (value - underlyingOpenValue) / underlyingOpenValue;
                            value = pos.OpenValue * (1 + variation);
                        }
                        val += pos.Qty * value;
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