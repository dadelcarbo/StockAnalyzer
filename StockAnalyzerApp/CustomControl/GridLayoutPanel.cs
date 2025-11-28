using StockAnalyzerApp.CustomControl.GraphControls;
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

    public enum RatioType
    {
        Ratio,
        Percent,
        Absolute
    }

    public partial class GridLayoutPanel : UserControl
    {
        public GridLayoutPanel()
        {
            InitializeComponent();
        }

        public void SetRows(IEnumerable<CollapsiblePanel> controls)
        {
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.Visible = false;

            this.tableLayoutPanel1.Controls.Clear();
            this.tableLayoutPanel1.RowStyles.Clear();

            float totalRatio = controls.Where(c => !c.IsCollapsed && c.RatioType == RatioType.Ratio).Sum(c => c.SizeRatio);
            float totalPercent = controls.Where(c => !c.IsCollapsed && c.RatioType == RatioType.Percent).Sum(c => c.SizeRatio);

            this.tableLayoutPanel1.RowCount = controls.Where(c => !c.IsCollapsed).Count();

            int row = 0;
            foreach (var control in controls.Where(c => !c.IsCollapsed))
            {
                switch (control.RatioType)
                {
                    case RatioType.Ratio:
                        this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, (control.SizeRatio / totalRatio) * 100f));
                        this.tableLayoutPanel1.Controls.Add(control, row++, 0);
                        break;
                    case RatioType.Percent:
                        this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, control.SizeRatio));
                        this.tableLayoutPanel1.Controls.Add(control, row++, 0);
                        break;
                    case RatioType.Absolute:
                        this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, control.SizeRatio));
                        this.tableLayoutPanel1.Controls.Add(control, row++, 0);
                        break;
                    default:
                        break;
                }
            }

            this.tableLayoutPanel1.ResumeLayout();
            this.tableLayoutPanel1.Visible = true;
        }
    }
}
