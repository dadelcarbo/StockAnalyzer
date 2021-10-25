using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockDrawing
{
    public class GraphCurveTypeList : List<GraphCurveType>
    {
        public GraphCurveTypeList()
        {
            this.Indicators = new List<IStockIndicator>();
            this.ShowMes = new List<IStockDecorator>();
        }
        public void GetMinMax(int startIndex, int endIndex, ref float minValue, ref float maxValue, bool includeInvisible)
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            float tmpMin = float.MaxValue, tmpMax = float.MinValue;
            foreach (GraphCurveType currentCurveType in this)
            {
                if (!includeInvisible && !currentCurveType.IsVisible)
                {
                    continue;
                }
                if (currentCurveType.DataSerie != null)
                {
                    currentCurveType.DataSerie.GetMinMax(startIndex, endIndex, ref tmpMin, ref tmpMax);
                    minValue = Math.Min(minValue, tmpMin);
                    maxValue = Math.Max(maxValue, tmpMax);
                }
            }
            foreach (IStockIndicator stockIndicator in this.Indicators)
            {
                for (int i = 0; i < stockIndicator.SeriesCount; i++)
                {
                    if (stockIndicator.SerieVisibility[i] && stockIndicator.Series[i].Count > 0)
                    {
                        stockIndicator.Series[i].GetMinMax(startIndex, endIndex, ref tmpMin, ref tmpMax);
                        minValue = Math.Min(minValue, tmpMin);
                        maxValue = Math.Max(maxValue, tmpMax);
                    }
                }
            }
            if (this.TrailStop?.Series[0] != null && this.TrailStop.Series[0].Count > 0)
            {
                maxValue = Math.Max(maxValue, this.TrailStop.Series[1].GetMax(startIndex, endIndex));
                minValue = Math.Min(minValue, this.TrailStop.Series[0].GetMin(startIndex, endIndex));
            }
            if (this.Cloud != null)
            {
                for (int i = 0; i < Cloud.SeriesCount; i++)
                {
                    if (Cloud.SerieVisibility[i] && Cloud.Series[i]?.Count > 0)
                    {
                        Cloud.Series[i].GetMinMax(startIndex, endIndex, ref tmpMin, ref tmpMax);
                        minValue = Math.Min(minValue, tmpMin);
                        maxValue = Math.Max(maxValue, tmpMax);
                    }
                }
            }
        }
        public int GetNbVisible()
        {
            int count = this.Count(ct => ct.IsVisible == true);
            if (this.TrailStop?.Series[0] != null && this.TrailStop.Series[0].Count > 0) count += 2;
            foreach (IStockIndicator stockIndicator in this.Indicators)
            {
                foreach (bool isVisible in stockIndicator.SerieVisibility)
                {
                    if (isVisible) count++;
                }
            }
            if (this.Cloud != null) count += 2;
            return count;
        }

        public IStockPaintBar PaintBar { get; set; }
        public IStockAutoDrawing AutoDrawing { get; set; }
        public IStockDecorator Decorator { get; set; }
        public IStockTrailStop TrailStop { get; set; }
        public IStockCloud Cloud { get; set; }
        public List<IStockIndicator> Indicators { get; set; }
        public List<IStockDecorator> ShowMes { get; set; }
    }
}
