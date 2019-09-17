using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.GroupViewDlg
{
    public class GroupViewModel : NotifyPropertyChangedBase
    {
        static public Array Groups
        {
            get { return Enum.GetValues(typeof(StockSerie.Groups)); }
        }

        private StockSerie.Groups group;
        public StockSerie.Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {

                    // Graphical initialisation
                    StockSplashScreen.ProgressText = "Initialising Series";
                    StockSplashScreen.ProgressVal = 0;
                    StockSplashScreen.ProgressMax = 100;
                    StockSplashScreen.ProgressMin = 0;
                    StockSplashScreen.ShowSplashScreen();

                    group = value;
                    var stockSeries = StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(group) && s.Initialise()).ToList();

                    this.GroupLines = new ObservableCollection<GroupLineViewModel>();
                    int count = 0;
                    foreach (var s in stockSeries)
                    {
                        var barDuration = s.BarDuration;
                        try
                        {
                            s.BarDuration = BarDuration.Daily;

                            count++;
                            StockSplashScreen.ProgressText = s.StockName;

                            if (s.LastIndex > GroupLineViewModel.MIN_BARS && s.Last().Key.AddDays(5) > DateTime.Today)
                            {
                                this.GroupLines.Add(new GroupLineViewModel(s));
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            s.BarDuration = barDuration;
                        }
                    }
                    StockSplashScreen.CloseForm(true);
                }
            }
        }

        private ObservableCollection<GroupLineViewModel> groupLines;
        public ObservableCollection<GroupLineViewModel> GroupLines
        {
            get { return this.groupLines; }
            set
            {
                if (value != groupLines)
                {
                    this.groupLines = value;
                    OnPropertyChanged("GroupLines");
                }
            }
        }

        #region Progress properties
        private int currentStock;
        public int CurrentStock { get { return currentStock; } set { if (value != currentStock) { currentStock = value; OnPropertyChanged("CurrentStock"); } } }

        private bool progressVisible;
        public bool ProgressVisible { get { return progressVisible; } set { if (value != progressVisible) { progressVisible = value; OnPropertyChanged("ProgressVisible"); OnPropertyChanged("ControlEnabled"); } } }

        public bool ControlEnabled { get { return !progressVisible; } }

        #endregion

        public GroupViewModel(StockSerie.Groups group)
        {
            this.Group = group;
        }
        public GroupViewModel()
        {
            this.Group = StockSerie.Groups.SECTORS;
        }
    }
}
