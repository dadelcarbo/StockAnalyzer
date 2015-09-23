using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
    partial class StockRiskCalculatorDlg
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.portfolioValueTextBox = new System.Windows.Forms.TextBox();
            this.riskCalculatorBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.stockNameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buyValueTextBox = new System.Windows.Forms.TextBox();
            this.target2ValueTextBox = new System.Windows.Forms.TextBox();
            this.stopValueTextBox = new System.Windows.Forms.TextBox();
            this.target1ValueTextBox = new System.Windows.Forms.TextBox();
            this.qtyTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.amountTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.gain2TextBox = new System.Windows.Forms.TextBox();
            this.stopLossTextBox = new System.Windows.Forms.TextBox();
            this.gain1TextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.stockReturnTextBox = new System.Windows.Forms.TextBox();
            this.stockRiskTextBox = new System.Windows.Forms.TextBox();
            this.stockReturn1TextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.portofolioReturn2TextBox = new System.Windows.Forms.TextBox();
            this.portofolioRiskTextBox = new System.Windows.Forms.TextBox();
            this.portofolioReturn1TextBox = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.riskCalculatorBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "PortfolioValue";
            // 
            // portfolioValueTextBox
            // 
            this.portfolioValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "PortofolioValue", true));
            this.portfolioValueTextBox.Location = new System.Drawing.Point(97, 12);
            this.portfolioValueTextBox.Name = "portfolioValueTextBox";
            this.portfolioValueTextBox.Size = new System.Drawing.Size(100, 20);
            this.portfolioValueTextBox.TabIndex = 1;
            // 
            // riskCalculatorBindingSource
            // 
            this.riskCalculatorBindingSource.DataSource = typeof(CustomControl.PortofolioDlgs.RiskCalculatorViewModel);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(498, 193);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(203, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Stock";
            // 
            // stockNameTextBox
            // 
            this.stockNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stockNameTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StockName", true));
            this.stockNameTextBox.Enabled = false;
            this.stockNameTextBox.Location = new System.Drawing.Point(244, 12);
            this.stockNameTextBox.Name = "stockNameTextBox";
            this.stockNameTextBox.Size = new System.Drawing.Size(334, 20);
            this.stockNameTextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Buy";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Stop";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(209, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Target1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(400, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Target2";
            // 
            // buyValueTextBox
            // 
            this.buyValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "Buy", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.buyValueTextBox.Location = new System.Drawing.Point(97, 50);
            this.buyValueTextBox.Name = "buyValueTextBox";
            this.buyValueTextBox.Size = new System.Drawing.Size(100, 20);
            this.buyValueTextBox.TabIndex = 1;
            // 
            // target2ValueTextBox
            // 
            this.target2ValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "Target2", true));
            this.target2ValueTextBox.Location = new System.Drawing.Point(478, 77);
            this.target2ValueTextBox.Name = "target2ValueTextBox";
            this.target2ValueTextBox.Size = new System.Drawing.Size(100, 20);
            this.target2ValueTextBox.TabIndex = 1;
            // 
            // stopValueTextBox
            // 
            this.stopValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "Stop", true));
            this.stopValueTextBox.Location = new System.Drawing.Point(97, 74);
            this.stopValueTextBox.Name = "stopValueTextBox";
            this.stopValueTextBox.Size = new System.Drawing.Size(100, 20);
            this.stopValueTextBox.TabIndex = 1;
            // 
            // target1ValueTextBox
            // 
            this.target1ValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "Target1", true));
            this.target1ValueTextBox.Location = new System.Drawing.Point(287, 74);
            this.target1ValueTextBox.Name = "target1ValueTextBox";
            this.target1ValueTextBox.Size = new System.Drawing.Size(100, 20);
            this.target1ValueTextBox.TabIndex = 1;
            // 
            // qtyTextBox
            // 
            this.qtyTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "Qty", true));
            this.qtyTextBox.Enabled = false;
            this.qtyTextBox.Location = new System.Drawing.Point(255, 50);
            this.qtyTextBox.Name = "qtyTextBox";
            this.qtyTextBox.Size = new System.Drawing.Size(65, 20);
            this.qtyTextBox.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(203, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Quantity";
            // 
            // amountTextBox
            // 
            this.amountTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "Amount", true));
            this.amountTextBox.Location = new System.Drawing.Point(396, 50);
            this.amountTextBox.Name = "amountTextBox";
            this.amountTextBox.Size = new System.Drawing.Size(65, 20);
            this.amountTextBox.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(344, 53);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Amount";
            // 
            // gain2TextBox
            // 
            this.gain2TextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StockGain2", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.gain2TextBox.Enabled = false;
            this.gain2TextBox.Location = new System.Drawing.Point(478, 103);
            this.gain2TextBox.Name = "gain2TextBox";
            this.gain2TextBox.Size = new System.Drawing.Size(100, 20);
            this.gain2TextBox.TabIndex = 1;
            // 
            // stopLossTextBox
            // 
            this.stopLossTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StopValue", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.stopLossTextBox.Enabled = false;
            this.stopLossTextBox.Location = new System.Drawing.Point(97, 100);
            this.stopLossTextBox.Name = "stopLossTextBox";
            this.stopLossTextBox.Size = new System.Drawing.Size(100, 20);
            this.stopLossTextBox.TabIndex = 1;
            // 
            // gain1TextBox
            // 
            this.gain1TextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StockGain1", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.gain1TextBox.Enabled = false;
            this.gain1TextBox.Location = new System.Drawing.Point(287, 100);
            this.gain1TextBox.Name = "gain1TextBox";
            this.gain1TextBox.Size = new System.Drawing.Size(100, 20);
            this.gain1TextBox.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 103);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Stop Loss";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(209, 103);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "Gain1";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(400, 106);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(38, 13);
            this.label11.TabIndex = 8;
            this.label11.Text = "Gain 2";
            // 
            // stockReturnTextBox
            // 
            this.stockReturnTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StockReturn2", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
            this.stockReturnTextBox.Enabled = false;
            this.stockReturnTextBox.Location = new System.Drawing.Point(478, 129);
            this.stockReturnTextBox.Name = "stockReturnTextBox";
            this.stockReturnTextBox.Size = new System.Drawing.Size(100, 20);
            this.stockReturnTextBox.TabIndex = 1;
            // 
            // stockRiskTextBox
            // 
            this.stockRiskTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StockRisk", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
            this.stockRiskTextBox.Enabled = false;
            this.stockRiskTextBox.Location = new System.Drawing.Point(97, 126);
            this.stockRiskTextBox.Name = "stockRiskTextBox";
            this.stockRiskTextBox.Size = new System.Drawing.Size(100, 20);
            this.stockRiskTextBox.TabIndex = 1;
            // 
            // stockReturn1TextBox
            // 
            this.stockReturn1TextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "StockReturn1", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
            this.stockReturn1TextBox.Enabled = false;
            this.stockReturn1TextBox.Location = new System.Drawing.Point(287, 126);
            this.stockReturn1TextBox.Name = "stockReturn1TextBox";
            this.stockReturn1TextBox.Size = new System.Drawing.Size(100, 20);
            this.stockReturn1TextBox.TabIndex = 1;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(19, 129);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 6;
            this.label12.Text = "StockRisk";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(209, 129);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 13);
            this.label13.TabIndex = 7;
            this.label13.Text = "Stock Return 1";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(400, 132);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(79, 13);
            this.label14.TabIndex = 8;
            this.label14.Text = "Stock Return 2";
            // 
            // portofolioReturn2TextBox
            // 
            this.portofolioReturn2TextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "PortfolioReturn2", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
            this.portofolioReturn2TextBox.Enabled = false;
            this.portofolioReturn2TextBox.Location = new System.Drawing.Point(478, 155);
            this.portofolioReturn2TextBox.Name = "portofolioReturn2TextBox";
            this.portofolioReturn2TextBox.Size = new System.Drawing.Size(100, 20);
            this.portofolioReturn2TextBox.TabIndex = 1;
            // 
            // portofolioRiskTextBox
            // 
            this.portofolioRiskTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "PortfolioRisk", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
            this.portofolioRiskTextBox.Enabled = false;
            this.portofolioRiskTextBox.Location = new System.Drawing.Point(97, 152);
            this.portofolioRiskTextBox.Name = "portofolioRiskTextBox";
            this.portofolioRiskTextBox.Size = new System.Drawing.Size(100, 20);
            this.portofolioRiskTextBox.TabIndex = 1;
            // 
            // portofolioReturn1TextBox
            // 
            this.portofolioReturn1TextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.riskCalculatorBindingSource, "PortfolioReturn1", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
            this.portofolioReturn1TextBox.Enabled = false;
            this.portofolioReturn1TextBox.Location = new System.Drawing.Point(287, 152);
            this.portofolioReturn1TextBox.Name = "portofolioReturn1TextBox";
            this.portofolioReturn1TextBox.Size = new System.Drawing.Size(100, 20);
            this.portofolioReturn1TextBox.TabIndex = 1;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(19, 155);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(69, 13);
            this.label15.TabIndex = 6;
            this.label15.Text = "Portfolio Risk";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(209, 155);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(54, 13);
            this.label16.TabIndex = 7;
            this.label16.Text = "Portfolio 1";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(400, 158);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(54, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "Portfolio 2";
            // 
            // StockRiskCalculatorDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 230);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.stockNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.portofolioReturn1TextBox);
            this.Controls.Add(this.stockReturn1TextBox);
            this.Controls.Add(this.gain1TextBox);
            this.Controls.Add(this.target1ValueTextBox);
            this.Controls.Add(this.portofolioRiskTextBox);
            this.Controls.Add(this.stockRiskTextBox);
            this.Controls.Add(this.stopLossTextBox);
            this.Controls.Add(this.portofolioReturn2TextBox);
            this.Controls.Add(this.stopValueTextBox);
            this.Controls.Add(this.stockReturnTextBox);
            this.Controls.Add(this.amountTextBox);
            this.Controls.Add(this.gain2TextBox);
            this.Controls.Add(this.qtyTextBox);
            this.Controls.Add(this.target2ValueTextBox);
            this.Controls.Add(this.buyValueTextBox);
            this.Controls.Add(this.portfolioValueTextBox);
            this.Controls.Add(this.label1);
            this.Name = "StockRiskCalculatorDlg";
            this.Text = "Risk Calculator";
            ((System.ComponentModel.ISupportInitialize)(this.riskCalculatorBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox portfolioValueTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox stockNameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox buyValueTextBox;
        private System.Windows.Forms.TextBox target2ValueTextBox;
        private System.Windows.Forms.TextBox stopValueTextBox;
        private System.Windows.Forms.TextBox target1ValueTextBox;
        private System.Windows.Forms.TextBox qtyTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox amountTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.BindingSource riskCalculatorBindingSource;
        private System.Windows.Forms.TextBox gain2TextBox;
        private System.Windows.Forms.TextBox stopLossTextBox;
        private System.Windows.Forms.TextBox gain1TextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox stockReturnTextBox;
        private System.Windows.Forms.TextBox stockRiskTextBox;
        private System.Windows.Forms.TextBox stockReturn1TextBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox portofolioReturn2TextBox;
        private System.Windows.Forms.TextBox portofolioRiskTextBox;
        private System.Windows.Forms.TextBox portofolioReturn1TextBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
    }
}