using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_SAR2 : StockIndicatorBase
   {
      public StockIndicator_SAR2()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      public override IndicatorDisplayStyle DisplayStyle
      {
         get
         {
            return IndicatorDisplayStyle.SupportResistance;
         }
      }
      public override string Definition
      {
         get { return "SAR2(float step, float maximum, float ratio)"; }
      }

      public override string[] ParameterNames
      {
         get { return new string[] { "Step", "Max", "Ratio" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 0.02f, 0.2f, 5.0f }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeFloat(0.0001f, 10.0f), new ParamRangeFloat(0.001f, 10.0f), new ParamRangeFloat(0.001f, 100.0f) }; }
      }

      public override string[] SerieNames { get { return new string[] { "SAR1.S", "SAR1.R", "SAR2.S", "SAR2.R" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2), new Pen(Color.Green, 3), new Pen(Color.Red, 3) };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         float accelerationFactorStep = (float)this.parameters[0];
         float accelerationFactorMax = (float)this.parameters[1];

         FloatSerie sarSupport1;
         FloatSerie sarResistance1;

         FloatSerie sarSupport2;
         FloatSerie sarResistance2;

         float ratio = (float)this.parameters[2];

         stockSerie.CalculateSAR(accelerationFactorStep, accelerationFactorStep, accelerationFactorMax, out sarSupport1, out sarResistance1, 1);
         stockSerie.CalculateSAR2(accelerationFactorStep / ratio, accelerationFactorStep / ratio, accelerationFactorMax / ratio, sarSupport1, sarResistance1, out sarSupport2, out sarResistance2);

         this.Series[0] = sarSupport1;
         this.Series[1] = sarResistance1;

         this.Series[2] = sarSupport2;
         this.Series[3] = sarResistance2;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 5; i < stockSerie.Count; i++)
         {
            this.Events[0][i] = float.IsNaN(sarResistance1[i - 1]) && !float.IsNaN(sarResistance1[i]);
            this.Events[1][i] = float.IsNaN(sarSupport1[i - 1]) && !float.IsNaN(sarSupport1[i]);

            this.Events[2][i] = float.IsNaN(sarResistance2[i - 1]) && !float.IsNaN(sarResistance2[i]);
            this.Events[3][i] = float.IsNaN(sarSupport2[i - 1]) && !float.IsNaN(sarSupport2[i]);
         }
      }

      static string[] eventNames = new string[] { "UpTrend1", "DownTrend1", "UpTrend2", "DownTrend2", "UpTrend1", "DownTrend1", "UpTrend2", "DownTrend2" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { false, false, false, false, false, false, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
