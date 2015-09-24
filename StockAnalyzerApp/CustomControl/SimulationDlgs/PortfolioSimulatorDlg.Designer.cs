namespace StockAnalyzerApp.CustomControl
{
   partial class PortfolioSimulatorDlg
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
         this.Portofolio = new System.Windows.Forms.Label();
         this.portofolioComboBox = new System.Windows.Forms.ComboBox();
         this.label3 = new System.Windows.Forms.Label();
         this.watchListComboBox = new System.Windows.Forms.ComboBox();
         this.simulateTradingBtn = new System.Windows.Forms.Button();
         this.parametrizableControl = new CustomControl.IndicatorDlgs.ParametrizableControl();
         this.newPortfolioBtn = new System.Windows.Forms.Button();
         this.strategyComboBox = new System.Windows.Forms.ComboBox();
         this.label10 = new System.Windows.Forms.Label();
         this.endDatePicker = new System.Windows.Forms.DateTimePicker();
         this.startDatePicker = new System.Windows.Forms.DateTimePicker();
         this.label2 = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.taxRateTextBox = new System.Windows.Forms.TextBox();
         this.label8 = new System.Windows.Forms.Label();
         this.fixedFeeTextBox = new System.Windows.Forms.TextBox();
         this.groupBox4 = new System.Windows.Forms.GroupBox();
         this.frequencyComboBox = new System.Windows.Forms.ComboBox();
         this.label4 = new System.Windows.Forms.Label();
         this.groupBox4.SuspendLayout();
         this.SuspendLayout();
         // 
         // Portofolio
         // 
         this.Portofolio.AutoSize = true;
         this.Portofolio.Location = new System.Drawing.Point(12, 15);
         this.Portofolio.Name = "Portofolio";
         this.Portofolio.Size = new System.Drawing.Size(51, 13);
         this.Portofolio.TabIndex = 0;
         this.Portofolio.Text = "Portofolio";
         // 
         // portofolioComboBox
         // 
         this.portofolioComboBox.FormattingEnabled = true;
         this.portofolioComboBox.Location = new System.Drawing.Point(69, 12);
         this.portofolioComboBox.Name = "portofolioComboBox";
         this.portofolioComboBox.Size = new System.Drawing.Size(238, 21);
         this.portofolioComboBox.TabIndex = 2;
         this.portofolioComboBox.SelectedIndexChanged += new System.EventHandler(this.portofolioComboBox_SelectedIndexChanged);
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(12, 42);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(51, 13);
         this.label3.TabIndex = 0;
         this.label3.Text = "Watchlist";
         // 
         // watchListComboBox
         // 
         this.watchListComboBox.FormattingEnabled = true;
         this.watchListComboBox.Location = new System.Drawing.Point(69, 39);
         this.watchListComboBox.Name = "watchListComboBox";
         this.watchListComboBox.Size = new System.Drawing.Size(238, 21);
         this.watchListComboBox.TabIndex = 2;
         this.watchListComboBox.SelectedIndexChanged += new System.EventHandler(this.watchListComboBox_SelectedIndexChanged);
         // 
         // simulateTradingBtn
         // 
         this.simulateTradingBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.simulateTradingBtn.Location = new System.Drawing.Point(282, 532);
         this.simulateTradingBtn.Name = "simulateTradingBtn";
         this.simulateTradingBtn.Size = new System.Drawing.Size(99, 23);
         this.simulateTradingBtn.TabIndex = 4;
         this.simulateTradingBtn.Text = "Simulate Trading";
         this.simulateTradingBtn.UseVisualStyleBackColor = true;
         this.simulateTradingBtn.Click += new System.EventHandler(this.simulateTradingBtn_Click);
         // 
         // parametrizableControl
         // 
         this.parametrizableControl.Location = new System.Drawing.Point(13, 338);
         this.parametrizableControl.Name = "parametrizableControl";
         this.parametrizableControl.Size = new System.Drawing.Size(294, 181);
         this.parametrizableControl.TabIndex = 6;
         this.parametrizableControl.ViewableItem = null;
         // 
         // newPortfolioBtn
         // 
         this.newPortfolioBtn.Location = new System.Drawing.Point(313, 12);
         this.newPortfolioBtn.Name = "newPortfolioBtn";
         this.newPortfolioBtn.Size = new System.Drawing.Size(75, 23);
         this.newPortfolioBtn.TabIndex = 7;
         this.newPortfolioBtn.Text = "New";
         this.newPortfolioBtn.UseVisualStyleBackColor = true;
         this.newPortfolioBtn.Click += new System.EventHandler(this.newPortfolioBtn_Click);
         // 
         // strategyComboBox
         // 
         this.strategyComboBox.FormattingEnabled = true;
         this.strategyComboBox.Location = new System.Drawing.Point(69, 66);
         this.strategyComboBox.Name = "strategyComboBox";
         this.strategyComboBox.Size = new System.Drawing.Size(238, 21);
         this.strategyComboBox.TabIndex = 32;
         this.strategyComboBox.SelectedIndexChanged += new System.EventHandler(this.strategyComboBox_SelectedIndexChanged);
         // 
         // label10
         // 
         this.label10.AutoSize = true;
         this.label10.Location = new System.Drawing.Point(8, 69);
         this.label10.Name = "label10";
         this.label10.Size = new System.Drawing.Size(46, 13);
         this.label10.TabIndex = 31;
         this.label10.Text = "Strategy";
         // 
         // endDatePicker
         // 
         this.endDatePicker.CustomFormat = "";
         this.endDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
         this.endDatePicker.Location = new System.Drawing.Point(220, 128);
         this.endDatePicker.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
         this.endDatePicker.Name = "endDatePicker";
         this.endDatePicker.Size = new System.Drawing.Size(87, 20);
         this.endDatePicker.TabIndex = 28;
         this.endDatePicker.Value = new System.DateTime(2014, 10, 22, 0, 0, 0, 0);
         // 
         // startDatePicker
         // 
         this.startDatePicker.CustomFormat = "";
         this.startDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
         this.startDatePicker.Location = new System.Drawing.Point(69, 128);
         this.startDatePicker.MinDate = new System.DateTime(1990, 1, 1, 0, 0, 0, 0);
         this.startDatePicker.Name = "startDatePicker";
         this.startDatePicker.Size = new System.Drawing.Size(87, 20);
         this.startDatePicker.TabIndex = 29;
         this.startDatePicker.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(162, 132);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(52, 13);
         this.label2.TabIndex = 26;
         this.label2.Text = "End Date";
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(8, 132);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(55, 13);
         this.label1.TabIndex = 27;
         this.label1.Text = "Start Date";
         // 
         // label6
         // 
         this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(10, 22);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(25, 13);
         this.label6.TabIndex = 22;
         this.label6.Text = "Fee";
         // 
         // taxRateTextBox
         // 
         this.taxRateTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
         this.taxRateTextBox.Location = new System.Drawing.Point(137, 20);
         this.taxRateTextBox.Name = "taxRateTextBox";
         this.taxRateTextBox.Size = new System.Drawing.Size(47, 20);
         this.taxRateTextBox.TabIndex = 19;
         this.taxRateTextBox.Text = "0.0";
         // 
         // label8
         // 
         this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(89, 23);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(42, 13);
         this.label8.TabIndex = 21;
         this.label8.Text = "Tax (%)";
         // 
         // fixedFeeTextBox
         // 
         this.fixedFeeTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
         this.fixedFeeTextBox.Location = new System.Drawing.Point(38, 19);
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
         this.groupBox4.Location = new System.Drawing.Point(11, 164);
         this.groupBox4.Name = "groupBox4";
         this.groupBox4.Size = new System.Drawing.Size(296, 53);
         this.groupBox4.TabIndex = 30;
         this.groupBox4.TabStop = false;
         this.groupBox4.Text = "Charges";
         // 
         // frequencyComboBox
         // 
         this.frequencyComboBox.FormattingEnabled = true;
         this.frequencyComboBox.Location = new System.Drawing.Point(69, 93);
         this.frequencyComboBox.Name = "frequencyComboBox";
         this.frequencyComboBox.Size = new System.Drawing.Size(238, 21);
         this.frequencyComboBox.TabIndex = 34;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(8, 96);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(57, 13);
         this.label4.TabIndex = 33;
         this.label4.Text = "Frequency";
         // 
         // PortfolioSimulatorDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(393, 567);
         this.Controls.Add(this.frequencyComboBox);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.strategyComboBox);
         this.Controls.Add(this.label10);
         this.Controls.Add(this.groupBox4);
         this.Controls.Add(this.endDatePicker);
         this.Controls.Add(this.startDatePicker);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.newPortfolioBtn);
         this.Controls.Add(this.parametrizableControl);
         this.Controls.Add(this.simulateTradingBtn);
         this.Controls.Add(this.watchListComboBox);
         this.Controls.Add(this.portofolioComboBox);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.Portofolio);
         this.Name = "PortfolioSimulatorDlg";
         this.Text = "Portofolio Simulation";
         this.groupBox4.ResumeLayout(false);
         this.groupBox4.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.Label Portofolio;
      private System.Windows.Forms.ComboBox portofolioComboBox;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.ComboBox watchListComboBox;
      private System.Windows.Forms.Button simulateTradingBtn;
      private IndicatorDlgs.ParametrizableControl parametrizableControl;
      private System.Windows.Forms.Button newPortfolioBtn;
      private System.Windows.Forms.ComboBox strategyComboBox;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.DateTimePicker endDatePicker;
      private System.Windows.Forms.DateTimePicker startDatePicker;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.TextBox taxRateTextBox;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.TextBox fixedFeeTextBox;
      private System.Windows.Forms.GroupBox groupBox4;
      private System.Windows.Forms.ComboBox frequencyComboBox;
      private System.Windows.Forms.Label label4;
   }
}