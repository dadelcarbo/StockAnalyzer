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
        private StockSerie.Groups selectedGroup;
        private StockBarDuration barDuration;

        public MarketReplayDlg()
        {
            InitializeComponent();
        }

        public MarketReplayDlg(StockSerie.Groups selectedGroup, StockBarDuration barDuration)
        {
            InitializeComponent();
            this.selectedGroup = selectedGroup;
            this.barDuration = barDuration;
        }
    }
}
