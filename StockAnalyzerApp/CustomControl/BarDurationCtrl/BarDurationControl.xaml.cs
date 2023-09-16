using StockAnalyzer.StockClasses;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.BarDurationCtrl
{
    /// <summary>
    /// Interaction logic for BarDurationControl.xaml
    /// </summary>
    public partial class BarDurationControl : UserControl
    {
        public StockBarDuration BarDuration
        {
            get { return (StockBarDuration)GetValue(BarDurationProperty); }
            set { SetValue(BarDurationProperty, value); }
        }
        public static readonly DependencyProperty BarDurationProperty = DependencyProperty.Register("BarDuration", typeof(StockBarDuration), typeof(BarDurationControl));

        public Visibility LineBreakVisibility
        {
            get { return (Visibility)GetValue(LineBreakVisibilityProperty); }
            set { SetValue(LineBreakVisibilityProperty, value); }
        }
        public static readonly DependencyProperty LineBreakVisibilityProperty = DependencyProperty.Register("LineBreakVisibility", typeof(Visibility), typeof(BarDurationControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility SmoothingVisibility
        {
            get { return (Visibility)GetValue(SmoothingVisibilityProperty); }
            set { SetValue(SmoothingVisibilityProperty, value); }
        }
        public static readonly DependencyProperty SmoothingVisibilityProperty = DependencyProperty.Register("SmoothingVisibility", typeof(Visibility), typeof(BarDurationControl), new PropertyMetadata(Visibility.Collapsed));


        static public IEnumerable Durations => StockBarDuration.Values.Select(v => v.Duration);

        static public IEnumerable LineBreaks => new int[] { 0, 1, 2, 3, 4, 5 };

        static public IEnumerable Smoothings => new int[] { 1, 3, 6, 12, 20, 50, 100 };

        public BarDurationControl()
        {
            InitializeComponent();
        }
    }
}
