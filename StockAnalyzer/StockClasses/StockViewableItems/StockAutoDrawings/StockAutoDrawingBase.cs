using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public abstract class StockAutoDrawingBase : Parameterizable, IStockAutoDrawing
    {
        public StockAutoDrawingBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
            this.DrawingItems = new StockDrawingItems();
        }

        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.AutoDrawing; }
        }
        public ViewableItemType Type { get { return ViewableItemType.AutoDrawing; } }
        public virtual bool RequiresVolumeData { get { return false; } }

        public string ToThemeString()
        {
            string themeString = "AUTODRAWING|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }

        public StockDrawingItems DrawingItems { get; private set; }

        protected FloatSerie[] series;
        public FloatSerie[] Series { get { return series; } }

        public virtual System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                    seriePens[0].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }
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

        #region IStockText implementation
        protected List<StockText> stockTexts = new List<StockText>();
        public List<StockText> StockTexts => stockTexts;
        #endregion

    }
}
