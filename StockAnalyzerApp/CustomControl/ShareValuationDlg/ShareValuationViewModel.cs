using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.ShareValuationDlg
{
    public class ShareValuationViewModel : NotifyPropertyChangedBase
    {
        #region Common Properties
        private float discountRate = 0.06f;
        public float DiscountRate { get => discountRate; set => SetProperty(ref discountRate, value); }

        private float defaultGrowthRate = 0.05f;
        public float DefaultGrowthRate { get => defaultGrowthRate; set => SetProperty(ref defaultGrowthRate, value); }
        #endregion

        #region Share Valuation Properties

        private string stockName = string.Empty;
        public string StockName { get => stockName; set => SetProperty(ref stockName, value); }

        private string tradingViewSymbol = string.Empty;
        public string TradingViewSymbol { get => tradingViewSymbol; set => SetProperty(ref tradingViewSymbol, value); }


        #endregion
    }
}
