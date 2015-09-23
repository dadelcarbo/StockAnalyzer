using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;

namespace StockAnalyzer.StockClasses.StockViewableItems
{
    public class StockViewableItemsManager
    {
       static public IStockViewableSeries GetViewableItem(string fullFileString)
        {
            return GetViewableItem(fullFileString, null);
        }
       static public IStockViewableSeries GetViewableItem(string fullFileString, StockSerie stockSerie)
       {
           IStockViewableSeries viewableSerie = null;
           string [] fields = fullFileString.Split('|');
           int offset = 2;
           switch (fields[0].ToUpper())
           {
               case "INDICATOR":
                   if (stockSerie == null)
                   {
                       viewableSerie = StockIndicatorManager.CreateIndicator(fields[1]);
                   }
                   else
                   {
                       viewableSerie = stockSerie.GetIndicator(fields[1]);
                   }
                   offset = 2;
                   break;
               case "PAINTBAR":
                   if (stockSerie == null)
                   {
                       viewableSerie = StockPaintBarManager.CreatePaintBar(fields[1]);
                   }
                   else
                   {
                       viewableSerie = stockSerie.GetPaintBar(fields[1]);
                   }
                   offset = 2;
                   break;
               case "TRAILSTOP":
                   if (stockSerie == null)
                   {
                       viewableSerie = StockTrailStopManager.CreateTrailStop(fields[1]);
                   }
                   else
                   {
                       viewableSerie = stockSerie.GetTrailStop(fields[1]);
                   }
                   offset = 2;
                   break;
               case "DECORATOR":
                   if (stockSerie == null)
                   {
                       viewableSerie = StockDecoratorManager.CreateDecorator(fields[1], fields[2]);
                   }
                   else
                   {
                       viewableSerie =  stockSerie.GetDecorator(fields[1], fields[2]);
                   }
                   offset = 3;
                   break;
               case "TRAIL":
                   if (stockSerie == null)
                   {
                       viewableSerie = StockTrailManager.CreateTrail(fields[1], fields[2]);
                   }
                   else
                   {
                       viewableSerie =  stockSerie.GetTrail(fields[1], fields[2]);
                   }
                   offset = 3;
                   break;
               default:
                   return null;
           }

           if (viewableSerie != null)
           {
               for (int i = 0; i < viewableSerie.SeriesCount; i++)
               {
                   int index = 2 * i + offset;
                   if (index < fields.Length)
                   {
                       viewableSerie.SeriePens[i] = GraphCurveType.PenFromString(fields[2 * i + offset]);
                       viewableSerie.SerieVisibility[i] = bool.Parse(fields[2 * i + offset+1]);
                   }
                   else
                   {
                       viewableSerie.SerieVisibility[i] = true;
                   }
               }
           }
           return viewableSerie;
       }
       static public IStockViewableSeries CreateInitialisedFrom(IStockViewableSeries aViewableSerie, StockSerie stockSerie)
       {
           if (!stockSerie.Initialise()) return null;

           IStockViewableSeries viewableSerie = null;
           switch (aViewableSerie.Type)
           {
               case ViewableItemType.Indicator:
                   viewableSerie = stockSerie.GetIndicator(aViewableSerie.Name);
                   break;
               case ViewableItemType.Decorator:
                   viewableSerie = stockSerie.GetDecorator(aViewableSerie.Name, ((IStockDecorator)aViewableSerie).DecoratedItem);
                   break;
               case ViewableItemType.PaintBar:
                   viewableSerie = stockSerie.GetPaintBar(aViewableSerie.Name);
                   break;
               case ViewableItemType.TrailStop:
                   viewableSerie = stockSerie.GetTrailStop(aViewableSerie.Name);
                   break;
               case ViewableItemType.Trail:
                   viewableSerie = stockSerie.GetTrail(aViewableSerie.Name, ((IStockTrail)aViewableSerie).TrailedItem);
                   break;
               default:
                   break;
           }
           return viewableSerie;
       }
    }
}
