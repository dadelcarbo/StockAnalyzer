using System.ComponentModel;
using System.Runtime.CompilerServices;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public abstract class IndicatorBase : IIndicator
{
    public abstract string DisplayName { get; }
    public virtual string Description => DisplayName;
    public string ShortName { get { return this.GetType().Name.Split('_')[1]; } }
    public abstract DisplayType DisplayType { get; }

    public decimal Max { get; protected set; }

    /// <summary>
    /// Contains the indicator calculated data 
    /// </summary>
    public IIndicatorSeries Series { get; set; }

    public abstract void Initialize(StockSerie stockSerie);

    public event PropertyChangedEventHandler ParameterChanged;
    protected internal void RaiseParameterChanged([CallerMemberName] string propertyName = null)
    {
        if (this.ParameterChanged != null)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            this.ParameterChanged(this, e);
        }
    }

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
