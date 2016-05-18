using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzer.StockClasses
{
   public class StockAlert : IEquatable<StockAlert>
   {
      public DateTime Date { get; set; }
      public string StockName { get; set; }
      public string Alert { get; set; }
      public StockSerie.StockBarDuration BarDuration { get; set; }
      public float AlertClose { get; set; }
      public float CurrentClose { get; set; }

      public StockAlert()
      {
      }

      public StockAlert(StockAlertDef alertDef, DateTime date, string stockName, float alertClose, float currentClose)
      {
         this.Alert = alertDef.EventFullName;
         this.BarDuration = alertDef.BarDuration;
         Date = date;
         StockName = stockName;
         AlertClose = alertClose;
         CurrentClose = currentClose;
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

         IStockViewableSeries indicator =
            StockViewableItemsManager.GetViewableItem(this.Alert.Remove(this.Alert.IndexOf("=>")));
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
                        "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) +
                        System.Environment.NewLine + "@Indicator" + indCount);
                  }
               }
            }
            if (indicator is IStockDecorator)
            {
               IStockDecorator decorator = indicator as IStockDecorator;
               IStockIndicator decoratedIndicator = StockIndicatorManager.CreateIndicator(decorator.DecoratedItem);
               theme = theme.Replace("@Indicator" + indCount,
                  decoratedIndicator.ToThemeString() + System.Environment.NewLine + "@Indicator" + indCount);
               if (decoratedIndicator.HorizontalLines != null && decoratedIndicator.HorizontalLines.Count() > 0)
               {
                  foreach (HLine hline in decoratedIndicator.HorizontalLines)
                  {
                     theme = theme.Replace("@Indicator" + indCount,
                        "LINE|" + hline.Level.ToString() + "|" + GraphCurveType.PenToString(hline.LinePen) +
                        System.Environment.NewLine + "@Indicator" + indCount);
                  }
               }
            }
            theme = theme.Replace("@Indicator" + indCount, indicator.ToThemeString());

            indCount++;
         }
         return theme;
      }

      public static bool operator ==(StockAlert a, StockAlert b)
      {
         return a.Equals(b);
      }
      public static bool operator !=(StockAlert a, StockAlert b)
      {
         return !a.Equals(b);
      }

      public override bool Equals(object obj)
      {
         return this.Equals(obj as StockAlert);
      }

      public bool Equals(StockAlert other)
      {
         if (System.Object.ReferenceEquals(this, other))
         {
            return true;
         }
         return this.StockName == other.StockName &&
                this.Date == other.Date &&
                this.Alert == other.Alert &&
                this.BarDuration == other.BarDuration &&
                this.AlertClose == other.AlertClose;
      }
   }
}
