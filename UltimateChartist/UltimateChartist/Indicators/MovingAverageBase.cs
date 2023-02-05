using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public abstract class MovingAverageBase : IndicatorBase, ILineIndicator
    {
        public static List<string> MaTypes => new List<string> { "EMA", "HMA", "MA", "XMA", "MID" };

        public override string[] ParameterNames => new string[] { "Period" };

        public override object[] ParameterDefaultValues => new object[] { 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 1000) };

        public Brush DefaultBrush = new SolidColorBrush(Colors.Red);


        public ValueSerie Values { get; protected set; }

        public abstract void Initialize(StockSerie bars);
    }
}