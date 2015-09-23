using System.Drawing;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public abstract class StockPaintBarBase : Parameterizable, IStockPaintBar
    {
        public StockPaintBarBase()
        {
            this.eventSeries = new BoolSerie[this.SeriesCount];
            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
        }
        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.PaintBar; }
        }
        public ViewableItemType Type { get { return ViewableItemType.PaintBar; } }

        public virtual bool RequiresVolumeData { get { return false; } }

        public string ToThemeString()
        {
            string themeString = "PAINTBAR|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }

        abstract public Pen[] SeriePens { get; }

        private bool[] serieVisibility;
        public bool[] SerieVisibility { get { return this.serieVisibility; } }

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

        public abstract string[] EventNames { get; }
        public override string[] SerieNames { get { return EventNames; } }
        abstract public bool[] IsEvent { get; }

        public BoolSerie[] Events
        {
            get { return eventSeries; }
        }
        virtual protected void CreateEventSeries(int count)
        {
            for (int i = 0; i < this.EventCount; i++)
            {
                this.eventSeries[i] = new BoolSerie(count, this.EventNames[i]);
            }
        }
        #endregion

    }
}
