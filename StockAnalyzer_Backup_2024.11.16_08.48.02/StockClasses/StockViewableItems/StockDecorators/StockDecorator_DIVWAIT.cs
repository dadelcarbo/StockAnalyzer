using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_DIVWAIT : StockDecoratorBase, IStockDecorator
    {
        public override string Definition => "Plots exhaustion points and divergences after waiting for price confirmation";

        public StockDecorator_DIVWAIT()
        {
        }

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public override string[] ParameterNames => new string[] { "FadeOut", "Smooting" };
        public override Object[] ParameterDefaultValues => new Object[] { 1.5f, 1 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.1f, 10.0f), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Signal", "BuyExhaustion", "SellExhaustion" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkRed), new Pen(Color.DarkGray), new Pen(Color.DarkGray) };

                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[2].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            using MethodLogger ml = new MethodLogger(this);
            CreateEventSeries(stockSerie.Count);

            IStockDecorator originalDecorator = stockSerie.GetDecorator(this.Name.Replace("WAIT", ""), this.DecoratedItem);

            this.Series[0] = originalDecorator.Series[0];
            this.Series[1] = originalDecorator.Series[1];
            this.Series[2] = originalDecorator.Series[2];

            IStockTrailStop trailIndicator = stockSerie.GetTrailStop("TRAILHL(1)");

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            int exhaustionTopIndex = 0;
            int exhaustionBottomIndex = 1;
            int bearishDivergenceIndex = 2;
            int bullishDivergenceIndex = 3;

            int upTrendIndex = 0;

            bool waitExhaustionTop = false;
            bool waitExhaustionBottom = false;
            bool waitBearishDivergence = false;
            bool waitBullishDivergence = false;

            for (int i = 10; i < stockSerie.Count; i++)
            {
                if (waitExhaustionTop)
                {
                    if (!trailIndicator.Events[upTrendIndex][i])// (highSerie[i - 1] > highSerie[i])
                    {
                        waitExhaustionTop = false;
                        this.eventSeries[exhaustionTopIndex][i] = true;
                    }
                }
                else
                {
                    if (originalDecorator.Events[exhaustionTopIndex][i])
                    {
                        if (!trailIndicator.Events[upTrendIndex][i])// (highSerie[i - 1] > highSerie[i])
                        {
                            this.eventSeries[exhaustionTopIndex][i] = true;
                        }
                        else
                        {
                            waitExhaustionTop = true;
                        }
                    }
                }

                if (waitBearishDivergence)
                {
                    if (!trailIndicator.Events[upTrendIndex][i])// (highSerie[i - 1] > highSerie[i])
                    {
                        waitBearishDivergence = false;
                        this.eventSeries[bearishDivergenceIndex][i] = true;
                    }
                }
                else
                {
                    if (originalDecorator.Events[bearishDivergenceIndex][i])
                    {
                        if (!trailIndicator.Events[upTrendIndex][i])// (highSerie[i - 1] > highSerie[i])
                        {
                            this.eventSeries[bearishDivergenceIndex][i] = true;
                        }
                        else
                        {
                            waitBearishDivergence = true;
                        }
                    }
                }
                if (waitExhaustionBottom)
                {
                    if (trailIndicator.Events[upTrendIndex][i]) // (lowSerie[i - 1] < lowSerie[i])
                    {
                        waitExhaustionBottom = false;
                        this.eventSeries[exhaustionBottomIndex][i] = true;
                    }
                }
                else
                {
                    if (originalDecorator.Events[exhaustionBottomIndex][i])
                    {
                        if (trailIndicator.Events[upTrendIndex][i]) // (lowSerie[i - 1] < lowSerie[i])
                        {
                            this.eventSeries[exhaustionBottomIndex][i] = true;
                        }
                        else
                        {
                            waitExhaustionBottom = true;
                        }
                    }
                }

                if (waitBullishDivergence)
                {
                    if (trailIndicator.Events[upTrendIndex][i]) // (lowSerie[i - 1] < lowSerie[i])
                    {
                        waitBullishDivergence = false;
                        this.eventSeries[bullishDivergenceIndex][i] = true;
                    }
                }
                else
                {
                    if (originalDecorator.Events[bullishDivergenceIndex][i])
                    {
                        if (trailIndicator.Events[upTrendIndex][i]) // (lowSerie[i - 1] < lowSerie[i])
                        {
                            this.eventSeries[bullishDivergenceIndex][i] = true;
                        }
                        else
                        {
                            waitBullishDivergence = true;
                        }
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
                    eventPens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
                    eventPens[0].Width = 3;
                    eventPens[1].Width = 3;
                    eventPens[2].Width = 2;
                    eventPens[3].Width = 2;
                }
                return eventPens;
            }
        }

        static readonly string[] eventNames = new string[] { "ExhaustionTop", "ExhaustionBottom", "BearishDivergence", "BullishDivergence" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, true, true };
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
