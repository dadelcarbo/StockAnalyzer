using System.Drawing;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
   public abstract class StockDecoratorBase : Parameterizable, IStockDecorator, IStockEvent
   {
      public StockDecoratorBase()
      {
         this.series = new BoolSerie[this.SeriesCount];
         this.serieVisibility = new bool[this.SeriesCount];
         for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = true) ;
      }
      public abstract IndicatorDisplayTarget DisplayTarget { get; }
      public virtual IndicatorDisplayStyle DisplayStyle
      {
         get { return IndicatorDisplayStyle.DecoratorPlot; }
      }

      public ViewableItemType Type { get { return ViewableItemType.Decorator; } }

      public virtual bool RequiresVolumeData { get { return false; } }

      public string DecoratedItem { get; set; }

      public string ToThemeString()
      {
         if (DecoratedItem == null)
         {
            throw new System.NullReferenceException("Decorated Item is null, please Initialise");
         }
         string themeString = "DECORATOR|" + this.Name + "|" + this.DecoratedItem;
         for (int i = 0; i < this.SeriesCount; i++)
         {
            themeString += "|" + GraphCurveType.PenToString(this.SeriePens[i]) + "|" + this.SerieVisibility[i].ToString();
         }
         return themeString;
      }

      protected BoolSerie[] series;
      public BoolSerie[] Series { get { return series; } }

      abstract public Pen[] SeriePens { get; }

      private bool[] serieVisibility;
      public bool[] SerieVisibility { get { return this.serieVisibility; } }

      public void Initialise(string[] parameters)
      {
         this.ParseInputParameters(parameters);
      }

      abstract public void ApplyTo(StockSerie stockSerie);

      protected BoolSerie[] eventSeries;
      public int EventCount
      {
         get { return EventNames.Length; }
      }

      abstract public string[] EventNames { get; }

      public BoolSerie[] Events
      {
         get { return eventSeries; }
      }

      abstract public bool[] IsEvent { get; }
   }
}
