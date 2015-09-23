using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLMOMEX : StockIndicatorBase
    {
        public StockIndicator_VOLMOMEX()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override bool RequiresVolumeData { get { return true; } }

        public override string Definition
        {
            get { return "VOLMOMEX(int Period, int SmoothPeriod, float FadeOut)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "SmoothPeriod", "SignalPeriod", "FadeOut" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 6, 6, 1.5f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0.01f, 10.0f) }; }
        }
        public override string[] SerieNames { get { return new string[] { "VOLMOMEX", "SIGNAL", "UpExLimit", "DownExLimit" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed), new Pen(Color.Maroon), new Pen(Color.Maroon) };
                    seriePens[2].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[3].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
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
                List<StockDailyValue> dailyValues = stockSerie.GenerateHeikinAshiBarFromDaily(stockSerie.Values.ToList());
                FloatSerie upVolume = new FloatSerie(stockSerie.Count);
                FloatSerie downVolume = new FloatSerie(stockSerie.Count);

                int i = -1;
                foreach (StockDailyValue dailyValue in dailyValues)
                {
                    i++;
                    float R = dailyValue.HIGH - dailyValue.LOW; // Bar range

                    float R2 = Math.Abs(dailyValue.CLOSE- dailyValue.OPEN); // Body range

                    if (R == 0 || R2 == 0)
                    {
                        upVolume[i] = downVolume[i] = dailyValue.VOLUME/2L;
                        continue;
                    }
                    
                    float R1 = dailyValue.HIGH - Math.Max(dailyValue.CLOSE, dailyValue.OPEN); // Higher shade range
                    float R3 = Math.Min(dailyValue.CLOSE, dailyValue.OPEN)- dailyValue.LOW; // Lower shade range

                    float V = dailyValue.VOLUME;
                    float V1 = V*(R1/R);
                    float V2 = V*(R2/R);
                    float V3 = V*(R3/R);

                    if (dailyValue.CLOSE > dailyValue.OPEN) // UpBar
                    {
                        upVolume[i] = V2 + (V1 + V3)/2.0f;
                        downVolume[i] = V - upVolume[i];
                    }
                    else // DownBar
                    {
                        downVolume[i] = V2 + (V1 + V3) / 2.0f;
                        upVolume[i] = V - downVolume[i];
                    }

                    //V = V(R1/R) + V(R2/R) + V(R3/R);

                }

                //FloatSerie upVolume = stockSerie.GetSerie(StockDataType.UPVOLUME).Sqrt();
                //FloatSerie downVolume = stockSerie.GetSerie(StockDataType.DOWNVOLUME).Sqrt();
                FloatSerie cumulVolume = (upVolume - downVolume).Cumul();
                FloatSerie diffVolume = (cumulVolume - cumulVolume.CalculateEMA((int)this.parameters[0])).CalculateEMA((int)this.parameters[1]);
                FloatSerie fastSerie = diffVolume;

                FloatSerie fastMom = fastSerie;
                this.series[0] = fastMom;
                this.Series[0].Name = this.Name;

                FloatSerie signalSerie = fastMom.CalculateEMA(((int)this.parameters[2]));
                this.series[1] = signalSerie;
                this.Series[1].Name = this.SerieNames[1];


                if (this.series[0] != null && this.Series[0].Count > 0)
                {
                    this.CreateEventSeries(stockSerie.Count);

                    FloatSerie upExLimit = new FloatSerie(stockSerie.Count, this.SerieNames[1]);
                    FloatSerie downExLimit = new FloatSerie(stockSerie.Count, this.SerieNames[2]);
                    FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                    FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                    FloatSerie indicatorToDecorate = this.Series[0];
                    float exhaustionSellLimit = indicatorToDecorate[0];
                    float exhaustionBuyLimit = indicatorToDecorate[0];
                    float exhaustionBuyPrice = highSerie[0];
                    float exhaustionSellPrice = lowSerie[0];
                    float exFadeOut = (100.0f - (float)this.parameters[3]) / 100.0f;

                    float previousValue = indicatorToDecorate[0];
                    float currentValue;

                    for (i = 1; i < indicatorToDecorate.Count - 1; i++)
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
                    this.series[2] = upExLimit;
                    this.series[3] = downExLimit;

                    //for ( i = 5; i < indicatorToDecorate.Count - 1; i++)
                    //{
                    //    this.eventSeries[0][i] = fastMom[i - 1] == upExLimit[i - 1] && fastMom[i] < fastMom[i - 1];
                    //    this.eventSeries[1][i] = fastMom[i - 1] == downExLimit[i - 1] && fastMom[i] > fastMom[i - 1];
                    //}
                }
                else
                {
                    for ( i = 0; i < this.SeriesCount; i++)
                    {
                        this.Series[i] = new FloatSerie(0, this.SerieNames[i]);
                    }
                }
                for (i = 0; i < stockSerie.Count; i++)
                {
                    this.eventSeries[0][i] = fastMom[i] >= signalSerie[i];
                    this.eventSeries[1][i] = fastMom[i] < signalSerie[i];
                }
            }
        }

        static string[] eventNames = new string[] { "Bullish", "Bearish" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}