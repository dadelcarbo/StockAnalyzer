using System.Drawing;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
   public abstract class StockTrailStopBase : Parameterizable, IStockTrailStop
   {
      public StockTrailStopBase()
      {
         this.series = new FloatSerie[this.SeriesCount];
         if (EventCount != 0)
         {
            this.eventSeries = new BoolSerie[this.EventCount];
         }
         this.serieVisibility = new bool[this.SeriesCount];
         for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
      }
      public abstract IndicatorDisplayTarget DisplayTarget { get; }
      public IndicatorDisplayStyle DisplayStyle
      {
         get { return IndicatorDisplayStyle.TrailStop; }
      }
      public ViewableItemType Type { get { return ViewableItemType.TrailStop; } }
      public virtual bool RequiresVolumeData { get { return false; } }

      public string ToThemeString()
      {
         string themeString = "TRAILSTOP|" + this.Name;
         for (int i = 0; i < this.SeriesCount; i++)
         {
            themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
         }
         return themeString;
      }

      protected FloatSerie[] series;
      public FloatSerie[] Series { get { return series; } }

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
