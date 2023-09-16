using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrails
{
    public abstract class StockTrailBase : Parameterizable, IStockTrail
    {
        public StockTrailBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            this.serieVisibility = new bool[this.SeriesCount];
            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
        }
        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public virtual IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.SimpleCurve; }
        }

        public ViewableItemType Type { get { return ViewableItemType.Trail; } }

        public virtual bool RequiresVolumeData { get { return false; } }

        public string TrailedItem { get; set; }

        public string ToThemeString()
        {
            if (TrailedItem == null)
            {
                throw new System.NullReferenceException("Trailed Item is null, please Initialise");
            }
            string themeString = "TRAIL|" + this.Name + "|" + this.TrailedItem;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            return themeString;
        }

        private bool[] serieVisibility;
        public bool[] SerieVisibility { get { return this.serieVisibility; } }

        public void Initialise(string[] parameters)
        {
            this.ParseInputParameters(parameters);
        }

        abstract public void ApplyTo(StockSerie stockSerie);


        protected FloatSerie[] series;
        public FloatSerie[] Series { get { return series; } }

        abstract public Pen[] SeriePens { get; }
        public virtual HLine[] HorizontalLines
        {
            get { return null; }
        }
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
        public BoolSerie GetEvents(string eventName)
        {
            int index = Array.IndexOf(this.EventNames, eventName);
            return index != -1 ? this.Events[index] : null;
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
