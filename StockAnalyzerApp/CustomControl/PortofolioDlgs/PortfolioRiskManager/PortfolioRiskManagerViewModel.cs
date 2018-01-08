using StockAnalyzer;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs.PortfolioRiskManager
{
    class PortfolioRiskManagerViewModel : NotifyPropertyChangedBase
    {
        public PortfolioRiskManagerViewModel()
        {
            if (this.Portfolios == null) return;
            this.Portfolio = this.Portfolios.FirstOrDefault();
        }
        public IEnumerable<StockPortofolio> Portfolios => StockAnalyzerForm.MainFrame.StockPortofolioList;
        public IEnumerable<StockSerie> StockSeries => StockDictionary.StockDictionarySingleton.Values.OrderBy(s => s.StockName);

        private StockPortofolio portfolio;
        public StockPortofolio Portfolio
        {
            get { return portfolio; }
            set
            {
                if (portfolio == value) return;
                try
                {
                    StockAnalyzerForm.MainFrame.UseWaitCursor = true;
                    portfolio = value;
                    portfolio.Initialize();
                    OnPropertyChanged("Portfolio");
                    this.GeneratePositions();
                }
                finally
                {
                    StockAnalyzerForm.MainFrame.UseWaitCursor = false;
                }
            }
        }

        private void GeneratePositions()
        {
            if (this.portfolio == null)
            {
                Positions.Clear();
                return;
            }

            Positions = new ObservableCollection<PositionViewModel>();

            var nbActiveStocks = this.portfolio.OrderList.GetNbActiveStock();
            var stockDictionary = StockDictionary.StockDictionarySingleton;
            foreach (string stockName in nbActiveStocks.Keys)
            {
                var stockOrder = this.portfolio.OrderList.GetActiveSummaryOrder(stockName);
                if (stockOrder != null)
                {
                    var position = new PositionViewModel(portfolio, stockOrder);
                    StockSerie stockSerie;
                    if (stockDictionary.ContainsKey(stockName) && (stockSerie = stockDictionary[stockName]).Initialise())
                    {
                        StockDailyValue lastStockValue = stockSerie.Values.Last();
                        position.CurrentValue = lastStockValue.CLOSE;
                    }
                    else
                    {
                        float totalValue = Math.Abs(stockOrder.TotalCost);
                        position.CurrentValue = stockOrder.UnitCost;
                    }
                    this.Positions.Add(position);
                }
            }

            OnPropertyChanged("Positions");
        }

        public ObservableCollection<PositionViewModel> Positions { get; set; }
    }
}
