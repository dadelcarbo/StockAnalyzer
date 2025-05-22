using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class InstrumentViewModel : NotifyPropertyChangedBase
    {
        static public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));

        private StockSerie.Groups group;
        public StockSerie.Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged("Group");
                    this.Lines = new ObservableCollection<StockSerie>(StockDictionary.Instance.Values.Where(s => s.BelongsToGroupFull(this.group)));

                    OnPropertyChanged("Lines");
                }
            }
        }

        private int nbStocks;
        public int NbStocks
        {
            get { return nbStocks; }
            set
            {
                if (value != nbStocks)
                {
                    nbStocks = value;
                    OnPropertyChanged("NbStocks");
                }
            }
        }

        private string runStatus = "Load";
        public string RunStatus
        {
            get { return runStatus; }
            set
            {
                if (value != runStatus)
                {
                    runStatus = value;
                    OnPropertyChanged("RunStatus");
                }
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        private Visibility progressVisibility;
        public Visibility ProgressVisibility
        {
            get { return progressVisibility; }
            set
            {
                if (value != progressVisibility)
                {
                    progressVisibility = value;
                    OnPropertyChanged("ProgressVisibility");
                }
            }
        }
        public ObservableCollection<StockSerie> Lines { get; set; }

        public InstrumentViewModel()
        {
            this.Lines = new ObservableCollection<StockSerie>();

            ProgressVisibility = Visibility.Collapsed;
        }

        private bool canceled = false;
        public async Task CalculateAsync()
        {
            if (ProgressVisibility == Visibility.Visible)
            {
                canceled = true;
                return;
            }
            else
            {
                this.RunStatus = "Cancel";
                canceled = false;
            }
            ProgressVisibility = Visibility.Visible;
            this.Progress = 0;

            Lines.Clear();
            OnPropertyChanged("Lines");
            await Task.Delay(10);

            try
            {
                var stockList = StockDictionary.Instance.Values.Where(s => s.BelongsToGroupFull(this.group)).ToList();
                this.Progress = 0;
                this.NbStocks = stockList.Count;
                int count = 0;
                int step = Math.Max(1, this.NbStocks / 100);
                foreach (var stockSerie in stockList)
                {
                    if (canceled)
                    {
                        break;
                    }
                    count++;
                    if (step == 1 || count % step == 0)
                    {
                        this.Progress = count;

                        await Task.Delay(1);
                    }
                    stockSerie.Initialise();
                    stockSerie.BarDuration = BarDuration.Daily;

                    Lines.Add(stockSerie);
                }
            }
            catch (Exception exception)
            {
                StockLog.Write(exception);
                StockAnalyzerException.MessageBox(exception);
            }

            OnPropertyChanged("Lines");
            await Task.Delay(0);

            ProgressVisibility = Visibility.Collapsed;
            this.RunStatus = "Load";
        }
    }
}
