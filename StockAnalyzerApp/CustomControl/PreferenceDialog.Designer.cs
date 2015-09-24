namespace StockAnalyzerApp.CustomControl
{
   partial class PreferenceDialog
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferenceDialog));
         this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
         this.folderBrowserDlg = new System.Windows.Forms.FolderBrowserDialog();
         this.okBtn = new System.Windows.Forms.Button();
         this.cancelBtn = new System.Windows.Forms.Button();
         this.downloadDataCheckBox = new System.Windows.Forms.CheckBox();
         this.settingsBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
         this.intradaySupportCheckBox = new System.Windows.Forms.CheckBox();
         this.settingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
         this.generateBreadthCheckBox = new System.Windows.Forms.CheckBox();
         this.enableLoggingCheckBox = new System.Windows.Forms.CheckBox();
         this.label2 = new System.Windows.Forms.Label();
         this.userIDTextBox = new System.Windows.Forms.TextBox();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.getLicenseButton = new System.Windows.Forms.Button();
         this.msg1 = new System.Windows.Forms.Label();
         this.chartParamGroupBox = new System.Windows.Forms.GroupBox();
         this.barNumberUpDown = new System.Windows.Forms.NumericUpDown();
         this.showVariationCheckBox = new System.Windows.Forms.CheckBox();
         this.barNumberLabel = new System.Windows.Forms.Label();
         this.shortSellSupportCheckBox = new System.Windows.Forms.CheckBox();
         this.groupBox2 = new System.Windows.Forms.GroupBox();
         this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
         this.label1 = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.settingsBindingSource1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.settingsBindingSource)).BeginInit();
         this.groupBox1.SuspendLayout();
         this.chartParamGroupBox.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.barNumberUpDown)).BeginInit();
         this.groupBox2.SuspendLayout();
         this.SuspendLayout();
         // 
         // openFileDlg
         // 
         this.openFileDlg.FileName = "openFileDialog1";
         resources.ApplyResources(this.openFileDlg, "openFileDlg");
         // 
         // folderBrowserDlg
         // 
         resources.ApplyResources(this.folderBrowserDlg, "folderBrowserDlg");
         // 
         // okBtn
         // 
         resources.ApplyResources(this.okBtn, "okBtn");
         this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.okBtn.Name = "okBtn";
         this.okBtn.UseVisualStyleBackColor = true;
         this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
         // 
         // cancelBtn
         // 
         resources.ApplyResources(this.cancelBtn, "cancelBtn");
         this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.cancelBtn.Name = "cancelBtn";
         this.cancelBtn.UseVisualStyleBackColor = true;
         this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
         // 
         // downloadDataCheckBox
         // 
         resources.ApplyResources(this.downloadDataCheckBox, "downloadDataCheckBox");
         this.downloadDataCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.settingsBindingSource1, "DownloadData", true));
         this.downloadDataCheckBox.Name = "downloadDataCheckBox";
         this.downloadDataCheckBox.UseVisualStyleBackColor = true;
         this.downloadDataCheckBox.CheckedChanged += new System.EventHandler(this.downloadDataCheckBox_CheckedChanged);
         // 
         // settingsBindingSource1
         // 
         this.settingsBindingSource1.DataSource = typeof(StockAnalyzerSettings.Properties.Settings);
         // 
         // intradaySupportCheckBox
         // 
         resources.ApplyResources(this.intradaySupportCheckBox, "intradaySupportCheckBox");
         this.intradaySupportCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.settingsBindingSource, "SupportIntraday", true));
         this.intradaySupportCheckBox.Name = "intradaySupportCheckBox";
         this.intradaySupportCheckBox.UseVisualStyleBackColor = true;
         this.intradaySupportCheckBox.CheckedChanged += new System.EventHandler(this.intradaySupportCheckBox_CheckedChanged);
         // 
         // settingsBindingSource
         // 
         this.settingsBindingSource.DataSource = typeof(StockAnalyzerSettings.Properties.Settings);
         // 
         // generateBreadthCheckBox
         // 
         resources.ApplyResources(this.generateBreadthCheckBox, "generateBreadthCheckBox");
         this.generateBreadthCheckBox.Name = "generateBreadthCheckBox";
         this.generateBreadthCheckBox.UseVisualStyleBackColor = true;
         this.generateBreadthCheckBox.CheckedChanged += new System.EventHandler(this.generateBreadthCheckBox_CheckedChanged);
         // 
         // enableLoggingCheckBox
         // 
         resources.ApplyResources(this.enableLoggingCheckBox, "enableLoggingCheckBox");
         this.enableLoggingCheckBox.Name = "enableLoggingCheckBox";
         this.enableLoggingCheckBox.UseVisualStyleBackColor = true;
         // 
         // label2
         // 
         resources.ApplyResources(this.label2, "label2");
         this.label2.Name = "label2";
         // 
         // userIDTextBox
         // 
         resources.ApplyResources(this.userIDTextBox, "userIDTextBox");
         this.userIDTextBox.Name = "userIDTextBox";
         this.userIDTextBox.TextChanged += new System.EventHandler(this.userIDTextBox_TextChanged);
         // 
         // groupBox1
         // 
         resources.ApplyResources(this.groupBox1, "groupBox1");
         this.groupBox1.Controls.Add(this.generateBreadthCheckBox);
         this.groupBox1.Controls.Add(this.downloadDataCheckBox);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.TabStop = false;
         // 
         // getLicenseButton
         // 
         resources.ApplyResources(this.getLicenseButton, "getLicenseButton");
         this.getLicenseButton.Name = "getLicenseButton";
         this.getLicenseButton.UseVisualStyleBackColor = true;
         this.getLicenseButton.Click += new System.EventHandler(this.getLicenseButton_Click);
         // 
         // msg1
         // 
         resources.ApplyResources(this.msg1, "msg1");
         this.msg1.Name = "msg1";
         // 
         // chartParamGroupBox
         // 
         resources.ApplyResources(this.chartParamGroupBox, "chartParamGroupBox");
         this.chartParamGroupBox.Controls.Add(this.barNumberUpDown);
         this.chartParamGroupBox.Controls.Add(this.showVariationCheckBox);
         this.chartParamGroupBox.Controls.Add(this.barNumberLabel);
         this.chartParamGroupBox.Name = "chartParamGroupBox";
         this.chartParamGroupBox.TabStop = false;
         // 
         // barNumberUpDown
         // 
         resources.ApplyResources(this.barNumberUpDown, "barNumberUpDown");
         this.barNumberUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
         this.barNumberUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
         this.barNumberUpDown.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
         this.barNumberUpDown.Name = "barNumberUpDown";
         this.barNumberUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
         // 
         // showVariationCheckBox
         // 
         resources.ApplyResources(this.showVariationCheckBox, "showVariationCheckBox");
         this.showVariationCheckBox.Name = "showVariationCheckBox";
         this.showVariationCheckBox.UseVisualStyleBackColor = true;
         // 
         // barNumberLabel
         // 
         resources.ApplyResources(this.barNumberLabel, "barNumberLabel");
         this.barNumberLabel.Name = "barNumberLabel";
         // 
         // shortSellSupportCheckBox
         // 
         resources.ApplyResources(this.shortSellSupportCheckBox, "shortSellSupportCheckBox");
         this.shortSellSupportCheckBox.Name = "shortSellSupportCheckBox";
         this.shortSellSupportCheckBox.UseVisualStyleBackColor = true;
         // 
         // groupBox2
         // 
         resources.ApplyResources(this.groupBox2, "groupBox2");
         this.groupBox2.Controls.Add(this.dateTimePicker);
         this.groupBox2.Controls.Add(this.label1);
         this.groupBox2.Controls.Add(this.shortSellSupportCheckBox);
         this.groupBox2.Name = "groupBox2";
         this.groupBox2.TabStop = false;
         // 
         // dateTimePicker
         // 
         resources.ApplyResources(this.dateTimePicker, "dateTimePicker");
         this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
         this.dateTimePicker.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
         this.dateTimePicker.Name = "dateTimePicker";
         // 
         // label1
         // 
         resources.ApplyResources(this.label1, "label1");
         this.label1.Name = "label1";
         // 
         // PreferenceDialog
         // 
         this.AcceptButton = this.okBtn;
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.cancelBtn;
         this.Controls.Add(this.groupBox2);
         this.Controls.Add(this.chartParamGroupBox);
         this.Controls.Add(this.msg1);
         this.Controls.Add(this.getLicenseButton);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.userIDTextBox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.intradaySupportCheckBox);
         this.Controls.Add(this.enableLoggingCheckBox);
         this.Controls.Add(this.cancelBtn);
         this.Controls.Add(this.okBtn);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "PreferenceDialog";
         ((System.ComponentModel.ISupportInitialize)(this.settingsBindingSource1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.settingsBindingSource)).EndInit();
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.chartParamGroupBox.ResumeLayout(false);
         this.chartParamGroupBox.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.barNumberUpDown)).EndInit();
         this.groupBox2.ResumeLayout(false);
         this.groupBox2.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.OpenFileDialog openFileDlg;
      private System.Windows.Forms.FolderBrowserDialog folderBrowserDlg;
      private System.Windows.Forms.Button okBtn;
      private System.Windows.Forms.Button cancelBtn;
      private System.Windows.Forms.CheckBox downloadDataCheckBox;
      private System.Windows.Forms.CheckBox intradaySupportCheckBox;
      private System.Windows.Forms.BindingSource settingsBindingSource;
      private System.Windows.Forms.BindingSource settingsBindingSource1;
      private System.Windows.Forms.CheckBox generateBreadthCheckBox;
      private System.Windows.Forms.CheckBox enableLoggingCheckBox;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox userIDTextBox;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Button getLicenseButton;
      private System.Windows.Forms.Label msg1;
      private System.Windows.Forms.GroupBox chartParamGroupBox;
      private System.Windows.Forms.NumericUpDown barNumberUpDown;
      private System.Windows.Forms.CheckBox showVariationCheckBox;
      private System.Windows.Forms.Label barNumberLabel;
      private System.Windows.Forms.CheckBox shortSellSupportCheckBox;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.DateTimePicker dateTimePicker;
      private System.Windows.Forms.Label label1;
   }
}