﻿using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_VOLPROAM : StockPaintBarIndicatorEventBase
    {
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue, 2), new Pen(Color.Purple, 2) };
                return seriePens;
            }
        }
    }
}
