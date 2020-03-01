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
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public IEnumerable<StockPositionViewModel> OpenedPositions
        {
            get
            {
                var positions = Portfolio.Positions.Where(p => !p.IsClosed).OrderBy(p => p.StockName).Select(p => new StockPositionViewModel(p)).ToList();
                float val = this.Portfolio.Balance;
                foreach (var pos in positions)
                {
                    float value = StockPortfolio.PriceProvider.GetClosingPrice(pos.StockName, DateTime.Now);
                    if (value == 0.0f)
                    {
                        val += pos.Qty * pos.OpenValue;
                        pos.LastValue = pos.Qty * pos.OpenValue;
                    }
                    else
                    {
                        val += pos.Qty * value;
                        pos.LastValue = pos.Qty * value;
                    }
                }
                this.Value = val;
                return positions;
            }
        }

        public float Value { get; set; }
    }
}