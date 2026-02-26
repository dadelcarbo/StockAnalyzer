using System;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOperation
    {
        public StockOperation() { }

        static public StockOperation FromSimu(long id, DateTime date, string name, string type, int qty, float amount, bool isShort = false)
        {
            var operation = new StockOperation();

            operation.Id = id;
            operation.Date = date;
            operation.OperationType = type;
            operation.IsShort = isShort;

            operation.Qty = qty;
            operation.StockName = name;
            operation.IsShort = isShort;

            operation.Amount = amount;
            operation.Balance = 0;

            return operation;
        }

        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string OperationType { get; set; }


        public string StockName { get; set; }
        public int Qty { get; set; }

        public bool IsShort { get; set; }

        public float Amount { get; set; }
        public float Balance { get; set; }

        public const string BUY = "achat";
        public const string SELL = "vente";

        public string ToFileString()
        {
            // DATE TYPE NAME SHORT QTY AMOUNT
            return $"{Id}\t{Date}\t{OperationType}\t{StockName}\t{IsShort}\t{Qty}\t{Amount}";
        }

        const string SHORT = "Short";
        const string LONG = "Long";
        public override string ToString()
        {
            return $"{Id} {Date} {OperationType} {Qty} {StockName} {Amount} {Balance} {(IsShort ? SHORT : LONG)}";
        }
    }
}
