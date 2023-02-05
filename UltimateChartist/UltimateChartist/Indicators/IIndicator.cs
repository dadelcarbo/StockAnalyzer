using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string ShortName { get; }
        string DisplayName { get; }
        string Description { get; }
        DisplayType DisplayType { get; }
    }

    /// <summary>
    /// Interface for all one line indicator
    /// </summary>
    public interface ILineIndicator : IIndicator
    {
        ValueSerie Values { get; }
    }

    /// <summary>
    /// Interface for all one line indicator
    /// </summary>
    public interface ISignalLineIndicator : IIndicator
    {
        ValueSerie Values { get; }
        ValueSerie Signal { get; }
    }

    public interface ICloud : IIndicator
    {
        IRangeSerie Range { get; }
    }

    public interface ITrailStop : IIndicator
    {
        IRangeReentrySerie BullTrailStop { get; }

        IRangeReentrySerie BearTrailStop { get; }
    }
}
