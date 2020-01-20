using StockAnalyzer.StockClasses;
using StockAnalyzer.StockBinckPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class StockPositionViewModel
    {
        StockPosition position;
        public StockPositionViewModel(StockPosition p)
        {
            this.position = p;
        }
        public bool IsValidName => StockDictionary.StockDictionarySingleton.ContainsKey(position.StockName);
        public string StockName => position.StockName;
        public int Qty => position.Qty;
        public float OpenValue => position.OpenValue;
        public DateTime StartDate => position.StartDate;
    }
}
