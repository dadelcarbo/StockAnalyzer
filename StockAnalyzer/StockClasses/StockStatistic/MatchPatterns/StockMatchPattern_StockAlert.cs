using System.Linq;

namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public class StockMatchPattern_StockAlert : IStockMatchPattern
    {
        public StockMatchPattern_StockAlert(StockAlertDef alert)
        {
            this.Alert = alert;
        }

        public StockAlertDef Alert { get; set; }

        public bool MatchPattern(StockSerie stockSerie, int index)
        {
            return stockSerie.MatchEvent(Alert, index);
        }
    }
}