using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace UltimateChartist.UserControls.ChartControls.Indicators
{
    public class IndicatorTreeViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public ObservableCollection<IndicatorTreeViewModel> Items { get; set; }
    }
}
