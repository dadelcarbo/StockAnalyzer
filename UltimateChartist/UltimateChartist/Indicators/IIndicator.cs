using System.ComponentModel;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

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

    decimal Max { get; }

    IIndicatorSeries Series { get; }

    void Initialize(StockSerie stockSerie);
}

public interface IRangedIndicator
{
    decimal Minimum { get; }
    decimal Maximum { get; }
}
