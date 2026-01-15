using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls.ChartView;

namespace StockAnalyzerApp.CustomControl.SectorDlg
{
    /// <summary>
    /// Interaction logic for SectorUserControl.xaml
    /// </summary>
    public partial class SectorUserControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockAndDurationAndIndexChangedEventHandler SelectedStockChanged;

        private System.Windows.Forms.Form Form { get; }
        public SectorUserControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;

            this.ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SectorData")
                {
                    this.sectorChart.Series.Clear();
                    for (int i = 0; i < this.ViewModel.SectorData.Length; i++)
                    {
                        var lineSeries = GetLineSeries(i);
                        this.sectorChart.Series.Add(lineSeries);
                    }
                }
            };

            this.ViewModel.Perform("1M");
        }

        private LineSeries GetLineSeries(int index)
        {
            // Create the LineSeries
            var lineSeries = new LineSeries
            {
                CategoryBinding = new PropertyNameDataPointBinding("X"),
                ValueBinding = new PropertyNameDataPointBinding("Y"),
                ItemsSource = ViewModel.SectorData[index].Points, // Replace with your actual data context
                IsHitTestVisible = true
            };

            // Set LegendSettings
            var legendSettings = new SeriesLegendSettings
            {
                Title = ViewModel.SectorData[index].LegendName // Replace with your actual data context
            };
            lineSeries.LegendSettings = legendSettings;

            // Create the DataTemplate for TrackBallInfoTemplate
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));

            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // Create the first TextBlock for ShortName
            var shortNameTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            shortNameTextBlockFactory.SetBinding(
                TextBlock.TextProperty,
                new Binding($"DataContext.SectorData[{index}].ShortName")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(SectorUserControl)
                    }
                }
            );
            shortNameTextBlockFactory.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));

            // Create the second TextBlock for Value
            var valueTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            valueTextBlockFactory.SetBinding(
                TextBlock.TextProperty,
                new Binding("DataPoint.Value")
                {
                    StringFormat = "F2"
                }
            );

            // Add TextBlocks to the StackPanel
            stackPanelFactory.AppendChild(valueTextBlockFactory);
            stackPanelFactory.AppendChild(shortNameTextBlockFactory);

            // Create the DataTemplate
            var trackBallInfoTemplate = new DataTemplate
            {
                VisualTree = stackPanelFactory
            };

            // Assign the DataTemplate to TrackBallInfoTemplate
            lineSeries.TrackBallInfoTemplate = trackBallInfoTemplate;

            return lineSeries;
        }

        public SectorViewModel ViewModel => (SectorViewModel)this.DataContext;

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            Cursor previousCursor = this.Cursor;
            this.Cursor = Cursors.Wait;

            this.ViewModel.Perform("YTD");

            this.Cursor = previousCursor;
        }
    }
}
