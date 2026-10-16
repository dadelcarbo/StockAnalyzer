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
        public string StockName { get => stockName; private set => SetProperty(ref stockName, value); }

        private string tradingViewSymbol = string.Empty;
        public string TradingViewSymbol { get => tradingViewSymbol; set => SetProperty(ref tradingViewSymbol, value); }

        private float closePrice = 0f;
        public float ClosePrice { get => closePrice; set => SetProperty(ref closePrice, value); }

        private float freeCashFlowPerShare = 0f;
        public float FreeCashFlowPerShare { get => freeCashFlowPerShare; set => SetProperty(ref freeCashFlowPerShare, value); }

        private float debtPerShare = 0f;
        public float DebtPerShare { get => debtPerShare; set => SetProperty(ref debtPerShare, value); }

        private float cashPerShare = 0f;
        public float CashPerShare { get => cashPerShare; set => SetProperty(ref cashPerShare, value); }

        private float growthRate = 0.05f;
        public float GrowthRate { get => growthRate; set => SetProperty(ref growthRate, value); }

        private float intrinsicValue = 0f;
        public float IntrinsicValue { get => intrinsicValue; set => SetProperty(ref intrinsicValue, value); }

        #endregion
    }
}
