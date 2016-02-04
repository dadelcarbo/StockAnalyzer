using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_ICHIMOKU : StockIndicatorBase
   {
      public StockIndicator_ICHIMOKU()
      {
      }

      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      
      public override object[] ParameterDefaultValues
      {
         get { return new Object[] {9, 26, 52}; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "TenkanSen", "KijunSen", "SenkouSpanB" }; }
      }

      public override string[] SerieNames { get { return new string[] { "TenkanSen", "KijunSen", "SenkouSpanA", "SenkouSpanB" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

         FloatSerie TenkanSenSerie = new FloatSerie(highSerie.Count, "TenkanSen"); //  (highest high + lowest low)/2 for the last 9 periods.
         int period = (int) this.parameters[0];
         float highest = float.MinValue;
         float lowest = float.MaxValue;
         for (int i = 0; i < highSerie.Count; i++)
         {
            if (i < period)
            {
               highest = highSerie.GetMax(0, i);
               lowest = lowSerie.GetMin(0, i);
            }
            else
            {
               highest = highSerie.GetMax(i - period, i);
               lowest = lowSerie.GetMin(i - period, i);
            }
            TenkanSenSerie[i] = (highest + lowest) / 2f;
         }
         this.series[0] = TenkanSenSerie;

         FloatSerie KijunSenSerie = new FloatSerie(highSerie.Count, "KijunSen"); // (highest high + lowest low)/2 for the past 26 periods.
         period = (int)this.parameters[1];
         for (int i = 0; i < highSerie.Count; i++)
         {
            if (i < period)
            {
               highest = highSerie.GetMax(0, i);
               lowest = lowSerie.GetMin(0, i);
            }
            else
            {
               highest = highSerie.GetMax(i - period, i);
               lowest = lowSerie.GetMin(i - period, i);
            }
            KijunSenSerie[i] = (highest + lowest) / 2f;
         }
         this.series[1] = KijunSenSerie;

         FloatSerie SenkouSpanA = (TenkanSenSerie + KijunSenSerie)/2.0f;
         SenkouSpanA.Name = "SenkouSpanA";
         
         this.series[2] = SenkouSpanA;

         FloatSerie SenkouSpanB = new FloatSerie(highSerie.Count, "SenkouSpanB"); // (highest high + lowest low)/2 calculated over the past 52 time periods and plotted 26 periods ahead
         FloatSerie tmpSerie = new FloatSerie(highSerie.Count, "tmp");
         period = (int)this.parameters[2];
         for (int i = 0; i < highSerie.Count; i++)
         {
            if (i < period)
            {
               highest = highSerie.GetMax(0, i);
               lowest = lowSerie.GetMin(0, i);
            }
            else
            {
               highest = highSerie.GetMax(i - period, i);
               lowest = lowSerie.GetMin(i - period, i);
            }
            tmpSerie[i] = (highest + lowest) / 2f;
         }
         period = 26;
         for (int i = period; i < highSerie.Count; i++)
         {
            SenkouSpanB[i] = tmpSerie[i - period];
         }
         for (int i = 0; i < period; i++)
         {
            SenkouSpanB[i] = SenkouSpanA[period];
         }
         this.series[3] = SenkouSpanB;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

      }

      static string[] eventNames = new string[] { "AboveCloud", "InCloud", "BelowCloud" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { false, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
