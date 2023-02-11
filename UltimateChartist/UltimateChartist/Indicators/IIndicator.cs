using System.ComponentModel;
using Telerik.Windows.Controls.ChartView;
using UltimateChartist.DataModels;

namespace UltimateChartist.Indicators
{
    public enum DisplayType
    {
        Price,
        Ranged,
        NonRanged,
        Volume
    }

    /// <summary>
    /// Base interface for all indicators. It contains mainly meta data
    /// </summary>
    public interface IIndicator
    {
        event PropertyChangedEventHandler ParameterChanged;

        string ShortName { get; }
        string DisplayName { get; }
        string Description { get; }
        DisplayType DisplayType { get; }

        IIndicatorSeries Series { get; }

        void Initialize(StockSerie bars);
    }
}
