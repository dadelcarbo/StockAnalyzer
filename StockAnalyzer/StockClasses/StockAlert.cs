using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzer.StockClasses
{
   public class StockAlert
   {
      public DateTime Date { get; set; }
      public string StockName { get; set; }
      public string Alert { get; set; }
      public StockSerie.StockBarDuration BarDuration { get; set; }

      public StockAlert()
      {
      }

      public static ObservableCollection<StockAlert> ParseAlertFile()
      {
         ObservableCollection<StockAlert> alerts = new ObservableCollection<StockAlert>();
         string fileName = Path.GetTempPath() + "AlertLog.txt";
         
         IEnumerable<string> alertLog = new List<string>();
         if (File.Exists(fileName))
         {
            alertLog = File.ReadAllLines(fileName);
         }

         foreach (string line in alertLog)
         {
            string[] fields = line.Split(';');
            StockSerie.StockBarDuration barDuration;
            StockSerie.StockBarDuration.TryParse(fields[2], out barDuration);
            alerts.Add(new StockAlert() { StockName = fields[0], Date = DateTime.Parse(fields[1]), BarDuration = barDuration, Alert = fields[3]});
         }

         return alerts;
      }

      const string ThemeTemplate = @"#ScrollGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|Line
DATA|CLOSE|1:255:0:0:0:Solid|True
#CloseGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart|1:255:184:134:11:Solid
DATA|CLOSE|1:255:0:0:0:Solid|True
@PriceIndicator
SECONDARY|NONE
#Indicator1Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator1
#Indicator2Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator2
#Indicator3Graph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart
@Indicator3
#VolumeGraph
GRAPH|255:255:255:224|255:255:224:96|True|255:211:211:211|BarChart";

      public string ToTheme()
      {
         indCount = 1;

         string theme = ThemeTemplate;

         IStockViewableSeries indicator = StockViewableItemsManager.GetViewableItem(this.Alert.Remove(this.Alert.IndexOf("=>")));
         theme = AppendThemeLine(indicator, theme);
         
         return theme;
      }

      private static int indCount;
      private static string AppendThemeLine(IStockViewableSeries indicator, string theme)
      {
         int index = -1;
         if (indicator.DisplayTarget == IndicatorDisplayTarget.PriceIndicator)
         {
            index = theme.IndexOf("@PriceIndicator");
            theme = theme.Insert(index, indicator.ToThemeString() + System.Environment.NewLine);
         }
         else
         {
            if (indicator is IStockIndicator)
            {
               IStockIndicator stockIndicator = indicator as IStockIndicator;
               if (stockIndicator.HorizontalLines != null && stockIndicator.HorizontalLines.Count() > 0)
               {
                  foreach (HLine hline in stockIndicator.HorizontalLines)
                  {
                     theme = theme.Replace("@Indicator" + indCount,
                        "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) + System.Environment.NewLine + "@Indicator" + indCount);
                  }
               }
            }
            if (indicator is IStockDecorator)
            {
               IStockDecorator decorator = indicator as IStockDecorator;
               IStockIndicator decoratedIndicator = StockIndicatorManager.CreateIndicator(decorator.DecoratedItem);
               theme = theme.Replace("@Indicator" + indCount, decoratedIndicator.ToThemeString() + System.Environment.NewLine + "@Indicator" + indCount);
               if (decoratedIndicator.HorizontalLines != null && decoratedIndicator.HorizontalLines.Count() > 0)
               {
                  foreach (HLine hline in decoratedIndicator.HorizontalLines)
                  {
                     theme = theme.Replace("@Indicator" + indCount,
                        "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) + System.Environment.NewLine + "@Indicator" + indCount);
                  }
               }
            }
            theme = theme.Replace("@Indicator" + indCount, indicator.ToThemeString());



            indCount++;
         }
         return theme;
      }
   }
}
