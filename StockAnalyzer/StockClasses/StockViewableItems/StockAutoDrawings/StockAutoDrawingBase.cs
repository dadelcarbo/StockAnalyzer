using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public abstract class StockAutoDrawingBase : Parameterizable, IStockAutoDrawing
    {
        public StockAutoDrawingBase()
        {
            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            this.series = new FloatSerie[this.SeriesCount];
            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
            this.DrawingItems = new StockDrawingItems();
        }

        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.AutoDrawing;
        public ViewableItemType Type => ViewableItemType.AutoDrawing;
        public virtual bool RequiresVolumeData => false;

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
        public FloatSerie[] Series => series;
        public override string[] SerieNames => new string[] { };

        public virtual System.Drawing.Pen[] SeriePens => new Pen[] { };

        private readonly bool[] serieVisibility;
        public bool[] SerieVisibility => this.serieVisibility;

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
        abstract public bool[] IsEvent { get; }

        public BoolSerie[] Events => eventSeries;
        virtual protected void CreateEventSeries(int count)
        {
            for (int i = 0; i < this.EventCount; i++)
            {
                this.eventSeries[i] = new BoolSerie(count, this.EventNames[i]);
            }
        }
        public BoolSerie GetEvents(string eventName)
        {
            int index = Array.IndexOf(this.EventNames, eventName);
            return index != -1 ? this.Events[index] : null;
        }
        #endregion

        #region IStockText implementation
        protected List<StockText> stockTexts = new List<StockText>();
        public List<StockText> StockTexts => stockTexts;

        #endregion

    }
}
