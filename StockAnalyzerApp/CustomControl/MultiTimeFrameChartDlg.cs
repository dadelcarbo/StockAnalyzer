using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class MultiTimeFrameChartDlg : Form
    {
        public MultiTimeFrameChartDlg()
        {
            InitializeComponent();
        }

        public void ApplyTheme(StockSerie currentStockSerie)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.fullGraphUserControl1.ApplyTheme(currentStockSerie, StockSerie.StockBarDuration.Bar_6);
                this.fullGraphUserControl2.ApplyTheme(currentStockSerie, StockSerie.StockBarDuration.Bar_3);
                this.fullGraphUserControl3.ApplyTheme(currentStockSerie, StockSerie.StockBarDuration.Daily);
            }
        }
    }
}
