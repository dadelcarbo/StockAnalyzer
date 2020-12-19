using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_SAREX : StockIndicatorBase
   {
      public StockIndicator_SAREX()
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
         get { return "SAREX(float step, float maximum)"; }
      }

      public override string[] ParameterNames
      {
         get { return new string[] { "Step", "Max" }; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 0.02f, 0.2f }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeFloat(0.001f, 10.0f), new ParamRangeFloat(0.001f, 10.0f) }; }
      }

      public override string[] SerieNames { get { return new string[] { "SAREX.S", "SAREX.R" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         float accelerationFactorStep = (float)this.parameters[0];
         float accelerationFactorMax = (float)this.parameters[1];

         stockSerie.CalculateSAREX(accelerationFactorStep, accelerationFactorStep, out this.Series[0], out this.Series[1]);
      }

      static string[] eventNames = new string[] { };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
