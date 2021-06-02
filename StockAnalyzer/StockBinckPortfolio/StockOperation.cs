using System;
using System.Globalization;
using System.Linq;

namespace StockAnalyzer.StockBinckPortfolio
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

            operation.Description = $"{qty} {name}";
            operation.BinckName = name;
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
        public string Description { get; set; }
        public int Qty => this.IsOrder ? int.Parse(new string(this.Description.TakeWhile(c => !Char.IsLetter(c)).ToArray()).Replace(" ", "")) : 0;






        public bool IsShort { get; set; }

        public StockNameMapping NameMapping { get; set; }

        public bool IsProduct => NameMapping != null && NameMapping.Leverage != 1;
        public string BinckName { get; set; }
        //public StockNameMapping NameMapping { get; }

        public float Amount { get; set; }
        public float Balance { get; set; }
        public bool IsOrder => this.OperationType == BUY || this.OperationType == SELL || this.OperationType == DEPOSIT || this.OperationType == TRANSFER;

        public const string BUY = "achat";
        public const string SELL = "vente";
        public const string DEPOSIT = "dépôt";
        public const string TRANSFER = "transfert de titres";

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
