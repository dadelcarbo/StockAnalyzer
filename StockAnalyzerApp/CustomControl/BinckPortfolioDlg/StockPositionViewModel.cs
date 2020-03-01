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
        public bool IsValidName
        {
            get
            {
                var mapping = StockPortfolio.GetMapping(StockName);
                if (mapping == null)
                {
                    return StockDictionary.StockDictionarySingleton.ContainsKey(position.StockName);
                }
                return StockDictionary.StockDictionarySingleton.ContainsKey(mapping.StockName);
            }
        }
        public string StockName => position.StockName;
        public int Qty => position.Qty;
        public float OpenValue => position.OpenValue;
        public DateTime StartDate => position.StartDate;
        public float LastValue { get; set; }
    }
}
