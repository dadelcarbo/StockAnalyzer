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
        public override string DisplayName => $"{ShortName}({Period})";

        private int period = 20;
        [IndicatorParameterInt("Period", 1, 500)]
        public int Period { get => period; set { if (period != value) { period = value; RaisePropertyChanged(); } } }

        public IndicatorLineSeries Series { get; protected set; } = new IndicatorLineSeries { Brush = new SolidColorBrush(Colors.Red) };


    }
}