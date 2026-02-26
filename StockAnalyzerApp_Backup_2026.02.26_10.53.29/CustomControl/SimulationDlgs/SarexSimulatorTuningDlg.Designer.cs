using StockAnalyzer.StockClasses;
namespace StockAnalyzerApp.CustomControl
{
    partial class SarexSimulatorTuningDlg
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
            this.tuneAllBtn = new System.Windows.Forms.Button();
            this.generateReportCheckBox = new System.Windows.Forms.CheckBox();
            this.stockComboBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.allStocksCheckBox = new System.Windows.Forms.CheckBox();
            this.simulationParameterControl = new CustomControl.SarexSimulationParameterControl();
            this.SuspendLayout();
            // 
            // tuneAllBtn
            // 
            this.tuneAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tuneAllBtn.Location = new System.Drawing.Point(299, 400);
            this.tuneAllBtn.Name = "tuneAllBtn";
            this.tuneAllBtn.Size = new System.Drawing.Size(99, 23);
            this.tuneAllBtn.TabIndex = 4;
            this.tuneAllBtn.Text = "Tune All";
            this.tuneAllBtn.UseVisualStyleBackColor = true;
            this.tuneAllBtn.Click += new System.EventHandler(this.tuneAllBtn_Click);
            // 
            // generateReportCheckBox
            // 
            this.generateReportCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.generateReportCheckBox.AutoSize = true;
            this.generateReportCheckBox.Location = new System.Drawing.Point(12, 404);
            this.generateReportCheckBox.Name = "generateReportCheckBox";
            this.generateReportCheckBox.Size = new System.Drawing.Size(100, 17);
            this.generateReportCheckBox.TabIndex = 0;
            this.generateReportCheckBox.Text = "Generate report";
            this.generateReportCheckBox.UseVisualStyleBackColor = true;
            // 
            // stockComboBox
            // 
            this.stockComboBox.FormattingEnabled = true;
            this.stockComboBox.Location = new System.Drawing.Point(64, 12);
            this.stockComboBox.Name = "stockComboBox";
            this.stockComboBox.Size = new System.Drawing.Size(238, 21);
            this.stockComboBox.TabIndex = 7;
            this.stockComboBox.SelectedIndexChanged += new System.EventHandler(this.stockComboBox_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 15);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "Stock";
            // 
            // allStocksCheckBox
            // 
            this.allStocksCheckBox.AutoSize = true;
            this.allStocksCheckBox.Location = new System.Drawing.Point(313, 16);
            this.allStocksCheckBox.Name = "allStocksCheckBox";
            this.allStocksCheckBox.Size = new System.Drawing.Size(98, 17);
            this.allStocksCheckBox.TabIndex = 3;
            this.allStocksCheckBox.Text = "Tune all stocks";
            this.allStocksCheckBox.UseVisualStyleBackColor = true;
            // 
            // simulationParameterControl
            // 
            this.simulationParameterControl.Location = new System.Drawing.Point(-2, 39);
            this.simulationParameterControl.Name = "simulationParameterControl";
            this.simulationParameterControl.SelectedStrategy = "HighLowBuySellRateProfitLockStrategy";
            this.simulationParameterControl.Size = new System.Drawing.Size(402, 367);
            this.simulationParameterControl.TabIndex = 11;
            // 
            // SarexSimulatorTuningDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 433);
            this.Controls.Add(this.allStocksCheckBox);
            this.Controls.Add(this.stockComboBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tuneAllBtn);
            this.Controls.Add(this.generateReportCheckBox);
            this.Controls.Add(this.simulationParameterControl);
            this.Name = "SarexSimulatorTuningDlg";
            this.Text = "Trailing Order Simulator Tuning";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button tuneAllBtn;
        private System.Windows.Forms.CheckBox generateReportCheckBox;
        private System.Windows.Forms.ComboBox stockComboBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox allStocksCheckBox;
        private SarexSimulationParameterControl simulationParameterControl;
    }
}