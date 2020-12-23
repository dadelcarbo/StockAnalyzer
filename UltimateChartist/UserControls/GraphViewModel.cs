using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace UltimateChartist.UserControls
{
    public class GraphViewModel : ViewModelBase
    {
        public GraphViewModel()
        {
            this.Indicators = new ObservableCollection<string>();
        }
        public ObservableCollection<string> Indicators { get; private set; }
    }
}
