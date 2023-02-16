using System.Windows.Controls;
using UltimateChartist.ChartControls.Indicators;
using UltimateChartist.Indicators;

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

            this.Loaded += IndicatorChartUserControl_Loaded;
        }

        private void IndicatorChartUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var series in viewModel.Indicator.CartesianSeries)
            {
                this.indicatorChart.Series.Insert(0, series);
            }
            var rangedIndicator = viewModel.Indicator.Indicator as IRangedIndicator;
            if (rangedIndicator != null)
            {
                verticalAxis.Minimum = rangedIndicator.Minimum;
                verticalAxis.Maximum = rangedIndicator.Maximum;
            }

            var dlg = new IndicatorConfigWindow(viewModel.Indicator);
            dlg.ShowDialog();
        }
    }
}
