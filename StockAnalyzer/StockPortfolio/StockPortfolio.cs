using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using StockAnalyzer.Portofolio;

namespace StockAnalyzer.StockPortfolio
{
    public class StockPortfolio : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Name of the portfolio
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initial cash
        /// </summary>
        public float InitialCash { get; set; }
        
        private SortedDictionary<string, List<StockPosition>> positions;
        /// <summary>
        /// Dictionnary of positions
        /// </summary>
        public SortedDictionary<string, List<StockPosition>> Positions { get { return positions; } set { if (value != positions) { positions = value; OnPropertyChanged("Positions"); } } }
    }
}
