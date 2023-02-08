using System.Windows.Media;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public enum EmaType
    {
        EMA,
        MA,
        MID
    }
    public abstract class MovingAverageBase : IndicatorBase
    {
        public MovingAverageBase()
        {
        }

        public override string DisplayName => $"{ShortName}({Period})";

        [IndicatorParameter]
        public int Period { get; set; } = 20;

        public  IndicatorLineSeries Series { get; protected set; } = new IndicatorLineSeries { Brush = new SolidColorBrush(Colors.Red) };

        public abstract void Initialize(StockSerie bars);
    }
}