using System.Collections.ObjectModel;

namespace UltimateChartist.UserControls.ChartControls.Indicators
{
    public class IndicatorSelectorViewModel
    {
        public IndicatorSelectorViewModel()
        {
            this.Root = new ObservableCollection<IndicatorTreeViewModel>
            {
                new IndicatorTreeViewModel {
                    Name = "Price",
                    Items = new ObservableCollection<IndicatorTreeViewModel>
                    {
                         new IndicatorTreeViewModel {Name= "Item1"},
                         new IndicatorTreeViewModel {Name= "Item2"},
                    }
                },
                new IndicatorTreeViewModel {
                    Name = "Indicator1",
                    Items = new ObservableCollection<IndicatorTreeViewModel>
                    {
                         new IndicatorTreeViewModel {Name= "Item3"},
                         new IndicatorTreeViewModel {Name= "Item4"},
                    }
                }
            };
        }
        public ObservableCollection<IndicatorTreeViewModel> Root { get; set; }

    }
}
