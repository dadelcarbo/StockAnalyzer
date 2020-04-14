using System;
using System.Globalization;
using System.Linq;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockOperation
    {
        private StockOperation() { }
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

        static public StockOperation FromSimuLine(string line)
        {
            // DATE TYPE NAME SHORT QTY AMOUNT
            var operation = new StockOperation();
            var fields = line.Split('\t');

            operation.Id = 0;
            operation.Date = DateTime.Parse(fields[0]);
            operation.OperationType = fields[1].ToLower();

            operation.Description = fields[4] + " " + fields[2];
            operation.BinckName = fields[2];
            operation.StockName = fields[2];
            operation.IsShort = bool.Parse(fields[3]);

            float amount;
            float.TryParse(fields[5], out amount);
            operation.Amount = amount;

            float balance = 0;
            operation.Balance = balance;

            return operation;
        }

        public static int OperationId = 0;
        static public StockOperation FromSimu(DateTime date, string name, string type, int qty, float amount, bool isShort = false)
        {
            var operation = new StockOperation();

            operation.Id = OperationId++;
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
        public bool IsShort { get; set; }

        public StockNameMapping NameMapping { get; set; }

        public bool IsProduct => NameMapping != null && NameMapping.Leverage != 1;

        public string Description { get; set; }

        public int Qty => this.IsOrder ? int.Parse(new string(this.Description.TakeWhile(c => !Char.IsLetter(c)).ToArray()).Replace(" ", "")) : 0;
        public string BinckName { get; set; }
        //public StockNameMapping NameMapping { get; }

        public string StockName { get; set; }
        public float Amount { get; set; }
        public float Balance { get; set; }
        public bool IsOrder => this.OperationType == BUY || this.OperationType == SELL || this.OperationType == DEPOSIT || this.OperationType == TRANSFER;

        public const string BUY = "achat";
        public const string SELL = "vente";
        public const string DEPOSIT = "dépôt";
        public const string TRANSFER = "transfert de titres";

        internal void Dump()
        {
            Console.WriteLine($"{Id} {Date} {OperationType} {Description} {Amount} {Balance}");
        }

        internal string ToFileString()
        {
            // DATE TYPE NAME SHORT QTY AMOUNT
            return $"{Date}\t{OperationType}\t{StockName}\t{IsShort}\t{Qty}\t{Amount}";
        }
    }
}
