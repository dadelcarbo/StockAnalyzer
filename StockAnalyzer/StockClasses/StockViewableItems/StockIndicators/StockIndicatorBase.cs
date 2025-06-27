using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public abstract class StockIndicatorBase : Parameterizable, IStockIndicator, IStockText
    {
        protected StockIndicatorBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            this.serieVisibility = new bool[this.SeriesCount];

            if (EventCount != 0)
            {
                this.eventSeries = new BoolSerie[this.EventCount];
            }
            for (int i = 0; i < this.SeriesCount; i++)
            {
                this.serieVisibility[i] = true;
            }
        }
        abstract public IndicatorDisplayTarget DisplayTarget { get; }

        public ViewableItemType Type => ViewableItemType.Indicator;

        public virtual IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public virtual bool RequiresVolumeData => false;

        public string ToThemeString()
        {
            string themeString = "INDICATOR|" + this.Name;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            if (this.Areas != null)
            {
                foreach (var area in this.Areas)
                {
                    themeString += "|" + GraphCurveType.ColorToString(area.Color) + "|" + area.Visibility.ToString();
                }
            }
            return themeString;
        }

        protected string[] serieFormats = null;
        virtual public string[] SerieFormats => serieFormats;

        protected FloatSerie[] series;
        public FloatSerie[] Series => series;

        abstract public Pen[] SeriePens { get; }
        public virtual HLine[] HorizontalLines => null;

        protected Area[] areas;
        public virtual Area[] Areas => areas;

        private readonly bool[] serieVisibility;
        public bool[] SerieVisibility => this.serieVisibility;

        public void Initialise(string[] parameters)
        {
            this.ParseInputParameters(parameters);
        }
        public FloatSerie GetSerie(string name)
        {
            int index = Array.IndexOf(this.SerieNames, name);
            return index != -1 ? this.Series[index] : null;
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

        public BoolSerie[] Events => eventSeries;
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
        virtual protected void SetSerieNames()
        {
            for (int i = 0; i < this.SerieNames.Length; i++)
            {
                this.Series[i].Name = this.SerieNames[i];
            }
        }
        #endregion

        #region IStockText implementation
        protected List<StockText> stockTexts = new List<StockText>();
        public List<StockText> StockTexts => stockTexts;
        #endregion
    }
}
