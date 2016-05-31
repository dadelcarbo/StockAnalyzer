using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Text;
using System.Threading.Tasks;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockLogging;

namespace StockAnalyzerApp.CustomControl.MultiTimeFrameDlg
{
   public class MTFViewModel : NotifyPropertyChanged
   {
      public class MTFTrend : NotifyPropertyChanged
      {
         public MTFTrend(string name)
         {
            this.name = name;
         }

         private string name;
         public string Name { get { return name; } set { if (value != name) { name = value; OnPropertyChanged("Name"); } } }
         
         private StockSerie.Trend trend1;
         public StockSerie.Trend Trend1 { get { return trend1; } set { if (value != trend1) { trend1 = value; OnPropertyChanged("Trend1"); } } }

         private StockSerie.Trend trend2;
         public StockSerie.Trend Trend2 { get { return trend2; } set { if (value != trend2) { trend2 = value; OnPropertyChanged("Trend2"); } } }

         private StockSerie.Trend trend3;
         public StockSerie.Trend Trend3 { get { return trend3; } set { if (value != trend3) { trend3 = value; OnPropertyChanged("Trend3"); } } }

         private string toolTip1;
         public string ToolTip1 { get { return toolTip1; } set { if (value != toolTip1) { toolTip1 = value; OnPropertyChanged("ToolTip1"); } } }

         private string toolTip2;
         public string ToolTip2 { get { return toolTip2; } set { if (value != toolTip2) { toolTip2 = value; OnPropertyChanged("ToolTip2"); } } }

         private string toolTip3;
         public string ToolTip3 { get { return toolTip3; } set { if (value != toolTip3) { toolTip3 = value; OnPropertyChanged("ToolTip3"); } } }

      }

      public enum SelectedTrend
      {
         All,
         UpTrendOnly,
         DownTrendOnly
      }

      static public Array BarDurations
      {
         get { return Enum.GetValues(typeof (StockSerie.StockBarDuration)); }
      }
      static public Array Groups
      {
         get { return Enum.GetValues(typeof (StockSerie.Groups)); }
      }
      static public Array SelectedViews
      {
         get { return Enum.GetValues(typeof(SelectedTrend)); }
      }

      private SelectedTrend selectedView;
      public SelectedTrend SelectedView { get { return selectedView; } set { if (value != selectedView) { selectedView = value; DurationChanged("SelectedView"); } } }

      private StockSerie.Groups group;
      public StockSerie.Groups Group { get { return group; } set { if (value != group) { group = value; this.stockSeries = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(group));  DurationChanged("Group"); } } }

      private StockSerie.StockBarDuration barDuration1;
      public StockSerie.StockBarDuration BarDuration1 { get { return barDuration1; } set { if (value != barDuration1) { barDuration1 = value; DurationChanged("BarDuration1"); } } }

      private StockSerie.StockBarDuration barDuration2;
      public StockSerie.StockBarDuration BarDuration2 { get { return barDuration2; } set { if (value != barDuration2) { barDuration2 = value; DurationChanged("BarDuration2"); } } }

      private StockSerie.StockBarDuration barDuration3;
      public StockSerie.StockBarDuration BarDuration3 { get { return barDuration3; } set { if (value != barDuration3) { barDuration3 = value; DurationChanged("BarDuration3"); } } }
     
      private ObservableCollection<MTFTrend> trends;
      public ObservableCollection<MTFTrend> Trends { get { return trends; } set { if (value != trends) { trends = value; OnPropertyChanged("Trends"); } } }

      private string indicatorName;
      public string IndicatorName { get { return indicatorName; } set { if (value != indicatorName) { indicatorName = value; DurationChanged("IndicatorName"); } } }

      private IEnumerable<StockSerie> stockSeries;

