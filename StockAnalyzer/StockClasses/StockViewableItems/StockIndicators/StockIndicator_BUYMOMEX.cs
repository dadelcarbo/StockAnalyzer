using System;
using System.Drawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_BUYMOMEX : StockIndicatorBase
   {
      public StockIndicator_BUYMOMEX()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override bool RequiresVolumeData { get { return true; } }

      public override string Definition
      {
         get { return "BUYMOMEX(int Period, bool UseLog, float FadeOut)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "UseLog", "FadeOut" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 12, false, 1.5f }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeBool(), new ParamRangeFloat(0.1f, 10.0f) }; }
      }
      public override string[] SerieNames { get { return new string[] { "FASTMOM", "UpExLimit", "DownExLimit" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Black), new Pen(Color.Maroon), new Pen(Color.Maroon) };
               seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
               seriePens[2].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            }
            return seriePens;
         }
      }
      public override HLine[] HorizontalLines
      {
         get
         {
            HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.Gray)) };
            lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            return lines;
         }
      }
      public override void ApplyTo(StockSerie stockSerie)
      {
         using (MethodLogger ml = new MethodLogger(this))
         {
            FloatSerie fastMom = stockSerie.CalculateBuySellMomemtum((int)this.parameters[0], (bool)this.parameters[1]);
            this.series[0] = fastMom;
            this.Series[0].Name = this.Name;


            if (this.series[0] != null && this.Series[0].Count > 0)
            {
               this.CreateEventSeries(stockSerie.Count);

               FloatSerie upExLimit = new FloatSerie(stockSerie.Count, this.SerieNames[1]);
               FloatSerie downExLimit = new FloatSerie(stockSerie.Count, this.SerieNames[2]);
               FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
               FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
               for (int i = 1; i < this.SeriesCount; i++)
               {
                  this.Series[i] = new FloatSerie(stockSerie.Count, this.SerieNames[i]);
               }
               FloatSerie indicatorToDecorate = this.Series[0];
               float exhaustionSellLimit = indicatorToDecorate[0];
               float exhaustionBuyLimit = indicatorToDecorate[0];
               float exhaustionBuyPrice = highSerie[0];
               float exhaustionSellPrice = lowSerie[0];
               float exFadeOut = (100.0f - (float)this.parameters[2]) / 100.0f;

               float previousValue = indicatorToDecorate[0];
               float currentValue;

               for (int i = 1; i < indicatorToDecorate.Count - 1; i++)
               {
                  currentValue = indicatorToDecorate[i];

                  if (currentValue < previousValue)
                  {
                     if (indicatorToDecorate.IsBottom(i))
                     {
                        if (currentValue <= exhaustionSellLimit)
                        {
                           // This is an exhaustion selling
                           exhaustionSellPrice = lowSerie[i];
                           exhaustionSellLimit = currentValue;
                        }
                        else
                        {
                           exhaustionSellLimit *= exFadeOut;
                        }
                        exhaustionBuyLimit *= exFadeOut;
                     }
                     else
                     { // trail exhaustion limit down
                        exhaustionSellLimit = Math.Min(currentValue, exhaustionSellLimit);
                        exhaustionBuyLimit *= exFadeOut;
                     }
                  }
                  else if (currentValue > previousValue)
                  {
                     if (indicatorToDecorate.IsTop(i))
                     {
                        if (currentValue >= exhaustionBuyLimit)
                        {
                           // This is an exhaustion selling
                           exhaustionBuyPrice = highSerie[i];
                           exhaustionBuyLimit = currentValue;
                        }
                        else
                        {
                           exhaustionSellLimit *= exFadeOut;
                        }
                        exhaustionBuyLimit *= exFadeOut;
                     }
                     else
                     { // trail exhaustion limit up
                        exhaustionBuyLimit = Math.Max(currentValue, exhaustionBuyLimit);
                        exhaustionSellLimit *= exFadeOut;
                     }
                  }
                  else
                  {
                     exhaustionSellLimit *= exFadeOut;
                     exhaustionBuyLimit *= exFadeOut;
                  }
                  previousValue = currentValue;
                  upExLimit[i] = exhaustionBuyLimit;
                  downExLimit[i] = exhaustionSellLimit;
               }
               upExLimit[indicatorToDecorate.Count - 1] = exhaustionBuyLimit;
               downExLimit[indicatorToDecorate.Count - 1] = exhaustionSellLimit;
               this.series[1] = upExLimit;
               this.series[2] = downExLimit;

               for (int i = 5; i < indicatorToDecorate.Count - 1; i++)
               {
                  this.eventSeries[0][i] = fastMom[i - 1] == upExLimit[i - 1] && fastMom[i] < fastMom[i - 1];
                  this.eventSeries[1][i] = fastMom[i - 1] == downExLimit[i - 1] && fastMom[i] > fastMom[i - 1];
               }
            }
            else
            {
               for (int i = 0; i < this.SeriesCount; i++)
               {
                  this.Series[i] = new FloatSerie(0, this.SerieNames[i]);
               }
            }
         }
      }

      static string[] eventNames = new string[] { "ExhaustionBuying", "ExhautionSelling" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}