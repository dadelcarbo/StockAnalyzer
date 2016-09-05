using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockStrategyClasses.StockMoneyManagement
{
    public class StockPreviousLowRiskFree : StockMoneyManagementBase
    {
        public override string Description
        {
            get { return "Sets a stop loss at previous low in the defined period"; }
        }

        public override string Name
        {
            get { return "StockPreviousLowRiskFree"; }
        }

        public override void OpenPosition(int size, float value, int index)
        {
            this.currentStopValue = lowSerie.GetMin(Math.Max(0, index - 9), index);
            this.currentTargetValue = value + value - this.currentStopValue;

            this.positionSize = size;
            this.positionOpen = value;
            this.index = index;
        }
    }
}
