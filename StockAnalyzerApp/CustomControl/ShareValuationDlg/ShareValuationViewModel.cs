using StockAnalyzer;
using StockAnalyzer.StockData;
using StockAnalyzer.StockData.DataProviders;
using StockAnalyzer.StockHelpers;
using StockAnalyzerSettings;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.ShareValuationDlg
{
    public class ShareValuationViewModel : NotifyPropertyChangedBase
    {
        public ShareValuationViewModel()
        {
        }
        #region Common Properties
        private float discountRate = 0.06f;
        public float DiscountRate { get => discountRate; set => SetProperty(ref discountRate, value); }
        #endregion

        #region Share Valuation Properties

        private string isin = string.Empty;

        private string stockName = string.Empty;
        public string StockName { get => stockName; private set => SetProperty(ref stockName, value); }

        private string tradingViewSymbol = string.Empty;
        public string TradingViewSymbol { get => tradingViewSymbol; set => SetProperty(ref tradingViewSymbol, value); }
        private string tradingViewCountry = string.Empty;
        public string TradingViewCountry { get => tradingViewCountry; set => SetProperty(ref tradingViewCountry, value); }

        private float closePrice = 0f;
        [JsonIgnore]
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

        private float premiumDiscount = 0f;
        [JsonIgnore]
        public float PremiumDiscount { get => premiumDiscount; set => SetProperty(ref premiumDiscount, value); }

        private DateTime? fundamentalDate = null;
        public DateTime? FundamentalDate { get => fundamentalDate; set => SetProperty(ref fundamentalDate, value); }
        private DateTime? calculationDate = null;
        public DateTime? CalculationDate { get => calculationDate; set => SetProperty(ref calculationDate, value); }

        private CommandBase loadFromTradingViewCmd;
        [JsonIgnore]
        public ICommand LoadFromTradingViewCmd => loadFromTradingViewCmd ??= new CommandBase(PerformLoadFromTradingViewCmd);

        private bool canSave = false;
        [JsonIgnore]
        public bool CanSave { get => canSave; set => SetProperty(ref canSave, value); }

        #endregion
        private async void PerformLoadFromTradingViewCmd()
        {
            try
            {
                if (string.IsNullOrEmpty(this.tradingViewSymbol))
                    return;
                var res = await TradingViewFundamentalDataProvider.GetFundamentalsAsync(instrument, TradingViewFundamentalDataProvider.DcfPerShare);
                if (res != null)
                {
                    this.FreeCashFlowPerShare = res["free_cash_flow_per_share_fy"];
                    this.DebtPerShare = res["total_debt_per_share_fy"];
                    this.CashPerShare = res["cash_per_share_fy"];
                }
            }
            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
                return;
            }
            this.FundamentalDate = DateTime.Now;

            PerformDcfCalculation();
        }

        /// <summary>
        /// Performs a discounted cash flow (DCF) valuation using configured inputs and stores results in the instance
        /// state.
        /// </summary>
        private void PerformDcfCalculation()
        {
            if (this.freeCashFlowPerShare == 0.0f || this.debtPerShare == 0.0f || this.cashPerShare == 0.0f)
            {
                this.IntrinsicValue = 0.0f;
                this.PremiumDiscount = 0.0f;
                this.CalculationDate = null;
                return;
            }

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

            this.CanSave = true;
        }

        private CommandBase performDcfCalculationCmd;
        [JsonIgnore]
        public ICommand PerformDcfCalculationCmd => performDcfCalculationCmd ??= new CommandBase(PerformDcfCalculation);

        private CommandBase saveCmd;
        [JsonIgnore]
        public ICommand SaveCmd => saveCmd ??= new CommandBase(PerformSaveCmd, () => this.CanSave, this, new[] { nameof(CanSave) });

        private void PerformSaveCmd()
        {
            var filePath = Path.Combine(Folders.DCF, $"{this.isin}.json");

            File.WriteAllText(filePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));

            this.CanSave = false;
        }

        private StockInstrument instrument;
        internal void Initialize(StockInstrument instrument)
        {
            this.instrument = instrument;
            if (instrument == null) return;

            this.StockName = instrument.DisplayName;
            this.TradingViewSymbol = $"{TradingViewFundamentalDataProvider.GetTradingViewSymbol(instrument)}";
            this.TradingViewCountry = $"{TradingViewFundamentalDataProvider.GetTradingViewCountry(instrument)}";

            this.isin = instrument.Isin;

            this.ClosePrice = instrument.GetDefaultDataSerie().LastValue.CLOSE;
            var filePath = Path.Combine(Folders.DCF, $"{this.isin}.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var savedData = JsonSerializer.Deserialize<ShareValuationViewModel>(json);
                if (savedData != null)
                {
                    this.FreeCashFlowPerShare = savedData.FreeCashFlowPerShare;
                    this.DebtPerShare = savedData.DebtPerShare;
                    this.CashPerShare = savedData.CashPerShare;
                    this.GrowthRate = savedData.GrowthRate;
                    this.DiscountRate = savedData.DiscountRate;
                    this.IntrinsicValue = savedData.IntrinsicValue;
                    this.PremiumDiscount = savedData.PremiumDiscount;
                    this.FundamentalDate = savedData.FundamentalDate;
                    this.CalculationDate = savedData.CalculationDate;

                    this.PremiumDiscount = (this.IntrinsicValue - this.ClosePrice) / this.ClosePrice;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.TradingViewSymbol) && !string.IsNullOrEmpty(this.TradingViewCountry))
                {
                    this.PerformLoadFromTradingViewCmd();
                }
            }
        }
    }
}
