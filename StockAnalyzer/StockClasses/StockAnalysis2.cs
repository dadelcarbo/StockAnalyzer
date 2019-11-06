using StockAnalyzer.StockDrawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockComment
    {
        public DateTime Date { get; set; }
        public string Comment { get; set; }
    }
    [Serializable]
    public class StockAnalysis2
    {
        public string Strategy { get; set; }
        public string Theme { get; set; }
        public bool Excluded { get; set; }
        public bool FollowUp { get; set; }
        public List<StockComment> Comments { get; set; }

        public List<StockDrawingItems2> DrawingItems { get; set; }

        public StockAnalysis2()
        {
            this.Comments = new List<StockComment>();
            this.DrawingItems = new List<StockDrawingItems2>();
            this.Excluded = false;
            this.FollowUp = false;
            this.Theme = string.Empty;
        }
        public void Clear()
        {
            this.Comments.Clear();
            this.DrawingItems.Clear();
            this.Excluded = false;
            this.FollowUp = false;
            this.Theme = string.Empty;
        }
        public bool IsEmpty()
        {
            return this.Comments.Count == 0 && this.Excluded == false &&
                this.FollowUp == false &&
                this.Theme == string.Empty &&
                this.DrawingItems.Sum(s => s.DrawingItems.Count) == 0;
        }
        public int DeleteTransientDrawings()
        {
            return this.DrawingItems.Sum(s => s.DrawingItems.RemoveAll(d => !d.IsPersistent));
        }
    }
}
