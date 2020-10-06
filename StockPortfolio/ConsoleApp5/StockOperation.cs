using System;
using System.Globalization;
using System.Linq;

namespace ConsoleApp5
{
    public class StockOperation
    {
        public StockOperation(string line)
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("fr-FR");
            var fields = line.Split('\t');

            this.Id = int.Parse(fields[0]);
            this.Date = DateTime.Parse(fields[1], ci);
            this.OperationType = fields[3];

            this.Description = fields[4];

            float amount;
            float.TryParse(fields[5].Replace(" ", "").Replace("�", ""), NumberStyles.Float, ci, out amount);
            this.Amount = amount;

            float balance;
            float.TryParse(fields[6].Replace(" ", "").Replace("�", ""), NumberStyles.Float, ci, out balance);
            this.Balance = balance;
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public string OperationType { get; set; }
        public string Description { get; set; }

        public int Qty => int.Parse(new string(this.Description.TakeWhile(c => !Char.IsLetter(c)).ToArray()).Replace(" ", ""));
        public string StockName => new string(this.Description.SkipWhile(c => !Char.IsLetter(c)).ToArray());

        public float Amount { get; set; }
        public float Balance { get; set; }


        internal void Dump()
        {
            Console.WriteLine($"{Id} {Date} {OperationType} {Description} {Amount} {Balance}");
        }
    }
}
