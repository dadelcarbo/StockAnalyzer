using System.ComponentModel.DataAnnotations;

namespace StockAnalyzer.StockAgent
{
    public class PositionManagement
    {
        public PositionManagement()
        {
            this.PortfolioInitialBalance = 10000f;
            this.PortfolioRisk = 1;
        }

        [Display(Description = "Maximum number of opened positions")]
        public int MaxPositions { get; set; }

        [Display(Description = "Maximum portfolio risk for a position in %", Name = "Portfolio Risk (%)")]
        public float PortfolioRisk { get; set; }

        [Display(Description = "Portfolio initial balance")]
        public float PortfolioInitialBalance { get; set; }

        public float StopATR { get; set; }
    }
}
