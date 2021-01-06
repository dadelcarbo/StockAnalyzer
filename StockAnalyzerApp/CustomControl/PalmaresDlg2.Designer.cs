namespace StockAnalyzerApp.CustomControl
{
   partial class PalmaresDlg2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PalmaresDlg2));
            this.palmaresView = new System.Windows.Forms.ListView();
            this.nameHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.variationHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.openHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.highHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lowHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.closeHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.indicatorHdr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.palmaresContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToWinnerWatchListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToLoserWatchListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.groupComboBox = new System.Windows.Forms.ComboBox();
            this.untilCheckBox = new System.Windows.Forms.CheckBox();
            this.untilDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.indicatorTextBox = new System.Windows.Forms.TextBox();
            this.palmaresContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // palmaresView
            // 
            resources.ApplyResources(this.palmaresView, "palmaresView");
            this.palmaresView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHdr,
            this.variationHdr,
            this.openHdr,
            this.highHdr,
            this.lowHdr,
            this.closeHdr,
            this.indicatorHdr});
            this.palmaresView.ContextMenuStrip = this.palmaresContextMenuStrip;
            this.palmaresView.FullRowSelect = true;
            this.palmaresView.GridLines = true;
            this.palmaresView.Name = "palmaresView";
            this.palmaresView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.palmaresView.UseCompatibleStateImageBehavior = false;
            this.palmaresView.View = System.Windows.Forms.View.Details;
            this.palmaresView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.palmaresView_ColumnClick);
            this.palmaresView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PalmaresDlg_MouseDoubleClick);
            //this.palmaresView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PalmaresDlg_MouseDoubleClick);
            // 
            // nameHdr
            // 
            resources.ApplyResources(this.nameHdr, "nameHdr");
            // 
            // variationHdr
            // 
            resources.ApplyResources(this.variationHdr, "variationHdr");
            // 
            // openHdr
            // 
            resources.ApplyResources(this.openHdr, "openHdr");
            // 
            // highHdr
            // 
            resources.ApplyResources(this.highHdr, "highHdr");
            // 
            // lowHdr
            // 
            resources.ApplyResources(this.lowHdr, "lowHdr");
            // 
            // closeHdr
            // 
            resources.ApplyResources(this.closeHdr, "closeHdr");
            // 
            // ROCEXHdr
            // 
            resources.ApplyResources(this.indicatorHdr, "indicatorHdr");
            // 
            // palmaresContextMenuStrip
            // 
            this.palmaresContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToWinnerWatchListToolStripMenuItem,
            this.addToLoserWatchListToolStripMenuItem,
            this.saveToFileToolStripMenuItem});
            this.palmaresContextMenuStrip.Name = "palmaresContextMenuStrip";
            resources.ApplyResources(this.palmaresContextMenuStrip, "palmaresContextMenuStrip");
            // 
            // addToWinnerWatchListToolStripMenuItem
            // 
            this.addToWinnerWatchListToolStripMenuItem.Name = "addToWinnerWatchListToolStripMenuItem";
            resources.ApplyResources(this.addToWinnerWatchListToolStripMenuItem, "addToWinnerWatchListToolStripMenuItem");
            this.addToWinnerWatchListToolStripMenuItem.Click += new System.EventHandler(this.addToWinnerWatchListToolStripMenuItem_Click);
            // 
            // addToLoserWatchListToolStripMenuItem
            // 
            this.addToLoserWatchListToolStripMenuItem.Name = "addToLoserWatchListToolStripMenuItem";
            resources.ApplyResources(this.addToLoserWatchListToolStripMenuItem, "addToLoserWatchListToolStripMenuItem");
            this.addToLoserWatchListToolStripMenuItem.Click += new System.EventHandler(this.addToLoserWatchListToolStripMenuItem_Click);
            // 
            // saveToFileToolStripMenuItem
            // 
            this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
            resources.ApplyResources(this.saveToFileToolStripMenuItem, "saveToFileToolStripMenuItem");
            this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
            // 
            // fromDateTimePicker
            // 
            this.fromDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.fromDateTimePicker, "fromDateTimePicker");
            this.fromDateTimePicker.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
            this.fromDateTimePicker.Name = "fromDateTimePicker";
            this.fromDateTimePicker.Value = new System.DateTime(2006, 1, 1, 0, 0, 0, 0);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupComboBox
            // 
            this.groupComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.groupComboBox, "groupComboBox");
            this.groupComboBox.Name = "groupComboBox";
            // 
            // untilCheckBox
            // 
            resources.ApplyResources(this.untilCheckBox, "untilCheckBox");
            this.untilCheckBox.Name = "untilCheckBox";
            this.untilCheckBox.UseVisualStyleBackColor = true;
            this.untilCheckBox.CheckedChanged += new System.EventHandler(this.untilCheckBox_CheckedChanged);
            // 
            // untilDateTimePicker
            // 
            this.untilDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.untilDateTimePicker, "untilDateTimePicker");
            this.untilDateTimePicker.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
            this.untilDateTimePicker.Name = "untilDateTimePicker";
            this.untilDateTimePicker.Value = new System.DateTime(2006, 1, 1, 0, 0, 0, 0);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // indicatorTextBox
            // 
            resources.ApplyResources(this.indicatorTextBox, "indicatorTextBox");
            this.indicatorTextBox.Name = "indicatorTextBox";
            this.indicatorTextBox.LostFocus += new System.EventHandler(this.indicatorTextBox_TextChanged);
            // 
            // PalmaresDlg
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.indicatorTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupComboBox);
            this.Controls.Add(this.untilCheckBox);
            this.Controls.Add(this.untilDateTimePicker);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.palmaresView);
            this.Controls.Add(this.fromDateTimePicker);
            this.Name = "PalmaresDlg";
            this.palmaresContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
      }
      #endregion

      private System.Windows.Forms.ListView palmaresView;
      private System.Windows.Forms.DateTimePicker fromDateTimePicker;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ColumnHeader nameHdr;
      private System.Windows.Forms.ColumnHeader variationHdr;
      private System.Windows.Forms.ColumnHeader openHdr;
      private System.Windows.Forms.ColumnHeader closeHdr;
      private System.Windows.Forms.ColumnHeader lowHdr;
      private System.Windows.Forms.ColumnHeader highHdr;
      private System.Windows.Forms.ColumnHeader indicatorHdr;
      private System.Windows.Forms.ComboBox groupComboBox;
      private System.Windows.Forms.CheckBox untilCheckBox;
      private System.Windows.Forms.DateTimePicker untilDateTimePicker;
      private System.Windows.Forms.ContextMenuStrip palmaresContextMenuStrip;
      private System.Windows.Forms.ToolStripMenuItem addToWinnerWatchListToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem addToLoserWatchListToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox indicatorTextBox;
   }
}