      public MTFViewModel()
      {
         indicatorName = "TRAILHL(2)";

         this.group = StockSerie.Groups.EURONEXT;
         this.stockSeries = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(group));
         this.Trends = new ObservableCollection<MTFTrend>();
         this.barDuration1 = StockSerie.StockBarDuration.Daily;
         this.barDuration2 = StockSerie.StockBarDuration.TLB;
         this.BarDuration3 = StockSerie.StockBarDuration.TLB_3D;
      }

      private void DurationChanged(string propertyName)
      {
         trends.Clear();

         // Calculate duration
         foreach (StockSerie stockSerie in stockSeries)
         {
            if (stockSerie.Initialise())
            {
               MTFTrend trend = new MTFTrend(stockSerie.StockName);
               try
               {
                  stockSerie.BarDuration = barDuration1;
                  IStockUpDownState upDownState = stockSerie.GetTrailStop(indicatorName);
                  trend.Trend1 = upDownState.UpDownState.Last();
                  if (trend.Trend1 == StockSerie.Trend.UpTrend && this.SelectedView == SelectedTrend.DownTrendOnly)
                     continue;
                  if (trend.Trend1 == StockSerie.Trend.DownTrend && this.SelectedView == SelectedTrend.UpTrendOnly)
                     continue;

                  float close = stockSerie.Values.Last().CLOSE;
                  float distToStop = 0;
                  int nbBars = 0;

                  if (trend.Trend1 == StockSerie.Trend.UpTrend)
                  {
                     distToStop = ((upDownState as IStockTrailStop).Series[0].Last - close)/close;
                  }
                  else
                  {
                     distToStop = ((upDownState as IStockTrailStop).Series[1].Last - close) / close;
                  }
                  for (int i = upDownState.UpDownState.Length - 2;
                        i > 1 &&
                        upDownState.UpDownState[i] ==
                        trend.Trend1;
                        i-- ,nbBars++) ;
                     trend.ToolTip1 = "Close: " + close + Environment.NewLine +
                                      "Stop %: " + distToStop.ToString("P2") + Environment.NewLine +
                                      "Nb Bars: " + nbBars;

                  stockSerie.BarDuration = barDuration2;
                  upDownState = stockSerie.GetTrailStop(indicatorName);
                  trend.Trend2 = upDownState.UpDownState.Last();
                  if (trend.Trend2 == StockSerie.Trend.UpTrend && this.SelectedView == SelectedTrend.DownTrendOnly)
                     continue;
                  if (trend.Trend2 == StockSerie.Trend.DownTrend && this.SelectedView == SelectedTrend.UpTrendOnly)
                     continue;

                  close = stockSerie.Values.Last().CLOSE;
                  distToStop = 0;
                  nbBars = 0;

                  if (trend.Trend2 == StockSerie.Trend.UpTrend)
                  {
                     distToStop = ((upDownState as IStockTrailStop).Series[0].Last - close) / close;
                  }
                  else
                  {
                     distToStop = ((upDownState as IStockTrailStop).Series[1].Last - close) / close;
                  }
                  for (int i = upDownState.UpDownState.Length - 2;
                        i > 1 &&
                        upDownState.UpDownState[i] ==
                        trend.Trend2;
                        i--, nbBars++) ;
                  trend.ToolTip2 = "Close: " + close + Environment.NewLine +
                                   "Stop %: " + distToStop.ToString("P2") + Environment.NewLine +
                                   "Nb Bars: " + nbBars;

                  stockSerie.BarDuration = barDuration3;
                  upDownState = stockSerie.GetTrailStop(indicatorName);
                  trend.Trend3 = upDownState.UpDownState.Last();
                  if (trend.Trend3 == StockSerie.Trend.UpTrend && this.SelectedView == SelectedTrend.DownTrendOnly)
                     continue;
                  if (trend.Trend3 == StockSerie.Trend.DownTrend && this.SelectedView == SelectedTrend.UpTrendOnly)
                     continue;

                  close = stockSerie.Values.Last().CLOSE;
                  distToStop = 0;
                  nbBars = 0;

                  if (trend.Trend3 == StockSerie.Trend.UpTrend)
                  {
                     distToStop = ((upDownState as IStockTrailStop).Series[0].Last - close) / close;
                  }
                  else
                  {
                     distToStop = ((upDownState as IStockTrailStop).Series[1].Last - close) / close;
                  }
                  for (int i = upDownState.UpDownState.Length - 2;
                        i > 1 &&
                        upDownState.UpDownState[i] ==
                        trend.Trend3;
                        i--, nbBars++) ;
                  trend.ToolTip3 = "Close: " + close + Environment.NewLine +
                                   "Stop %: " + distToStop.ToString("P2") + Environment.NewLine +
                                   "Nb Bars: " + nbBars;
               }
               catch (Exception ex)
               {
                  StockLog.Write(ex);
               }
               switch (SelectedView)
               {
                  case SelectedTrend.All:
                     trends.Add(trend);
                     break;
                  case SelectedTrend.DownTrendOnly:
                     if (trend.Trend1 == StockSerie.Trend.DownTrend
                         && trend.Trend2 == StockSerie.Trend.DownTrend
                         && trend.Trend3 == StockSerie.Trend.DownTrend)
                     {
                        trends.Add(trend);
                     }
                     break;
                  case SelectedTrend.UpTrendOnly:
                     if (trend.Trend1 == StockSerie.Trend.UpTrend
                        && trend.Trend2 == StockSerie.Trend.UpTrend
                        && trend.Trend3 == StockSerie.Trend.UpTrend)
                     {
                        trends.Add(trend);
                     }
                     break;
               }
            }
         }
         OnPropertyChanged(propertyName);
      }
   }
}
