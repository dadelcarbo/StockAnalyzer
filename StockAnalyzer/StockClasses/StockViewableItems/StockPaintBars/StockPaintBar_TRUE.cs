﻿using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TRUE : StockPaintBarIndicatorEventBase
    {
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] {
                    new Pen(Color.Green), new Pen(Color.Red),
                    new Pen(Color.Green), new Pen(Color.Red),
                    new Pen(Color.Green), new Pen(Color.Red),
                    new Pen(Color.Red),
                    new Pen(Color.Green), new Pen(Color.Black),
                    new Pen(Color.Green), new Pen(Color.Red),
                    new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
