using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public abstract class StockCloudBase : Parameterizable, IStockCloud
    {
        protected StockCloudBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            this.SerieVisibility = new bool[this.SeriesCount];
            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            for (int i = 0; i < this.SeriesCount; this.SerieVisibility[i++] = true) ;
        }
        abstract public IndicatorDisplayTarget DisplayTarget { get; }

        public ViewableItemType Type { get { return ViewableItemType.Indicator; } }

        public virtual IndicatorDisplayStyle DisplayStyle { get { return IndicatorDisplayStyle.SimpleCurve; } }

        public virtual bool RequiresVolumeData { get { return false; } }

        public string ToThemeString()
        {
            string themeString = "CLOUD|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }

        protected FloatSerie[] series;
        public FloatSerie[] Series { get { return series; } }

        abstract public Pen[] SeriePens { get; }
        public bool[] SerieVisibility { get; }

        public void Initialise(string[] parameters)
        {
            this.ParseInputParameters(parameters);
        }

        abstract public void ApplyTo(StockSerie stockSerie);

        #region IStockEvent implementation
        protected BoolSerie[] eventSeries;
        public int EventCount
        {
            get
            {
                if (EventNames != null)
                {
                    return EventNames.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        abstract public string[] EventNames { get; }
        abstract public bool[] IsEvent { get; }

        public BoolSerie[] Events
        {
            get { return eventSeries; }
        }

        public FloatSerie BullSerie => this.Series[0];

        public FloatSerie BearSerie => this.Series[1];

        virtual protected void CreateEventSeries(int count)
        {
            for (int i = 0; i < this.EventCount; i++)
            {
                this.eventSeries[i] = new BoolSerie(count, this.EventNames[i]);
            }
        }
        virtual protected void SetSerieNames()
        {
            for (int i = 0; i < this.SerieNames.Length; i++)
            {
                this.Series[i].Name = this.SerieNames[i];
            }
        }
        #endregion

    }
}
