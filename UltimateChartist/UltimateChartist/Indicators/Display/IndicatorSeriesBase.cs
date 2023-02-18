using System.Collections.Generic;
using Telerik.Windows.Controls;

namespace UltimateChartist.Indicators.Display;

public abstract class IndicatorSeriesBase : ViewModelBase, IIndicatorSeries
{
    private IEnumerable<IndicatorValueBase> values;
    public IEnumerable<IndicatorValueBase> Values { get => values; set { if (values != value) { values = value; RaisePropertyChanged(); } } }
}
