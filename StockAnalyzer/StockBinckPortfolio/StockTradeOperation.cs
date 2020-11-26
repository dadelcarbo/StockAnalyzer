using System;
using System.Globalization;
using System.Linq;

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
        public int Qty { get; set; }
        public string StockName { get; set; }
        public float Value { get; set; }
        public float Fee { get; set; }

        public override string ToString()
        {
            return $"{Id} {Date} {OperationType} {Qty} {StockName} {Value}";
        }
    }
}
