using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{
    public enum BuySell
    {
        Buy,
        Sell
    }

    public class TradeRequest
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public StockSerie StockSerie { get; set; }
        public BuySell BuySell { get; set; }
        public int Qty { get; set; }
        public float Value { get; set; }
        public float Stop { get; set; }

        public override string ToString()
        {
            return $"{BuySell} {Qty}@{Value}";
        }
    }
}
