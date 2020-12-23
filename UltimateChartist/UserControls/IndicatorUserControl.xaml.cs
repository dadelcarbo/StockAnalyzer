using System;
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

namespace UltimateChartist.UserControls
{
    /// <summary>
    /// Interaction logic for IndicatorUserControl.xaml
    /// </summary>
    public partial class IndicatorUserControl : UserControl
    {

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(IndicatorUserControl));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public IndicatorUserControl()
        {
            InitializeComponent();
        }
    }
}
