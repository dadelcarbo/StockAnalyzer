using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;

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
      
      }
      
      static public Array BarDurations
      {
         get { return Enum.GetValues(typeof (StockSerie.StockBarDuration)); }
      }

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

         this.stockSeries = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.COUNTRY));
         this.Trends = new ObservableCollection<MTFTrend>();
         this.barDuration1 = StockSerie.StockBarDuration.Daily;
         this.barDuration2 = StockSerie.StockBarDuration.TLB;
         this.BarDuration3 = StockSerie.StockBarDuration.TLB_3D;
      }

      private void DurationChanged(string p)
      {
         trends.Clear();

         // Calculate duration
         foreach (StockSerie stockSerie in stockSeries)
         {
            MTFTrend trend = new MTFTrend(stockSerie.StockName);

            stockSerie.BarDuration = barDuration1;
            IStockUpDownState upDownState = stockSerie.GetTrailStop(indicatorName);
            trend.Trend1 = upDownState.UpDownState.Last();

            stockSerie.BarDuration = barDuration2;
            upDownState = stockSerie.GetTrailStop(indicatorName);
            trend.Trend2 = upDownState.UpDownState.Last();

            stockSerie.BarDuration = barDuration3;
            upDownState = stockSerie.GetTrailStop(indicatorName);
            trend.Trend3 = upDownState.UpDownState.Last();
            
            trends.Add(trend);
         }

         OnPropertyChanged(p);
      }
   }
}
