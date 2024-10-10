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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.stockNameComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tableLayoutPanel1.Controls.Add(this.fullGraphUserControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.fullGraphUserControl2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.fullGraphUserControl3, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 30);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(735, 428);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // fullGraphUserControl1
            // 
            this.fullGraphUserControl1.CurrentStockSerie = null;
            this.fullGraphUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl1.EndIndex = 0;
            this.fullGraphUserControl1.Location = new System.Drawing.Point(3, 3);
            this.fullGraphUserControl1.Name = "fullGraphUserControl1";
            this.fullGraphUserControl1.Size = new System.Drawing.Size(239, 422);
            this.fullGraphUserControl1.StartIndex = 0;
            this.fullGraphUserControl1.TabIndex = 0;
            // 
            // fullGraphUserControl2
            // 
            this.fullGraphUserControl2.CurrentStockSerie = null;
            this.fullGraphUserControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl2.EndIndex = 0;
            this.fullGraphUserControl2.Location = new System.Drawing.Point(248, 3);
            this.fullGraphUserControl2.Name = "fullGraphUserControl2";
            this.fullGraphUserControl2.Size = new System.Drawing.Size(239, 422);
            this.fullGraphUserControl2.StartIndex = 0;
            this.fullGraphUserControl2.TabIndex = 1;
            // 
            // fullGraphUserControl3
            // 
            this.fullGraphUserControl3.CurrentStockSerie = null;
            this.fullGraphUserControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullGraphUserControl3.EndIndex = 0;
            this.fullGraphUserControl3.Location = new System.Drawing.Point(493, 3);
            this.fullGraphUserControl3.Name = "fullGraphUserControl3";
            this.fullGraphUserControl3.Size = new System.Drawing.Size(239, 422);
            this.fullGraphUserControl3.StartIndex = 0;
            this.fullGraphUserControl3.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.stockNameComboBox, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(737, 461);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // stockNameComboBox
            // 
            this.stockNameComboBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.stockNameComboBox.FormattingEnabled = true;
            this.stockNameComboBox.Location = new System.Drawing.Point(3, 3);
            this.stockNameComboBox.Name = "stockNameComboBox";
            this.stockNameComboBox.Size = new System.Drawing.Size(250, 21);
            this.stockNameComboBox.TabIndex = 1;
            this.stockNameComboBox.SelectedIndexChanged += new System.EventHandler(this.StockNameComboBox_SelectedIndexChanged);
            // 
            // MultiTimeFrameChartDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 461);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "MultiTimeFrameChartDlg";
            this.Text = "MultiTimeFrameGraph";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FullGraphUserControl fullGraphUserControl1;
        private FullGraphUserControl fullGraphUserControl2;
        private FullGraphUserControl fullGraphUserControl3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox stockNameComboBox;

    }
}