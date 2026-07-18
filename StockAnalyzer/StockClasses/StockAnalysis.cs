using StockAnalyzer.StockDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses
{
    public class StockAnalysis
    {
        public string Theme { get; set; }
        public Dictionary<BarDuration, StockDrawingItems> DrawingItems { get; set; }

        public StockAnalysis()
        {
            this.DrawingItems = new Dictionary<BarDuration, StockDrawingItems>();
            this.Theme = string.Empty;
        }
        public void Clear()
        {
            this.DrawingItems.Clear();
            this.Theme = string.Empty;
        }
        public bool IsEmpty()
        {
            return this.Theme == string.Empty
                && this.DrawingItems.Values.SelectMany(d => d, (d, dd) => dd).Where(dd => dd.IsPersistent).Count() == 0;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public int DeleteTransientDrawings()
        {
            int count = 0;
            if (!DrawingItem.KeepTransient)
            {
                foreach (var values in this.DrawingItems.Values)
                {
                    count = Math.Max(count, values.RemoveAll(d => !d.IsPersistent));

                }
            }
            return count;
        }
    }
}
