using System;
using System.Globalization;
using System.Linq;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockOperation
    {
        public StockOperation() { }
        static public StockOperation FromBinckLine(string line)
        {
            var operation = new StockOperation();
            CultureInfo ci = CultureInfo.GetCultureInfo("fr-FR");
            var fields = line.Split('\t');

            operation.Id = int.Parse(fields[0]);
            operation.Date = DateTime.Parse(fields[1], ci);
            operation.OperationType = fields[3].ToLower();

            operation.Description = fields[4];
            operation.BinckName = new string(operation.Description.SkipWhile(c => !Char.IsLetter(c)).ToArray()).Replace(" SA", "").ToUpper();
            operation.NameMapping = StockPortfolio.GetMapping(operation.BinckName);
            operation.IsShort = operation.NameMapping == null ? false : operation.NameMapping.Leverage < 0;
            operation.StockName = operation.NameMapping == null ? operation.BinckName : operation.NameMapping.StockName;

            float amount;
            float.TryParse(fields[5].Replace(" ", "").Replace("€", ""), NumberStyles.Float, ci, out amount);
            operation.Amount = amount;

            float balance;
            float.TryParse(fields[6].Replace(" ", "").Replace("€", ""), NumberStyles.Float, ci, out balance);
            operation.Balance = balance;

            return operation;
        }

        static public StockOperation FromSimuLine(bool hasId, int id, string line)
        {
            // DATE TYPE NAME SHORT QTY AMOUNT
            var operation = new StockOperation();
            var fields = line.Split('\t');

            int idOffset = hasId ? 1: 0;

            operation.Id = hasId ? int.Parse(fields[0]) : id;
            operation.Date = DateTime.Parse(fields[0 + idOffset]);
            operation.OperationType = fields[1 + idOffset].ToLower();

            operation.Description = fields[4 + idOffset] + " " + fields[2 + idOffset];
            operation.BinckName = fields[2 + idOffset];
            operation.StockName = fields[2 + idOffset];
            operation.IsShort = bool.Parse(fields[3 + idOffset]);

            float amount;
            float.TryParse(fields[5 + idOffset], out amount);
            operation.Amount = amount;

            float balance = 0;
            operation.Balance = balance;

            return operation;
        }

        static public StockOperation FromSimu(int id, DateTime date, string name, string type, int qty, float amount, bool isShort = false)
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

        public int Id { get; set; }
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
