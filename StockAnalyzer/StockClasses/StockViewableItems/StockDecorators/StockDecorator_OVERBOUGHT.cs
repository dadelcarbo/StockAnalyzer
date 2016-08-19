using System;
using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_OVERBOUGHT : StockDecoratorBase, IStockDecorator
    {
        public override string Definition
        {
            get { return "Plots exhaustion points and divergences and provide signal events"; }
        }

        public StockDecorator_OVERBOUGHT()
        {
        }

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.DecoratorPlot; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Smoothing", "Overbought", "Oversold", "LookBack", "SignalSmoothing" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1, 0.75f, -0.75f, 30, 6 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 1000.0f), new ParamRangeFloat(-1000.0f, 1000.0f), new ParamRangeInt(1, 500), new ParamRangeInt(0, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "Data", "Overbought", "Oversold", "Signal" }; } }

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

                int smoothing = (int)this.parameters[0];
                float overbought = (float)this.parameters[1];
                float oversold = (float)this.parameters[2];
                int lookbackPeriod = (int)this.parameters[3];
                int signalSmoothing = (int)this.parameters[4];

                IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);
                if (indicator != null && indicator.Series[0].Count > 0)
                {
                    FloatSerie indicatorToDecorate = indicator.Series[0].CalculateEMA(smoothing);
                    FloatSerie signalSerie = indicatorToDecorate.CalculateEMA(signalSmoothing);
                    FloatSerie upperLimit = new FloatSerie(indicatorToDecorate.Count); upperLimit.Reset(overbought);
                    FloatSerie lowerLimit = new FloatSerie(indicatorToDecorate.Count); lowerLimit.Reset(oversold);

                    if (smoothing <= 1) { this.SerieVisibility[0] = false; }
                    if (signalSmoothing <= 1) { this.SerieVisibility[3] = false; }

                    this.Series[0] = indicatorToDecorate;
                    this.Series[0].Name = this.SerieNames[0];
                    this.Series[1] = upperLimit;
                    this.Series[1].Name = this.SerieNames[1];
                    this.Series[2] = lowerLimit;
                    this.Series[2].Name = this.SerieNames[2];
                    this.Series[3] = signalSerie;
                    this.Series[3].Name = this.SerieNames[3];

                    if (indicator.DisplayTarget == IndicatorDisplayTarget.RangedIndicator && indicator is IRange)
                    {
                        IRange range = (IRange)indicator;
                        indicatorToDecorate = indicatorToDecorate.Sub((range.Max + range.Min) / 2.0f);
                    }
                    FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                    FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                    int lastExhaustionSellIndex = int.MinValue;
                    int lastExhaustionBuyIndex = int.MinValue;
                    float exhaustionBuyPrice = highSerie[0];
                    float exhaustionSellPrice = lowSerie[0];

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
                                if (currentValue <= oversold)
                                {
                                    // This is an exhaustion selling
                                    this.Events[1][i + 1] = true;
                                    exhaustionSellPrice = lowSerie[i];
                                    lastExhaustionSellIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (lowSerie[i] <= exhaustionSellPrice)
                                    {
                                        this.Events[3][i + 1] = true;
                                    }
                                }
                            }
                            else if (indicatorToDecorate.IsTopIsh(i))
                            {
                                if (currentValue >= overbought)
                                {
                                    // This is an exhaustion buying
                                    this.Events[0][i + 1] = true;
                                    exhaustionBuyPrice = highSerie[i];
                                    lastExhaustionBuyIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (highSerie[i] >= exhaustionBuyPrice)
                                    {
                                        this.Events[2][i + 1] = true;
                                    }
                                }
                            }
                        }
                        else if (currentValue < previousValue)
                        {
                            if (indicatorToDecorate.IsBottom(i))
                            {
                                if (currentValue <= oversold)
                                {
                                    // This is an exhaustion selling
                                    this.Events[1][i + 1] = true;
                                    exhaustionSellPrice = lowSerie[i];
                                    lastExhaustionSellIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (lowSerie[i] <= exhaustionSellPrice)
                                    {
                                        this.Events[3][i + 1] = true;
                                    }
                                }
                            }
                        }
                        else if (currentValue > previousValue)
                        {
                            if (indicatorToDecorate.IsTop(i))
                            {
                                if (currentValue >= overbought)
                                {
                                    // This is an exhaustion selling
                                    this.Events[0][i + 1] = true;
                                    exhaustionBuyPrice = highSerie[i];
                                    lastExhaustionBuyIndex = i + 1;
                                }
                                else
                                {
                                    // Check if divergence
                                    if (highSerie[i] >= exhaustionBuyPrice)
                                    {
                                        this.Events[2][i + 1] = true;
                                    }
                                }
                            }
                        }
                        previousValue = currentValue;

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
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false, false, false, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
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
