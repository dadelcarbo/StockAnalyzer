using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public partial class MarketReplayDlg : Form
    {
        public MarketReplayDlg(StockSerie.Groups selectedGroup, StockBarDuration barDuration)
        {
            InitializeComponent();

            marketReplayControl1.DataContext = marketReplayControl1.ViewModel = new MarketReplayViewModel(selectedGroup, barDuration);
        }
    }
}
