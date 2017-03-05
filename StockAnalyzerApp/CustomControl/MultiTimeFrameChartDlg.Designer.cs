using StockAnalyzerApp.CustomControl.GraphControls;
namespace StockAnalyzerApp.CustomControl
{
    partial class MultiTimeFrameChartDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.fullGraphUserControl1 = new FullGraphUserControl();
            this.fullGraphUserControl2 = new FullGraphUserControl();
            this.fullGraphUserControl3 = new FullGraphUserControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1));
            this.tableLayoutPanel1.Controls.Add(this.fullGraphUserControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.fullGraphUserControl2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.fullGraphUserControl3, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(741, 398);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // fullGraphUserControl1
            // 
            this.fullGraphUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl1.EndIndex = 0;
            this.fullGraphUserControl1.Location = new System.Drawing.Point(3, 3);
            this.fullGraphUserControl1.Name = "fullGraphUserControl1";
            this.fullGraphUserControl1.Size = new System.Drawing.Size(198, 392);
            this.fullGraphUserControl1.StartIndex = 0;
            this.fullGraphUserControl1.TabIndex = 0;
            // 
            // fullGraphUserControl2
            // 
            this.fullGraphUserControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl2.EndIndex = 0;
            this.fullGraphUserControl2.Location = new System.Drawing.Point(207, 3);
            this.fullGraphUserControl2.Name = "fullGraphUserControl2";
            this.fullGraphUserControl2.Size = new System.Drawing.Size(256, 392);
            this.fullGraphUserControl2.StartIndex = 0;
            this.fullGraphUserControl2.TabIndex = 1;
            // 
            // fullGraphUserControl3
            // 
            this.fullGraphUserControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl3.EndIndex = 0;
            this.fullGraphUserControl3.Location = new System.Drawing.Point(469, 3);
            this.fullGraphUserControl3.Name = "fullGraphUserControl3";
            this.fullGraphUserControl3.Size = new System.Drawing.Size(269, 392);
            this.fullGraphUserControl3.StartIndex = 0;
            this.fullGraphUserControl3.TabIndex = 2;
            // 
            // MultiTimeFrameChartDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 398);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MultiTimeFrameChartDlg";
            this.Text = "MultiTimeFrameGraph";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FullGraphUserControl fullGraphUserControl1;
        private FullGraphUserControl fullGraphUserControl2;
        private FullGraphUserControl fullGraphUserControl3;

    }
}