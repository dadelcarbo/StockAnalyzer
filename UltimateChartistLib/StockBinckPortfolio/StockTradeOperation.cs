using System;

namespace StockAnalyzer.StockBinckPortfolio
{
    public enum TradeOperationType
    {
        Buy,
        Sell,
        Short,
        Cover,
        Dividend,
        Transfer
    }
    public class StockTradeOperation
    {
        public StockTradeOperation() { }

        private StockPortfolio portfolio;
        public StockTradeOperation(StockPortfolio portfolio)
        {
            this.portfolio = portfolio;
            this.Id = this.portfolio.TradeOperations.Count;
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TradeOperationType OperationType { get; set; }
        public string StockName { get; set; }
        public int Qty { get; set; }
        public float Value { get; set; }
        public float Fee { get; set; }
        public bool IsOrder => this.OperationType == TradeOperationType.Buy || this.OperationType == TradeOperationType.Sell || this.IsShort;

        public bool IsShort => this.OperationType == TradeOperationType.Short || this.OperationType == TradeOperationType.Cover;

        public override string ToString()
        {
            return $"{Id} {Date} {OperationType} {Qty} {StockName} {Value}";
        }
    }
}
