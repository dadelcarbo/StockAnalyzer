﻿using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_CROSSSR : StockPaintBarIndicatorEventBase
    {
        public StockPaintBar_CROSSSR()
        {
            this.serieVisibility[8] = true;
            this.serieVisibility[9] = true;
        }

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] {
                   new Pen(Color.Green), new Pen(Color.Red),
                   new Pen(Color.Green), new Pen(Color.Red),
                   new Pen(Color.Green), new Pen(Color.Red),
                   new Pen(Color.Green), new Pen(Color.Red),
                   new Pen(Color.Green), new Pen(Color.Red),
                   new Pen(Color.Green), new Pen(Color.Red)
               };
                return seriePens;
            }
        }
    }
}
