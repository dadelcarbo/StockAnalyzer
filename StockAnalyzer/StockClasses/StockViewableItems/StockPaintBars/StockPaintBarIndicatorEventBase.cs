using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
   public abstract class StockPaintBarIndicatorEventBase : Parameterizable, IStockPaintBar
   {
      protected IStockIndicator baseIndicator;

      public StockPaintBarIndicatorEventBase()
      {
         baseIndicator = StockIndicatorManager.CreateIndicator(this.ShortName);
         if (baseIndicator == null)
         {
            throw new System.ApplicationException("Unable to create " + this.ShortName + " indicator");
         }
         this.parameters = baseIndicator.ParameterDefaultValues;

         this.serieVisibility = new bool[this.SeriesCount];
         for (int i = 0; i < this.SeriesCount; this.serieVisibility[i++] = false) ;
      }

      public override string Name
      {
         get { return ((IStockViewableSeries)baseIndicator).Name; }
      }

      public IndicatorDisplayTarget DisplayTarget { get { return baseIndicator.DisplayTarget; } }
      public IndicatorDisplayStyle DisplayStyle { get { return IndicatorDisplayStyle.PaintBar; } }
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

      public override string Definition
      {
         get { return this.baseIndicator.Definition; }
      }

      public override string[] ParameterNames
      {
         get { return this.baseIndicator.ParameterNames; }
      }
      public override object[] ParameterDefaultValues
      {
         get
         {
            if (this.baseIndicator == null) return null;
            else return this.baseIndicator.ParameterDefaultValues;
         }
      }
      public override object[] Parameters
      {
         get
         {
            if (this.baseIndicator == null) return null;
            else return this.baseIndicator.Parameters;
         }
      }

      public override ParamRange[] ParameterRanges
      {
         get { return this.baseIndicator.ParameterRanges; }
      }

      public override string[] SerieNames { get { return EventNames; } }

      abstract public Pen[] SeriePens { get; }

      protected bool[] serieVisibility;
      public bool[] SerieVisibility { get { return this.serieVisibility; } }

      public void Initialise(string[] parameters)
      {
         ((Parameterizable)baseIndicator).ParseInputParameters(parameters);
      }

      public void ApplyTo(StockSerie stockSerie)
      {
         this.baseIndicator = stockSerie.GetIndicator(this.Name);

      }

      #region IStockEvent implementation

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
      public string[] EventNames
      {
         get
         {
            if (baseIndicator == null)
            {
               return null;
            }
            else
            { return baseIndicator.EventNames; }
         }
      }

      public BoolSerie[] Events
      {
         get
         {
            if (baseIndicator == null)
            {
               return null;
            }
            else
            { return baseIndicator.Events; }
         }
      }

      public bool[] IsEvent { get; set; }
      #endregion

   }
}
