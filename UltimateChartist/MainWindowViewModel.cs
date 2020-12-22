using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using UltimateChartist.UserControls;

namespace UltimateChartist
{
    public class MainWindowViewModel : ViewModelBase
    {
        static MainWindowViewModel instance = new MainWindowViewModel();
        public static MainWindowViewModel Instance => instance;

        public MainWindowViewModel()
        {
            this.Graphs = new ObservableCollection<GraphViewModel> { };
        }

        public ObservableCollection<GraphViewModel> Graphs { get; private set; }
    }
}
