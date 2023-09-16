using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StockAnalyzer.StockPortfolio.StockStrategy
{
    public class StockStrategyEngine
    {
        static public void ProcessStrategies(List<StockBarDuration> barDurations, DateTime barDate)
        {
            using (new MethodLogger(typeof(StockStrategyEngine), false))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    var strategies = StockStrategy.Strategies.Where(a => a.Active && barDurations.Contains(a.BarDuration));

                    var stockList = strategies.Where(s => StockDictionary.Instance.ContainsKey(s.StockName)).Select(s => StockDictionary.Instance[s.StockName]).Where(s => !s.StockAnalysis.Excluded);
                    //stockList.AsParallel().ForAll(s => StockDataProviderBase.DownloadSerieData(s));

                    foreach (var strategy in strategies)
                    {
                        var stockSerie = stockList.First(s => s.StockName == strategy.StockName);
                        ProcessStrategy(strategy, barDate, stockSerie);
                    }
                }
                catch (Exception exception)
                {
                    StockAnalyzerException.MessageBox(exception);
                }
                finally
                {
                    sw.Stop();
                    StockLog.Write($"ProcessStrategies Duration {sw.Elapsed}");
                }
            }
        }
        static private void ProcessStrategy(StockStrategy strategy, DateTime barDate, StockSerie stockSerie)
        {
            try
            {
                StockLog.Write($"Processing strategy: {strategy.Name} StockName:{strategy.StockName} Duration:{strategy.BarDuration} Date:{barDate.ToShortDateString()} {barDate.ToShortTimeString()}");
                using (new StockSerieLocker(stockSerie))
                {
                    if (!stockSerie.Initialise())
                        return;
                    stockSerie.BarDuration = strategy.BarDuration;

                    // Sanity check on Serie data
                    if (!stockSerie.ContainsKey(barDate))
                    {
                        // Strategy Buy
                        StockLog.Write($"Sanity check failed !!! Bar not found");
                        return;
                    }
                    int index = stockSerie.IndexOf(barDate);

                    var portfolio = StockPortfolio.Portfolios.FirstOrDefault(p => p.Name == strategy.Portfolio);
                    if (portfolio == null)
                        return;


                    var position = portfolio.Positions.FirstOrDefault(p => p.StockName == strategy.StockName);
                    if (position == null)
                    {
                        if (stockSerie.MatchEvent(strategy.EntryEvent, strategy.BarDuration, index))
                        {
                            // Strategy Buy
                            StockLog.Write($"Processing strategy Buy");

                            // Calculate position size
                            var stop = stockSerie.GetTrailStop(strategy.EntryStop).Series[0][index];
                            if (float.IsNaN(stop))
                            {
                                StockLog.Write($"TrailStop is not bullish, no trade !!!");
                                return;
                            }
                            var bar = stockSerie.ValueArray[index];

                            var unitRisk = bar.CLOSE - stop;
                            var size = (int)Math.Floor(portfolio.TotalValue * portfolio.MaxRisk / unitRisk);

                            // Check liquidity
                            size = Math.Min((int)(portfolio.Balance / bar.CLOSE), size);

                            // Strategy Buy
                            StockLog.Write($"Processing strategy Buy {stockSerie.StockName} {size}@{bar.CLOSE}");

                            portfolio.AddSaxoActivityOrder(new OrderActivity
                            {
                                ActivityTime = barDate,
                                AveragePrice = bar.CLOSE,
                                ExecutionPrice = bar.CLOSE,
                                Amount = size,
                                BuySell = "Buy",
                                LogId = portfolio.LastLogId + 1,
                                Uic = strategy.Uic,
                                Status = "FinalFill",
                                PositionId = portfolio.LastLogId + 1
                            });

                            position = portfolio.Positions.FirstOrDefault(p => p.StockName == strategy.StockName);
                            position.TrailStop = stop;
                            position.Stop = stop;
                        }
                    }
                    else
                    {
                        if (stockSerie.MatchEvent(strategy.ExitEvent, strategy.BarDuration, index))
                        {
                            // Strategy Sell
                            StockLog.Write($"Processing strategy Sell");

                            var bar = stockSerie.ValueArray[index];
                            portfolio.AddSaxoActivityOrder(new OrderActivity
                            {
                                ActivityTime = barDate,
                                AveragePrice = bar.CLOSE,
                                ExecutionPrice = bar.CLOSE,
                                Amount = position.EntryQty,
                                BuySell = "Sell",
                                LogId = portfolio.LastLogId + 1,
                                Uic = strategy.Uic,
                                Status = "FinalFill",
                                PositionId = portfolio.LastLogId + 1
                            });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                StockAnalyzerException.MessageBox(exception);
            }
        }
    }
}
