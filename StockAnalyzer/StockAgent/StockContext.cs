using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent
{
    public enum StokPositionStatus
    {
        Opened,
        Closed
    }

    public class StockContext
    {
        public int CurrentIndex { get; set; }
        public StockSerie Serie {get; set;}

        public StokPositionStatus PositionStatus { get; set; }
        public int OpenIndex { get; set; }
        public float OpenValue { get; set; }

        public StockContext()
        {
            this.PositionStatus = StokPositionStatus.Closed;
        }
    }
}
