using StockAnalyzerSettings.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class DrawingStyleForm : Form
    {
        private readonly ColorDialog colorDlg;

        public Pen Pen { get; private set; }

        public DrawingStyleForm(Pen pen)
        {
            InitializeComponent();

            this.Pen = pen;
            colorDlg = new ColorDialog();
            colorDlg.AllowFullOpen = true;
            colorDlg.CustomColors = this.GetCustomColors();
        }

        private void DrawingStyleForm_Load(object sender, EventArgs e)
        {
            // Initialise dash type combo box
            foreach (string s in Enum.GetNames(typeof(DashStyle)))
            {
                this.lineTypeComboBox.Items.Add(s);
            }
            this.lineTypeComboBox.SelectedItem = this.Pen.DashStyle.ToString();

            // Initialise thickness combo box
            for (int i = 1; i <= 5; i++)
            {
                this.thicknessComboBox.Items.Add(i);
            }
            this.thicknessComboBox.SelectedItem = (int)this.Pen.Width;

            this.colorPanel.BackColor = this.Pen.Color;
        }

        private void thicknessComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Pen.Width = (int)this.thicknessComboBox.SelectedItem;
        }

        private void lineTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), this.lineTypeComboBox.SelectedItem.ToString());
        }

        void colorPanel_Click(object sender, EventArgs e)
        {
            colorDlg.Color = this.Pen.Color;
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Pen.Color = colorDlg.Color;

                this.colorPanel.BackColor = colorDlg.Color;
                this.SaveCustomColors(this.colorDlg.CustomColors);
            }
        }

        #region CUSTOM CONTROL DRAWING
        private void lineTypeComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                using Pen pen = new Pen(Color.Black, 2);
                pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), ((ComboBox)sender).Items[e.Index].ToString());
                g.DrawLine(pen, rect.X + 10, rect.Y + rect.Height / 2,
                                rect.Width - 10, rect.Y + rect.Height / 2);
            }
        }
        private void thicknessComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                int n = (int)((ComboBox)sender).Items[e.Index];
                using Pen pen = new Pen(Color.Black, n);
                g.DrawLine(pen, rect.X + 10, rect.Y + rect.Height / 2,
                                rect.Width - 10, rect.Y + rect.Height / 2);
            }
        }
        #endregion
        #region CUSTOM COLOR MANAGEMENT
        private int[] GetCustomColors()
        {
            string[] colors = Settings.Default.CustomColors.Split(',');
            int[] res = new int[colors.Length];
            int i = 0;
            foreach (string color in colors)
            {
                res[i++] = int.Parse(color);
            }
            return res;
        }
        private void SaveCustomColors(int[] colors)
        {
            string colorSetting = string.Empty;
            int i;
            for (i = 0; i < colors.Length - 1; i++)
            {
                colorSetting += colors[i].ToString() + ",";
            }
            colorSetting += colors[i].ToString();
            Settings.Default.CustomColors = colorSetting;
            Settings.Default.Save();
        }
        #endregion
    }
}
