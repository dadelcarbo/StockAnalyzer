using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_DIV : StockDecoratorBase, IStockDecorator
    {
        public override string Definition => "Plots exhaustion points and divergences and provide signal events";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public override string[] ParameterNames => new string[] { "FadeOut", "Smoothing", "LookBack", "SignalSmoothing" };
        public override Object[] ParameterDefaultValues => new Object[] { 1.5f, 1, 30, 6 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.1f, 10.0f), new ParamRangeInt(1, 500), new ParamRangeInt(0, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Data", "BuyExhaustion", "SellExhaustion", "Signal" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkGray), new Pen(Color.DarkGray), new Pen(Color.DarkRed) };

                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[2].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                CreateEventSeries(stockSerie.Count);

                IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);
                if (indicator != null && indicator.Series[0].Count > 0)
                {
                    FloatSerie indicatorToDecorate = indicator.Series[0].CalculateEMA((int)this.parameters[1]);
                    FloatSerie signalSerie = indicatorToDecorate.CalculateEMA((int)this.parameters[3]);
                    FloatSerie upperLimit = new FloatSerie(indicatorToDecorate.Count);
                    FloatSerie lowerLimit = new FloatSerie(indicatorToDecorate.Count);

                    if ((int)this.parameters[1] <= 1) { this.SerieVisibility[0] = false; }
                    this.Series[0] = indicatorToDecorate;
                    this.Series[0].Name = this.SerieNames[0];
                    this.Series[1] = upperLimit;
                    this.Series[1].Name = this.SerieNames[1];
                    this.Series[2] = lowerLimit;
                    this.Series[2].Name = this.SerieNames[2];
                    this.Series[3] = signalSerie;
                    this.Series[3].Name = this.SerieNames[3];

                    int lookbackPeriod = (int)this.parameters[2];

                    if (indicator.DisplayTarget == IndicatorDisplayTarget.RangedIndicator && indicator is IRange)
                    {
                        IRange range = (IRange)indicator;
                        indicatorToDecorate = indicatorToDecorate.Sub((range.Max + range.Min) / 2.0f);
                    }
                    FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                    FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                    int lastExhaustionSellIndex = int.MinValue;
                    int lastExhaustionBuyIndex = int.MinValue;
                    float exhaustionSellLimit = indicatorToDecorate[0];
                    float exhaustionBuyLimit = indicatorToDecorate[0];
                    float exhaustionBuyPrice = highSerie[0];
                    float exhaustionSellPrice = lowSerie[0];
                    float exFadeOut = (100.0f - (float)this.parameters[0]) / 100.0f;

                    float previousValue = indicatorToDecorate[0];
                    float currentValue;
                    int i = 0;
                    for (i = 1; i < indicatorToDecorate.Count - 1; i++)
                    {
                        if (indicatorToDecorate[i] > 0)
                        {
                            this.Events[6][i] = true;
                        }
                        else
                        {
                            this.Events[7][i] = true;
                        }
                        if (indicatorToDecorate[i] > signalSerie[i])
                        {
                            this.Events[8][i] = true;
                        }
                        else
                        {
                            this.Events[9][i] = true;
                        }
                        currentValue = indicatorToDecorate[i];
                        if (currentValue == previousValue)
                        {
                            if (indicatorToDecorate.IsBottomIsh(i))
                            {
                                if (currentValue <= exhaustionSellLimit)
                                {
                                    // This is an exhaustion selling
                                    this.Events[1][i + 1] = true;
                                    exhaustionSellPrice = lowSerie[i];
                                    exhaustionSellLimit = currentValue;
                                    lastExhaustionSellIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (lowSerie[i] <= exhaustionSellPrice)
                                    {
                                        this.Events[3][i + 1] = true;
                                    }
                                    exhaustionSellLimit *= exFadeOut;
                                }
                                exhaustionBuyLimit *= exFadeOut;
                            }
                            else if (indicatorToDecorate.IsTopIsh(i))
                            {
                                if (currentValue >= exhaustionBuyLimit)
                                {
                                    // This is an exhaustion buying
                                    this.Events[0][i + 1] = true;
                                    exhaustionBuyPrice = highSerie[i];
                                    exhaustionBuyLimit = currentValue;
                                    lastExhaustionBuyIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (highSerie[i] >= exhaustionBuyPrice)
                                    {
                                        this.Events[2][i + 1] = true;
                                    }
                                    exhaustionSellLimit *= exFadeOut;
                                }
                                exhaustionBuyLimit *= exFadeOut;
                            }
                            else
                            {
                                exhaustionSellLimit *= exFadeOut;
                                exhaustionBuyLimit *= exFadeOut;
                            }
                        }
                        else if (currentValue < previousValue)
                        {
                            if (indicatorToDecorate.IsBottom(i))
                            {
                                if (currentValue <= exhaustionSellLimit)
                                {
                                    // This is an exhaustion selling
                                    this.Events[1][i + 1] = true;
                                    exhaustionSellPrice = lowSerie[i];
                                    exhaustionSellLimit = currentValue;
                                    lastExhaustionSellIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (lowSerie[i] <= exhaustionSellPrice)
                                    {
                                        this.Events[3][i + 1] = true;
                                    }
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
                                    this.Events[0][i + 1] = true;
                                    exhaustionBuyPrice = highSerie[i];
                                    exhaustionBuyLimit = currentValue;
                                    lastExhaustionBuyIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (highSerie[i] >= exhaustionBuyPrice)
                                    {
                                        this.Events[2][i + 1] = true;
                                    }
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

                        upperLimit[i] = exhaustionBuyLimit;
                        lowerLimit[i] = exhaustionSellLimit;

                        // Exhaustion occured events
                        if (lookbackPeriod > 0)
                        {
                            if (i + 1 - lookbackPeriod < lastExhaustionBuyIndex)
                            {
                                this.Events[4][i + 1] = true;
                            }
                            if (i + 1 - lookbackPeriod < lastExhaustionSellIndex)
                            {
                                this.Events[5][i + 1] = true;
                            }
                        }
                    }
                    // Update last values
                    exhaustionSellLimit *= exFadeOut;
                    exhaustionBuyLimit *= exFadeOut;

                    upperLimit[i] = exhaustionBuyLimit;
                    lowerLimit[i] = exhaustionSellLimit;

                    if (indicator.DisplayTarget == IndicatorDisplayTarget.RangedIndicator && indicator is IRange)
                    {
                        IRange range = (IRange)indicator;
                        this.Series[1] = upperLimit.Add((range.Max + range.Min) / 2.0f);
                        this.Series[2] = lowerLimit.Add((range.Max + range.Min) / 2.0f);
                    }
                }
                else
                {
                    for (int i = 0; i < this.EventNames.Length; i++)
                    {
                        this.Events[i] = new BoolSerie(0, this.EventNames[i]);
                    }
                }
            }
        }

        public override System.Drawing.Pen[] EventPens
        {
            get
            {
                if (eventPens == null)
                {
                    eventPens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Transparent), new Pen(Color.Transparent), new Pen(Color.Transparent), new Pen(Color.Transparent), new Pen(Color.Transparent), new Pen(Color.Transparent), new Pen(Color.Transparent), new Pen(Color.Transparent) };
                    eventPens[0].Width = 3;
                    eventPens[1].Width = 3;
                    eventPens[2].Width = 2;
                    eventPens[3].Width = 2;
                }
                return eventPens;
            }
        }

        static string[] eventNames = new string[] { "ExhaustionTop", "ExhaustionBottom", "BearishDivergence", "BullishDivergence", "ExhaustionTopOccured", "ExhaustionBottomOccured", "Positive", "Negative", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false, false, false, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
/*
{***** Copyright David Carbonel - All rights reserved *****}
Inputs: 
	Indicator(Numericseries), LookBack(Numeric), ExFadeOut(Numeric),
	oExhaustionBearishLimit(numericref), oExhaustionBullishLimit(numericref);

Variables: 
	IsTop(False), IsBottom(True), NeedToLookForEvent(True),
	ExhaustionSellValue(0), ExhaustionSellPrice(0),	ExhaustionSellLimit(0),
	ExhaustionBuyValue(0),  ExhaustionBuyPrice(0),  ExhaustionBuyLimit(0),
	DivAnyType(0);

// Returns of off the following events
// 0 - No Events
// 1 - Exhaustion buying
// 2 - Exhaustion selling
// 3 - Bearish divergence
// 4 - Bullish divergence
// 5 - Simple Top
// 6 - Simple Bottom
// 7 - Exhaustion buying candidate
// 8 - Exhaustion selling candidate
// 9 - Bearish divergence candidate
// 10 - Bullish divergence candidate

//If Indicator > 0 Then
	ExhaustionSellLimit = ExhaustionSellLimit * (100-ExFadeOut) /100;
//Else
	ExhaustionBuyLimit = ExhaustionBuyLimit * (100-ExFadeOut) /100;

DivAnyType = 0;
IsTop = False;
IsBottom = False;
If Indicator[1] = Highest(Indicator,3) Then 
Begin
	IsTop = True;
	DivAnyType = 5;
End;
If Indicator[1] = Lowest(Indicator,3) Then
Begin
	IsBottom = True;
	DivAnyType = 6;
End;

NeedToLookForEvent = (IsTop Or IsBottom);

// Detect Exhaustion signals
If NeedToLookForEvent = True Then
	If IsTop And Indicator[1] >= ExhaustionBuyLimit Then 
		Begin
		ExhaustionBuyValue = Indicator[1];
		ExhaustionBuyPrice = Highest(High, 3);
		ExhaustionBuyLimit = ExhaustionBuyValue;
		NeedToLookForEvent = False;
		DivAnyType = 1;
		End
	Else If IsBottom And Indicator[1] <= ExhaustionSellLimit Then
    	Begin
		ExhaustionSellValue = Indicator[1];
		ExhaustionSellPrice = Lowest(Low, 3);
		ExhaustionSellLimit = ExhaustionSellValue;
		NeedToLookForEvent = False;
		DivAnyType = 2;
		End;

// Detect divergence short term divergence using the lookback
If NeedToLookForEvent = True Then
	If IsTop And Indicator[1] < Highest(Indicator,LookBack) And (High[1] = Highest(High, LookBack) Or High = Highest(High, LookBack)) Then 
		Begin
		ExhaustionBuyPrice = Highest(High, 3);
		NeedToLookForEvent = False;
		DivAnyType = 3;
		End
	Else If IsBottom And Indicator[1] > Lowest(Indicator,LookBack) And (Low[1] = Lowest(Low, LookBack) Or Low = Lowest(Low, LookBack)) Then
    	Begin
		NeedToLookForEvent = False;
		ExhaustionSellPrice = Lowest(Low, 3);
		DivAnyType = 4;
		End;

// Detect long term divergence from the last exhaustion buying/selling
If NeedToLookForEvent = True Then
Begin
	If IsTop And Indicator[1] < ExhaustionBuyValue And (High[1] > ExhaustionBuyPrice Or High > ExhaustionBuyPrice) Then 
		Begin
		ExhaustionBuyPrice = Highest(High, 3);
		DivAnyType = 3;
		End
	Else If IsBottom And Indicator[1] > ExhaustionSellValue And (Low[1] < ExhaustionSellPrice Or Low < ExhaustionSellPrice) Then
    	Begin
		ExhaustionSellPrice = Lowest(Low, 3);
		DivAnyType = 4;
		End;
End;


// 9 - Bearish divergence candidate
// 10 - Bullish divergence candidate
// Look for candidates
If DivAnyType = 0 Then
Begin
	// Exhaustion candidates
	If      Indicator >= ExhaustionBuyLimit And Indicator = Highest(Indicator,LookBack) 	Then DivAnyType = 7
	Else If Indicator <= ExhaustionSellLimit And Indicator = Lowest(Indicator,LookBack) 	Then DivAnyType = 8
	Else If Indicator < ExhaustionBuyValue And High > ExhaustionBuyPrice 				Then DivAnyType = 9
	Else If Indicator > ExhaustionSellValue And Low < ExhaustionSellPrice 				Then DivAnyType = 10
	Else If Indicator < Highest(Indicator,LookBack) And High = Highest(High, LookBack) 	Then DivAnyType = 9
	Else If Indicator > Lowest(Indicator,LookBack) And Low = Lowest(Low, LookBack) 		Then DivAnyType = 10;
End;

oExhaustionBullishLimit = Minlist(ExhaustionSellLimit, Indicator) ;
oExhaustionBearishLimit = Maxlist(ExhaustionBuyLimit, Indicator) ;
_MyDivAnyF = DivAnyType;
*/
