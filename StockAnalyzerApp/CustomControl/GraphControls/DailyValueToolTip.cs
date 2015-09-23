using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockDrawing;

namespace StockAnalyzerApp.GraphControls
{
    public partial class GraphValueToolTip : UserControl
    {
        public enum ToolTipType
        {
            DailyValue,
            RSI,
            MACD,
            Volume
        }
        public FloatSerie SecondaryStockSerie { get; set; }
        public int CurrentIndex { get; set; }
        public ToolTipType Type { get; set; }
        public Font TextFont { get; set; }
        public GraphValueToolTip()
        {
            InitializeComponent();
            TextFont = new Font(FontFamily.GenericMonospace, 8);
            CurrentIndex = -1;
        }
        public GraphCurveTypeList CurveList { get; set; }
        
        private void DailyValueToolTip_Paint(object sender, PaintEventArgs e)
        {
            this.BackColor = Color.FromArgb(212, Color.Cornsilk);
            if (this.CurveList != null && CurrentIndex != -1)
            {
                string value = string.Empty;
                switch (Type)
                {
                    case ToolTipType.DailyValue:
                        foreach (GraphCurveType curveType in this.CurveList)
                        {
                            if (!float.IsNaN(curveType.DataSerie[this.CurrentIndex]))
                            {
                                value += curveType.DataSerie.Name + ":\t" + curveType.DataSerie[this.CurrentIndex].ToString("0.##") + "\r\n";
                            }
                        }
                        if (this.SecondaryStockSerie != null)
                        {
                            value += this.SecondaryStockSerie.Name + ":\t" + this.SecondaryStockSerie[this.CurrentIndex].ToString("0.##") + "\r\n";
                        }
                        break;
                    case ToolTipType.RSI:
                    case ToolTipType.MACD: 
                    case ToolTipType.Volume:
                        foreach (GraphCurveType curveType in this.CurveList)
                        {
                            if (curveType.IsVisible)
                            {
                                value += curveType.DataSerie.Name + ":\t" + curveType.DataSerie[this.CurrentIndex].ToString("0.##") + "\r\n";
                            }
                        }
                        break;
                    default:
                        value = "Unknow:\r\n";
                        break;
                }
                e.Graphics.DrawString(value, TextFont, Brushes.Black, 0.0f, 0.0f);
            }
        }
    }
}
