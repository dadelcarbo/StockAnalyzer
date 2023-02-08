using System.Linq;
using System.Globalization;

namespace UltimateChartist.Indicators
{
    public abstract class IndicatorBase : IIndicator
    {
        public abstract string DisplayName { get; }
        public virtual string Description => DisplayName;
        public string ShortName { get { return this.GetType().Name.Split('_')[1]; } }
        public abstract DisplayType DisplayType { get; }
    }
}