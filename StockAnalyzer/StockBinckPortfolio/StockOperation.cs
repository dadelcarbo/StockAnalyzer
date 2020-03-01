using System;
using System.Globalization;
using System.Linq;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockOperation
    {
        public StockOperation(string line)
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("fr-FR");
            var fields = line.Split('\t');

            this.Id = int.Parse(fields[0]);
            this.Date = DateTime.Parse(fields[1], ci);
            this.OperationType = fields[3].ToLower();

            this.Description = fields[4];

            float amount;
            float.TryParse(fields[5].Replace(" ", "").Replace("€", ""), NumberStyles.Float, ci, out amount);
            this.Amount = amount;

            float balance;
            float.TryParse(fields[6].Replace(" ", "").Replace("€", ""), NumberStyles.Float, ci, out balance);
            this.Balance = balance;

            var nameMapping = StockPortfolio.GetMapping(BinckName);
            if (nameMapping == null)
            {
                this.StockName = BinckName;
                this.IsShort = false;
            }
            else
            {
                this.StockName = nameMapping.StockName;
                this.IsShort = nameMapping.Leverage < 0;
            }
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public string OperationType { get; set; }
        public bool IsShort { get; set; }
        public string Description { get; set; }

        public int Qty => this.IsOrder ? int.Parse(new string(this.Description.TakeWhile(c => !Char.IsLetter(c)).ToArray()).Replace(" ", "")) : 0;
        public string BinckName => new string(this.Description.SkipWhile(c => !Char.IsLetter(c)).ToArray()).Replace(" SA", "").ToUpper();
        //public StockNameMapping NameMapping { get; }

        public string StockName { get; }

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
    }
}
