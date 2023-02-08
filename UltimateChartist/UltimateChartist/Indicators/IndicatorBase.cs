using System.Linq;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UltimateChartist.DataModels;
using Telerik.Windows.Controls.ChartView;

namespace UltimateChartist.Indicators
{
    public abstract class IndicatorBase : INotifyPropertyChanged, IIndicator
    {
        public abstract string DisplayName { get; }
        public virtual string Description => DisplayName;
        public string ShortName { get { return this.GetType().Name.Split('_')[1]; } }
        public abstract DisplayType DisplayType { get; }

        public LineSeries LineSeries { get; set; }

        public abstract void Initialize(StockSerie bars);

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                propertyChanged(this, e);
            }
        }
    }
}