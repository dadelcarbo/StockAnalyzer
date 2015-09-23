namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    partial class FilteredSimulationParameterControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.displayPendingOrdersCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.shortSellingCheckBox = new System.Windows.Forms.CheckBox();
            this.amendOrdersCheckBox = new System.Windows.Forms.CheckBox();
            this.sellFallLimitTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.reinvestCheckBox = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.amountTextBox = new System.Windows.Forms.TextBox();
            this.buyGrowLimitTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.endDatePicker = new System.Windows.Forms.DateTimePicker();
            this.startDatePicker = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.taxRateTextBox = new System.Windows.Forms.TextBox();
            this.fixedFeeTextBox = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.removePendingOrdersCheckBox = new System.Windows.Forms.CheckBox();
            this.strategyComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // displayPendingOrdersCheckBox
            // 
            this.displayPendingOrdersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.displayPendingOrdersCheckBox.AutoSize = true;
            this.displayPendingOrdersCheckBox.Location = new System.Drawing.Point(14, 465);
            this.displayPendingOrdersCheckBox.Name = "displayPendingOrdersCheckBox";
            this.displayPendingOrdersCheckBox.Size = new System.Drawing.Size(133, 17);
            this.displayPendingOrdersCheckBox.TabIndex = 18;
            this.displayPendingOrdersCheckBox.Text = "Display pending orders";
            this.displayPendingOrdersCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.shortSellingCheckBox);
            this.groupBox2.Controls.Add(this.amendOrdersCheckBox);
            this.groupBox2.Controls.Add(this.sellFallLimitTextBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(162, 65);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(141, 112);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sell Orders";
            // 
            // shortSellingCheckBox
            // 
            this.shortSellingCheckBox.AutoSize = true;
            this.shortSellingCheckBox.Checked = true;
            this.shortSellingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shortSellingCheckBox.Location = new System.Drawing.Point(3, 61);
            this.shortSellingCheckBox.Name = "shortSellingCheckBox";
            this.shortSellingCheckBox.Size = new System.Drawing.Size(85, 17);
            this.shortSellingCheckBox.TabIndex = 2;
            this.shortSellingCheckBox.Text = "Short Selling";
            this.shortSellingCheckBox.UseVisualStyleBackColor = true;
            // 
            // amendOrdersCheckBox
            // 
            this.amendOrdersCheckBox.AutoSize = true;
            this.amendOrdersCheckBox.Checked = true;
            this.amendOrdersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.amendOrdersCheckBox.Location = new System.Drawing.Point(3, 84);
            this.amendOrdersCheckBox.Name = "amendOrdersCheckBox";
            this.amendOrdersCheckBox.Size = new System.Drawing.Size(91, 17);
            this.amendOrdersCheckBox.TabIndex = 2;
            this.amendOrdersCheckBox.Text = "Amend orders";
            this.amendOrdersCheckBox.UseVisualStyleBackColor = true;
            // 
            // sellFallLimitTextBox
            // 
            this.sellFallLimitTextBox.Location = new System.Drawing.Point(6, 32);
            this.sellFallLimitTextBox.Name = "sellFallLimitTextBox";
            this.sellFallLimitTextBox.Size = new System.Drawing.Size(58, 20);
            this.sellFallLimitTextBox.TabIndex = 1;
            this.sellFallLimitTextBox.Text = "3.25";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Sell when falls by (%)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.reinvestCheckBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.amountTextBox);
            this.groupBox1.Controls.Add(this.buyGrowLimitTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(11, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(141, 112);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Trailing Buy Orders";
            // 
            // reinvestCheckBox
            // 
            this.reinvestCheckBox.AutoSize = true;
            this.reinvestCheckBox.Location = new System.Drawing.Point(6, 84);
            this.reinvestCheckBox.Name = "reinvestCheckBox";
            this.reinvestCheckBox.Size = new System.Drawing.Size(117, 17);
            this.reinvestCheckBox.TabIndex = 3;
            this.reinvestCheckBox.Text = "Re-invest +/- value";
            this.reinvestCheckBox.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 61);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Amount";
            // 
            // amountTextBox
            // 
            this.amountTextBox.Location = new System.Drawing.Point(58, 58);
            this.amountTextBox.Name = "amountTextBox";
            this.amountTextBox.Size = new System.Drawing.Size(71, 20);
            this.amountTextBox.TabIndex = 1;
            this.amountTextBox.Text = "5000";
            // 
            // buyGrowLimitTextBox
            // 
            this.buyGrowLimitTextBox.Location = new System.Drawing.Point(58, 32);
            this.buyGrowLimitTextBox.Name = "buyGrowLimitTextBox";
            this.buyGrowLimitTextBox.Size = new System.Drawing.Size(71, 20);
            this.buyGrowLimitTextBox.TabIndex = 1;
            this.buyGrowLimitTextBox.Text = "3.25";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Buy when moves by (%)";
            // 
            // endDatePicker
            // 
            this.endDatePicker.CustomFormat = "";
            this.endDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.endDatePicker.Location = new System.Drawing.Point(220, 37);
            this.endDatePicker.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.endDatePicker.Name = "endDatePicker";
            this.endDatePicker.Size = new System.Drawing.Size(87, 20);
            this.endDatePicker.TabIndex = 11;
            this.endDatePicker.Value = new System.DateTime(2009, 10, 24, 0, 0, 0, 0);
            // 
            // startDatePicker
            // 
            this.startDatePicker.CustomFormat = "";
            this.startDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.startDatePicker.Location = new System.Drawing.Point(69, 37);
            this.startDatePicker.MinDate = new System.DateTime(1990, 1, 1, 0, 0, 0, 0);
            this.startDatePicker.Name = "startDatePicker";
            this.startDatePicker.Size = new System.Drawing.Size(87, 20);
            this.startDatePicker.TabIndex = 12;
            this.startDatePicker.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(162, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "End Date";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Start Date";
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(87, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Tax (%)";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(25, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Fee";
            // 
            // taxRateTextBox
            // 
            this.taxRateTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.taxRateTextBox.Location = new System.Drawing.Point(135, 20);
            this.taxRateTextBox.Name = "taxRateTextBox";
            this.taxRateTextBox.Size = new System.Drawing.Size(47, 20);
            this.taxRateTextBox.TabIndex = 19;
            this.taxRateTextBox.Text = "0.0";
            // 
            // fixedFeeTextBox
            // 
            this.fixedFeeTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.fixedFeeTextBox.Location = new System.Drawing.Point(36, 19);
            this.fixedFeeTextBox.Name = "fixedFeeTextBox";
            this.fixedFeeTextBox.Size = new System.Drawing.Size(48, 20);
            this.fixedFeeTextBox.TabIndex = 20;
            this.fixedFeeTextBox.Text = "0.0";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.fixedFeeTextBox);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.taxRateTextBox);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Location = new System.Drawing.Point(11, 183);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(292, 53);
            this.groupBox4.TabIndex = 23;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Charges";
            // 
            // removePendingOrdersCheckBox
            // 
            this.removePendingOrdersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.removePendingOrdersCheckBox.AutoSize = true;
            this.removePendingOrdersCheckBox.Location = new System.Drawing.Point(149, 465);
            this.removePendingOrdersCheckBox.Name = "removePendingOrdersCheckBox";
            this.removePendingOrdersCheckBox.Size = new System.Drawing.Size(139, 17);
            this.removePendingOrdersCheckBox.TabIndex = 18;
            this.removePendingOrdersCheckBox.Text = "Remove pending orders";
            this.removePendingOrdersCheckBox.UseVisualStyleBackColor = true;
            // 
            // strategyComboBox
            // 
            this.strategyComboBox.FormattingEnabled = true;
            this.strategyComboBox.Location = new System.Drawing.Point(69, 10);
            this.strategyComboBox.Name = "strategyComboBox";
            this.strategyComboBox.Size = new System.Drawing.Size(238, 21);
            this.strategyComboBox.TabIndex = 25;
            this.strategyComboBox.SelectedValueChanged += new System.EventHandler(this.strategyComboBox_SelectedValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 13);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(46, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "Strategy";
            // 
            // SimulationParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.strategyComboBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.removePendingOrdersCheckBox);
            this.Controls.Add(this.displayPendingOrdersCheckBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.endDatePicker);
            this.Controls.Add(this.startDatePicker);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "SimulationParameterControl";
            this.Size = new System.Drawing.Size(402, 487);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.CheckBox displayPendingOrdersCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox amendOrdersCheckBox;
        private System.Windows.Forms.TextBox sellFallLimitTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox reinvestCheckBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox amountTextBox;
        private System.Windows.Forms.TextBox buyGrowLimitTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker endDatePicker;
        private System.Windows.Forms.DateTimePicker startDatePicker;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox taxRateTextBox;
        private System.Windows.Forms.TextBox fixedFeeTextBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox removePendingOrdersCheckBox;
        private System.Windows.Forms.ComboBox strategyComboBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox shortSellingCheckBox;
    }
}
