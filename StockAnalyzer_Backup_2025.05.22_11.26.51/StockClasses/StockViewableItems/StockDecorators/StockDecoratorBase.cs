using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public abstract class StockDecoratorBase : Parameterizable, IStockDecorator, IStockEvent
    {
        public StockDecoratorBase()
        {
            this.series = new FloatSerie[this.SeriesCount];
            this.serieVisibility = new bool[this.SeriesCount];
            for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;

            this.eventSeries = new BoolSerie[this.EventCount];
            this.eventVisibility = new bool[this.EventCount];
            for (int i = 0; i < this.EventCount; this.eventVisibility[i++] = true) ;
        }
        public abstract IndicatorDisplayTarget DisplayTarget { get; }
        public virtual IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public ViewableItemType Type => ViewableItemType.Decorator;

        public virtual bool RequiresVolumeData => false;

        public string DecoratedItem { get; set; }

        public string ToThemeString()
        {
            if (DecoratedItem == null)
            {
                throw new NullReferenceException("Decorated Item is null, please Initialise");
            }
            string themeString = "DECORATOR|" + this.Name + "|" + this.DecoratedItem;
            for (int i = 0; i < this.SeriesCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
            }
            for (int i = 0; i < this.EventCount; i++)
            {
                themeString += "|" + GraphCurveType.PenToString(this.EventPens[i]) + "|" + this.EventVisibility[i].ToString();
            }
            return themeString;
        }

        protected FloatSerie[] series;
        public FloatSerie[] Series => series;

        abstract public Pen[] SeriePens { get; }

        private readonly bool[] serieVisibility;
        public bool[] SerieVisibility => this.serieVisibility;

        public void Initialise(string[] parameters)
        {
            this.ParseInputParameters(parameters);
        }

        abstract public void ApplyTo(StockSerie stockSerie);

        protected BoolSerie[] eventSeries;
        public int EventCount => EventNames.Length;

        abstract public string[] EventNames { get; }

        public BoolSerie[] Events => eventSeries;

        virtual protected void CreateEventSeries(int count)
        {
            for (int i = 0; i < this.EventCount; i++)
            {
                this.eventSeries[i] = new BoolSerie(count, this.EventNames[i]);
            }
        }

        abstract public bool[] IsEvent { get; }


        private readonly bool[] eventVisibility;
        public bool[] EventVisibility => this.eventVisibility;

        protected Pen[] eventPens = null;
        abstract public Pen[] EventPens { get; }
        public BoolSerie GetEvents(string eventName)
        {
            int index = Array.IndexOf(this.EventNames, eventName);
            return index != -1 ? this.Events[index] : null;
        }
    }
}
