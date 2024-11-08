using System;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertValue
    {
        public StockSerie StockSerie { get; set; }
        public StockAlertDef AlertDef { get; set; }
        public DateTime Date { get; set; }
        public float Value { get; set; }
        public float Variation { get; set; }
        public float Exchanged { get; set; }

        public float Speed { get; set; }
        public string SpeedFormat { get; set; }

        public float Stok { get; set; }

        public float TrailStop { get; set; }

        public int Highest { get; set; }
    }
}
