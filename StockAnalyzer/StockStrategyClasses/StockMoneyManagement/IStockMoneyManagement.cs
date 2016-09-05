using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockStrategyClasses.StockMoneyManagement
{
    public interface IStockMoneyManagement
    {
        string Description { get;  }
        string Name { get;  }

        void Initialise(StockSerie stockSerie);

        void OpenPosition(int size, float value, int index);
        void NextBar();

        float StopLoss { get; }
        float Target { get; }
    }
}