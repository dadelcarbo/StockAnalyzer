using StockAnalyzer;
using StockAnalyzer.StockHelpers;
using System;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.ShareValuationDlg
{
    public class ShareValuationViewModel : NotifyPropertyChangedBase
    {
        public ShareValuationViewModel()
        {
            var instrument = MainFrameViewModel.Instance.Instrument;
            if (instrument == null) return;
            this.StockName = instrument.DisplayName;
            this.TradingViewSymbol = $"{instrument.Market}:{instrument.Symbol}";

            this.ClosePrice = instrument.GetDefaultDataSerie().LastValue.CLOSE;
        }
        #region Common Properties
        private float discountRate = 0.06f;
        public float DiscountRate { get => discountRate; set => SetProperty(ref discountRate, value); }
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
        public float IntrinsicValue { get => intrinsicValue; private set => SetProperty(ref intrinsicValue, value); }

        private float premiumDiscount = 0f;
        public float PremiumDiscount { get => premiumDiscount; set => SetProperty(ref premiumDiscount, value); }

        private DateTime? calculationDate = null;
        public DateTime? CalculationDate { get => calculationDate; private set => SetProperty(ref calculationDate, value); }

        private CommandBase loadFromTradingViewCmd;
        public ICommand LoadFromTradingViewCmd => loadFromTradingViewCmd ??= new CommandBase(PerformLoadFromTradingViewCmd);

        private void PerformLoadFromTradingViewCmd()
        {
            this.cashPerShare = 2.5f;
            this.debtPerShare = 3.4f;
            this.freeCashFlowPerShare = 4.1f;

            PerformDcfCalculation();
        }

        /// <summary>
        /// Performs a discounted cash flow (DCF) valuation using configured inputs and stores results in the instance
        /// state.
        /// </summary>
        private void PerformDcfCalculation()
        {
            // Calculate discounted cash flow (DCF) valuation using the formula over 20 years with the configured growth rate and discount rate.
            float totalDcf = 0.0f;
            float fcf = this.FreeCashFlowPerShare;
            for (int i = 0; i < 20; i++)
            {
                fcf *= (1 + this.GrowthRate);
                totalDcf += fcf * (float)Math.Pow(1 - this.DiscountRate, i + 1);
            }

            this.IntrinsicValue = this.CashPerShare - this.DebtPerShare + totalDcf;
            this.PremiumDiscount = (this.IntrinsicValue - this.ClosePrice) / this.ClosePrice;
            this.CalculationDate = DateTime.Now;

        }

        private CommandBase performDcfCalculationCmd;
        public ICommand PerformDcfCalculationCmd => performDcfCalculationCmd ??= new CommandBase(PerformDcfCalculation);

        #endregion
    }
}
