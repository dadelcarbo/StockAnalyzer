using System.Windows;
using System.Windows.Controls;

namespace UltimateChartist.UserControls
{
    /// <summary>
    /// Interaction logic for GraphUserControl.xaml
    /// </summary>
    public partial class GraphUserControl : UserControl
    {
        public GraphUserControl()
        {
            InitializeComponent();
        }

        private void AddPanel_Click(object sender, RoutedEventArgs e)
        {
            int count = this.MainGrid.RowDefinitions.Count;
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var gridSplitter = new GridSplitter
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 3,
                ShowsPreview = true
            };
            gridSplitter.SetValue(Grid.RowProperty, count++);
            this.MainGrid.Children.Add(gridSplitter);

            var textBox = new TextBox { Text = $"Text{count}" };
            textBox.SetValue(Grid.RowProperty, count);
            this.MainGrid.Children.Add(textBox);
        }
    }
}
