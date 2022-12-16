using System.ComponentModel.DataAnnotations;

namespace StockAnalyzer.StockAgent
{
    public class PositionManagement
    {
        public PositionManagement()
        {
        }

        [Display(Description = "Maximum number of opened positions")]
        public int MaxPositions { get; set; }

        [Display(Description = "Maximum portfolio risk for a position in %", Name = "Portfolio Risk (%)")]
        public float PortfolioRisk { get; set; }

        [Display(Description = "Portfolio initial balance", Name = "Portfolio Initial Balance")]
        public float PortfolioInitialBalance { get; set; }

        [Display(Description = "Rank indicator to select best stocks, the highest the better")]
        public string Rank { get; set; }

        [Display(Description = "Sets a stop nb ATRs below opening price. Positio. size is adjusted to match max portfolio risk.\r\n If set to 0, all stocks are equally distributed", Name = "Stop ATR")]
        public float StopATR { get; set; }

        [Display(Description = "Regime indice")]
        public string RegimeIndice { get; set; }

        [Display(Description = "Regime EMA Period")]
        public int RegimePeriod { get; set; }
    }
}
