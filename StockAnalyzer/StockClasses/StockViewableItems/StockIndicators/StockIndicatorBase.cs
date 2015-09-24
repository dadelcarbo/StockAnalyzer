using System.Drawing;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public abstract class StockIndicatorBase : Parameterizable, IStockIndicator
   {
      public StockIndicatorBase()
      {
         this.series = new FloatSerie[this.SeriesCount];
         this.serieVisibility = new bool[this.SeriesCount];
         if (EventCount != 0)
         {
            this.eventSeries = new BoolSerie[this.EventCount];
         }
         for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
      }
      abstract public IndicatorDisplayTarget DisplayTarget { get; }

      public ViewableItemType Type { get { return ViewableItemType.Indicator; } }

      public virtual IndicatorDisplayStyle DisplayStyle { get { return IndicatorDisplayStyle.SimpleCurve; } }

      public virtual bool RequiresVolumeData { get { return false; } }

      public string ToThemeString()
      {
         string themeString = "INDICATOR|" + this.Name;
         for (int i = 0; i < this.SeriesCount; i++)
         {
            themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
         }
         return themeString;
      }

      protected FloatSerie[] series;
      public FloatSerie[] Series { get { return series; } }

      abstract public Pen[] SeriePens { get; }
      public virtual HLine[] HorizontalLines
      {
         get { return null; }
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

      abstract public string[] EventNames { get; }
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
