using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using System;
using System.ComponentModel;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public class StockPositionBaseViewModel : INotifyPropertyChanged
    {
        #region Notify Property Changed and Dirty management
        public void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            portfolio.IsDirty = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        readonly PortfolioViewModel portfolio;
        readonly StockPositionBase position;
        public StockPositionBaseViewModel(StockPositionBase pos, PortfolioViewModel portfolio)
        {
            this.position = pos;
            this.portfolio = portfolio;
            float value = StockPortfolio.PriceProvider.GetLastClosingPrice(pos.StockName);
            if (value == 0.0f) // if price is not found use open price
            {
                this.LastValue = pos.EntryValue;
            }
            else
            {
                this.LastValue = value;
            }

            if (!StockDictionary.Instance.ContainsKey(position.StockName))
            {
                var mapping = StockPortfolio.GetMapping(position.StockName, position.ISIN);
                if (mapping != null)
                {
                    position.StockName = mapping.StockName;
                }
            }
        }
        public bool IsValidName
        {
            get
            {
                var mapping = StockPortfolio.GetMapping(StockName, position.ISIN);
                if (mapping == null)
                {
                    return StockDictionary.Instance.ContainsKey(position.StockName);
                }
                return StockDictionary.Instance.ContainsKey(mapping.StockName);
            }
        }

        public long Id => position.Id;
        public string StockName => position.StockName;
        public string ISIN => position.ISIN;
        #region TRADE ENTRY
        public DateTime EntryDate => position.EntryDate;
        public int EntryQty => position.EntryQty;
        public float EntryValue => Math.Abs(position.EntryValue);
        public float EntryCost => position.EntryCost;
        public string EntryComment
        {
            get => position.EntryComment;
            set { position.EntryComment = value; }
        }
        #endregion


        public float Stop { get { return position.Stop; } set { if (position.Stop != value) { position.Stop = value; OnPropertyChanged("Stop"); OnPropertyChanged("TradeRisk"); OnPropertyChanged("PortfolioRisk"); } } }
        public float TrailStop { get { return position.TrailStop; } set { if (position.TrailStop != value) { position.TrailStop = value; OnPropertyChanged("TrailStop"); OnPropertyChanged("TradeRisk"); OnPropertyChanged("PortfolioRisk"); } } }

        public float TradeRisk => position.TrailStop == 0 ? 1.0f : (position.TrailStop - position.EntryValue) / position.EntryValue;
        public float PortfolioRisk => PortfolioPercent * (position.TrailStop - position.EntryValue) / position.EntryValue;

        public BarDuration BarDuration { get { return position.BarDuration; } set { if (position.BarDuration != value) { position.BarDuration = value; OnPropertyChanged("BarDuration"); } } }
        public string Theme { get { return position.Theme; } set { if (position.Theme != value) { position.Theme = value; OnPropertyChanged("Theme"); } } }

        public DateTime? ExitDate => position.ExitDate;
        public float? ExitValue => position.ExitValue;

        public float? Return => ExitValue == null ? null : (ExitValue - EntryValue) / EntryValue;
        public float? RiskRewardRatio
        {
            get
            {
                if (Stop == 0.0f)
                    return null;
                if (this.position.IsClosed)
                    return (ExitValue - EntryValue) / (EntryValue - Stop);
                else
                    return (LastValue - EntryValue) / (EntryValue - Stop);
            }
        }

        public float LastValue { get; set; }
        public float Variation => (LastValue - EntryValue) / EntryValue;
        public float PortfolioVariation => PortfolioPercent * Variation;
        // @@@@ Need to use the most accurate portfolio value (Position value or Risk free value ?
        public float PortfolioPercent => this.portfolio.Value > 0 ? EntryValue * this.EntryQty / this.portfolio.Value : 0.0f;
    }
}
