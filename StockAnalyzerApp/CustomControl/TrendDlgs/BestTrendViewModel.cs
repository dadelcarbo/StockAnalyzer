using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public class BestTrendViewModel : NotifyPropertyChangedBase
    {
        public BestTrendViewModel()
        {
        }

        Groups group;
        public Groups Group
        {
            get { return group; }
            set
            {
                if (group != value)
                {
                    group = value;
                    this.OnPropertyChanged("Group");
                }
            }
        }
        public List<Groups> Groups => StockDictionary.GetValidGroups();

        static public IList<BarDuration> BarDurations => StockBarDuration.BarDurations;
        private BarDuration barDuration;
        public BarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                if (barDuration != value)
                {
                    barDuration = value;
                    this.OnPropertyChanged("BarDuration");
                }
            }
        }

        public List<MomentumViewModel> BestTrends { get; private set; }
        public void Perform()
        {
            BestTrends = new List<MomentumViewModel>();
            try
            {
                foreach (var instrument in StockDictionary.Instruments.Values.Where(s => s.BelongsToGroup(this.Group)))
                {
                    var dataSerie = instrument.GetDataSerie(this.barDuration);
                    if (dataSerie == null || dataSerie.Count < 50)
                        continue;

                    var entryEvent = (StockViewableItemsManager.GetViewableItem(this.entryName, dataSerie) as IStockEvent)?.GetEvents(this.EntryEvent);
                    if (entryEvent == null)
                        continue;
                    var exitEvent = (StockViewableItemsManager.GetViewableItem(this.exitName, dataSerie) as IStockEvent)?.GetEvents(this.ExitEvent);
                    if (exitEvent == null)
                        continue;

                    for (int i = 1; i < dataSerie.Count - 1; i++)
                    {
                        if (entryEvent[i])
                        {
                            var entryValue = dataSerie.Values[i + 1].CLOSE;
                            int j = i;
                            while (++j < dataSerie.Count - 1 && !exitEvent[j]) ; // Find exit event
                            var exitValue = dataSerie.Values[j].CLOSE;

                            BestTrends.Add(new MomentumViewModel
                            {
                                Instrument = instrument,
                                BarDuration = this.barDuration,
                                StartIndex = i,
                                StartDate = dataSerie.Values[i].DATE,
                                StartValue = entryValue,
                                EndIndex = j,
                                EndDate = dataSerie.Values[j].DATE,
                                EndValue = exitValue,
                                Value = (exitValue - entryValue) / entryValue
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.OnPropertyChanged("BestTrends");
        }

        public IEnumerable<string> IndicatorNames { get; set; } = StockAnalyzerForm.MainFrame.GetIndicatorsFromCurrentTheme().Append(string.Empty);

        #region Entry
        private string entryName;
        public string EntryName
        {
            get => entryName;
            set
            {
                if (entryName != value)
                {
                    entryName = value;
                    if (!string.IsNullOrEmpty(entryName))
                    {
                        if (!IndicatorNames.Contains(value))
                        {
                            IndicatorNames = IndicatorNames.Prepend(value);
                            OnPropertyChanged("IndicatorNames");
                        }
                        var viewableSeries = StockViewableItemsManager.GetViewableItem(this.entryName);

                        this.EntryEvents = (viewableSeries as IStockEvent)?.EventNames;
                        this.EntryEvent = this.EntryEvents?.FirstOrDefault();
                    }
                    else
                    {
                        this.EntryEvents = null;
                        this.EntryEvent = null;
                    }
                    OnPropertyChanged("EntryEvents");
                    OnPropertyChanged("EntryName");
                }
            }
        }

        public string[] EntryEvents { get; set; }

        private string entryEvent;

        public string EntryEvent
        {
            get { return entryEvent; }
            set
            {
                if (value != entryEvent)
                {
                    entryEvent = value;
                    OnPropertyChanged("EntryEvent");
                }
            }
        }
        #endregion

        #region Exit
        private string exitName;
        public string ExitName
        {
            get => exitName;
            set
            {
                if (exitName != value)
                {
                    exitName = value;
                    if (!string.IsNullOrEmpty(exitName))
                    {
                        if (!IndicatorNames.Contains(value))
                        {
                            IndicatorNames = IndicatorNames.Prepend(value);
                            OnPropertyChanged("IndicatorNames");
                        }
                        var viewableSeries = StockViewableItemsManager.GetViewableItem(this.exitName);

                        this.ExitEvents = (viewableSeries as IStockEvent)?.EventNames;
                        this.ExitEvent = this.ExitEvents?.FirstOrDefault();
                    }
                    else
                    {
                        this.ExitEvents = null;
                        this.ExitEvent = null;
                    }
                    OnPropertyChanged("ExitEvents");
                    OnPropertyChanged("ExitName");
                }
            }
        }

        public string[] ExitEvents { get; set; }

        private string exitEvent;

        public string ExitEvent
        {
            get { return exitEvent; }
            set
            {
                if (value != exitEvent)
                {
                    exitEvent = value;
                    OnPropertyChanged("ExitEvent");
                }
            }
        }
        #endregion
    }
}
