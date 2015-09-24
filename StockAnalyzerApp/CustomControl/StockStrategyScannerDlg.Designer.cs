namespace StockAnalyzerApp.CustomControl
{
   partial class StockStrategyScannerDlg
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StockStrategyScannerDlg));
         this.groupComboBox = new System.Windows.Forms.ComboBox();
         this.label1 = new System.Windows.Forms.Label();
         this.selectedStockListBox = new System.Windows.Forms.ListBox();
         this.selectButton = new System.Windows.Forms.Button();
         this.clearButton = new System.Windows.Forms.Button();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.label4 = new System.Windows.Forms.Label();
         this.periodComboBox = new System.Windows.Forms.ComboBox();
         this.progressBar = new System.Windows.Forms.ProgressBar();
         this.label3 = new System.Windows.Forms.Label();
         this.progressLabel = new System.Windows.Forms.Label();
         this.reloadButton = new System.Windows.Forms.Button();
         this.refreshDataCheckBox = new System.Windows.Forms.CheckBox();
         this.strategyComboBox = new System.Windows.Forms.ComboBox();
         this.label2 = new System.Windows.Forms.Label();
         this.groupBox1.SuspendLayout();
         this.SuspendLayout();
         // 
         // groupComboBox
         // 
         resources.ApplyResources(this.groupComboBox, "groupComboBox");
         this.groupComboBox.FormattingEnabled = true;
         this.groupComboBox.Name = "groupComboBox";
         // 
         // label1
         // 
         resources.ApplyResources(this.label1, "label1");
         this.label1.Name = "label1";
         // 
         // selectedStockListBox
         // 
         resources.ApplyResources(this.selectedStockListBox, "selectedStockListBox");
         this.selectedStockListBox.FormattingEnabled = true;
         this.selectedStockListBox.Name = "selectedStockListBox";
         this.selectedStockListBox.SelectedValueChanged += new System.EventHandler(this.selectedStockListBox_SelectedValueChanged);
         // 
         // selectButton
         // 
         resources.ApplyResources(this.selectButton, "selectButton");
         this.selectButton.Name = "selectButton";
         this.selectButton.UseVisualStyleBackColor = true;
         this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
         // 
         // clearButton
         // 
         resources.ApplyResources(this.clearButton, "clearButton");
         this.clearButton.Name = "clearButton";
         this.clearButton.UseVisualStyleBackColor = true;
         this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
         // 
         // groupBox1
         // 
         resources.ApplyResources(this.groupBox1, "groupBox1");
         this.groupBox1.Controls.Add(this.label4);
         this.groupBox1.Controls.Add(this.periodComboBox);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.TabStop = false;
         // 
         // label4
         // 
         resources.ApplyResources(this.label4, "label4");
         this.label4.Name = "label4";
         // 
         // periodComboBox
         // 
         resources.ApplyResources(this.periodComboBox, "periodComboBox");
         this.periodComboBox.FormattingEnabled = true;
         this.periodComboBox.Items.AddRange(new object[] {
            ((object)(resources.GetObject("periodComboBox.Items"))),
            ((object)(resources.GetObject("periodComboBox.Items1"))),
            ((object)(resources.GetObject("periodComboBox.Items2"))),
            ((object)(resources.GetObject("periodComboBox.Items3"))),
            ((object)(resources.GetObject("periodComboBox.Items4")))});
         this.periodComboBox.Name = "periodComboBox";
         // 
         // progressBar
         // 
         resources.ApplyResources(this.progressBar, "progressBar");
         this.progressBar.Name = "progressBar";
         this.progressBar.Step = 1;
         this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
         // 
         // label3
         // 
         resources.ApplyResources(this.label3, "label3");
         this.label3.Name = "label3";
         // 
         // progressLabel
         // 
         resources.ApplyResources(this.progressLabel, "progressLabel");
         this.progressLabel.AutoEllipsis = true;
         this.progressLabel.Name = "progressLabel";
         // 
         // reloadButton
         // 
         resources.ApplyResources(this.reloadButton, "reloadButton");
         this.reloadButton.Image = global::StockAnalyzerApp.Properties.Resources.Reload;
         this.reloadButton.Name = "reloadButton";
         this.reloadButton.UseVisualStyleBackColor = true;
         this.reloadButton.Click += new System.EventHandler(this.reloadButton_Click);
         // 
         // refreshDataCheckBox
         // 
         resources.ApplyResources(this.refreshDataCheckBox, "refreshDataCheckBox");
         this.refreshDataCheckBox.Name = "refreshDataCheckBox";
         this.refreshDataCheckBox.UseVisualStyleBackColor = true;
         // 
         // strategyComboBox
         // 
         resources.ApplyResources(this.strategyComboBox, "strategyComboBox");
         this.strategyComboBox.FormattingEnabled = true;
         this.strategyComboBox.Name = "strategyComboBox";
         // 
         // label2
         // 
         resources.ApplyResources(this.label2, "label2");
         this.label2.Name = "label2";
         // 
         // StockStrategyScannerDlg
         // 
         this.AcceptButton = this.selectButton;
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.refreshDataCheckBox);
         this.Controls.Add(this.reloadButton);
         this.Controls.Add(this.progressLabel);
         this.Controls.Add(this.progressBar);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.clearButton);
         this.Controls.Add(this.selectButton);
         this.Controls.Add(this.selectedStockListBox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.strategyComboBox);
         this.Controls.Add(this.groupComboBox);
         this.Name = "StockStrategyScannerDlg";
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.ComboBox groupComboBox;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ListBox selectedStockListBox;
      private System.Windows.Forms.Button selectButton;
      private System.Windows.Forms.Button clearButton;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.ProgressBar progressBar;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label progressLabel;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.ComboBox periodComboBox;
      private System.Windows.Forms.Button reloadButton;
      private System.Windows.Forms.CheckBox refreshDataCheckBox;
      private System.Windows.Forms.ComboBox strategyComboBox;
      private System.Windows.Forms.Label label2;
   }
}