using System;
using System.Drawing;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PalmaresControl
{
    public partial class PalmaresDlg : Form
    {
        public PalmaresDlg()
        {
            InitializeComponent();

            this.palmaresControl1.KeyDown += Control_KeyDown;

            this.Shown += FormShown;
        }

        private void FormShown(object sender, EventArgs e)
        {
            TileWindows();
        }

        private void Control_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.T)
            {
                TileWindows();
                e.Handled = true; // Mark the event as handled
            }
        }

        private void TileWindows()
        {
            // Get the screen's working area (excluding taskbar)
            var screenArea = Screen.PrimaryScreen.WorkingArea;

            // Calculate the total width for both forms, accounting for borders
            int borderWidth = 7;
            int totalBorderWidth = borderWidth * 4; // Both forms have borders on both sides

            // Define the width for each form (half of the screen width, minus borders)
            int formWidth = (screenArea.Width / 2) + 2 * borderWidth;

            // Set the location and size for Form1 (left half)
            var form1 = this;
            form1.StartPosition = FormStartPosition.Manual;
            form1.Location = new Point(-borderWidth, 0);
            form1.Size = new Size(formWidth, screenArea.Height + borderWidth);
            form1.Show();

            // Set the location and size for Form2 (right half)
            var form2 = StockAnalyzerForm.MainFrame;
            form2.StartPosition = FormStartPosition.Manual;
            form2.Location = new Point(screenArea.Width / 2 - borderWidth, 0);
            form2.Size = new Size(formWidth, screenArea.Height + borderWidth);
            form2.Show();
        }
    }
}
