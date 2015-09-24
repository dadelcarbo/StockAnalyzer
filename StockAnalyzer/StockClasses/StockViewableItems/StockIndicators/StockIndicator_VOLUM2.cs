using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_VOLUM2 : StockIndicatorBase
   {
      public StockIndicator_VOLUM2()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override bool RequiresVolumeData { get { return true; } }

      public override string Name
      {
         get { return "VOLUM2(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
      }
      public override string Definition
      {
         get { return "VOLUM2(int FastPeriod, int SmoothPeriod, int SignalPeriod)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "FastPeriod", "SmoothPeriod", "SignalPeriod" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, 3, 3 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] SerieNames { get { return new string[] { "VOL(" + this.Parameters[0].ToString() + ")", "SIGNAL(" + this.Parameters[1].ToString() + ")" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.DarkGreen, 2), new Pen(Color.DarkRed, 2) };
            }
            return seriePens;
         }
      }
      static HLine[] lines = null;
      public override HLine[] HorizontalLines
      {
         get
         {
            if (lines == null)
            {
               lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
               lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }
            return lines;
         }
      }
      public override void ApplyTo(StockSerie stockSerie)
      {

         FloatSerie upVolume = stockSerie.GetSerie(StockDataType.UPVOLUME).Sqrt();
         FloatSerie downVolume = stockSerie.GetSerie(StockDataType.DOWNVOLUME).Sqrt();
         FloatSerie cumulVolume = upVolume.Cumul() - downVolume.Cumul();
         FloatSerie diffVolume = (cumulVolume.CalculateEMA((int)this.parameters[1]) - cumulVolume.CalculateEMA((int)this.parameters[0])).CalculateEMA((int)this.parameters[1]);
         FloatSerie fastSerie = diffVolume;
         FloatSerie slowSerie = fastSerie.CalculateEMA(((int)this.parameters[2]));

         this.Series[0] = fastSerie;
         this.Series[1] = slowSerie;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 2; i < stockSerie.Count; i++)
         {
            if (fastSerie[i] > slowSerie[i])
            {
               this.Events[2][i] = true;

               if (fastSerie[i - 1] < slowSerie[i - 1])
               {
                  this.Events[0][i] = true;
               }
            }
            else
            {
               this.Events[3][i] = true;

               if (fastSerie[i - 1] > slowSerie[i - 1])
               {
                  this.Events[1][i] = true;
               }
            }
         }
      }

      static readonly string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "UpTrend", "DownTrend" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
