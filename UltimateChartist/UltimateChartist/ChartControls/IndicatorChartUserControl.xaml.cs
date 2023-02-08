using System.Windows.Controls;

namespace UltimateChartist.ChartControls
{
    /// <summary>
    /// Interaction logic for IndicatorChartUserControl.xaml
    /// </summary>
    public partial class IndicatorChartUserControl : UserControl
    {
        private IndicatorChartViewModel viewModel;
        public IndicatorChartUserControl(IndicatorChartViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;
        }
    }
}
