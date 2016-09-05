using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockStrategyClasses.StockMoneyManagement
{
    public abstract class StockMoneyManagementBase : IStockMoneyManagement
    {
        public abstract string Description { get; }

        public abstract string Name { get; }

        protected StockSerie stockSerie;
        protected FloatSerie lowSerie;
        protected FloatSerie highSerie;
        protected FloatSerie closeSerie;

        public void Initialise(StockSerie serie)
        {
            this.stockSerie = serie;
            this.lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            this.highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            this.closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
        }

        protected float currentStopValue;
        protected float currentTargetValue;
        protected int positionSize;
        protected float positionOpen;
        protected int index;

        public abstract void OpenPosition(int size, float value, int index);

        public virtual void NextBar()
        {
            index++;
        }

        public float StopLoss
        {
            get { return currentStopValue; }
        }

        public float Target
        {
            get { return currentTargetValue; }
        }
    }
}
