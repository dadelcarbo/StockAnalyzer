using StockAnalyzer.StockClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        static public IEnumerable Durations => StockBarDuration.Values.Select(v=>v.Duration);

        static public IEnumerable LineBreaks => new int[] { 0, 1 , 2, 3, 4, 5 };

        static public IEnumerable Smoothings => new int[] { 1, 3, 6, 12, 20, 50, 100 };

        public BarDurationControl()
        {
            InitializeComponent();
        }
    }
}
