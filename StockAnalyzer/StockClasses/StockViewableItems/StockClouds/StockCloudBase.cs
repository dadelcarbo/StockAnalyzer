using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
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

        private static string[] eventNames = new string[]
          {
             "AboveCloud", "BelowCloud", "InCloud",          // 0,1,2
             "BullishCloud", "BearishCloud"                  // 3, 4
          };

        public string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { false, false, false, false, false };
        public bool[] IsEvent => isEvent;

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
        protected void GenerateEvents(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            for (int i = 5; i < stockSerie.Count; i++)
            {
                var close = closeSerie[i];
                var bullVal = this.BullSerie[i];
                var bearVal = this.BearSerie[i];
                var upBand = Math.Max(bullVal, bearVal);
                var lowBand = Math.Min(bullVal, bearVal);

                if (close > upBand)
                {
                    this.Events[0][i] = true;
                }
                else if (close < lowBand)
                {
                    this.Events[1][i] = true;
                }
                else
                {
                    this.Events[2][i] = true;
                }
                if (bullVal > bearVal)
                {
                    this.Events[3][i] = true;
                }
                else
                {
                    this.Events[4][i] = true;
                }
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
