namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
   partial class EuronextDataProviderConfigDlg
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EuronextDataProviderConfigDlg));
         this.personalListView = new System.Windows.Forms.ListView();
         this.isincolumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.symbolColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.groupColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.okButton = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.isinTextBox = new System.Windows.Forms.TextBox();
         this.testButton = new System.Windows.Forms.Button();
         this.addButton = new System.Windows.Forms.Button();
         this.label2 = new System.Windows.Forms.Label();
         this.nameTextBox = new System.Windows.Forms.TextBox();
         this.groupComboBox = new System.Windows.Forms.ComboBox();
         this.step1GroupBox = new System.Windows.Forms.GroupBox();
         this.step2GroupBox = new System.Windows.Forms.GroupBox();
         this.label4 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.backToStep1Button = new System.Windows.Forms.Button();
         this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.cancelButton = new System.Windows.Forms.Button();
         this.step1GroupBox.SuspendLayout();
         this.step2GroupBox.SuspendLayout();
         this.contextMenuStrip.SuspendLayout();
         this.SuspendLayout();
         // 
         // personalListView
         // 
         resources.ApplyResources(this.personalListView, "personalListView");
         this.personalListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.isincolumnHeader,
            this.symbolColumnHeader,
            this.nameColumnHeader,
            this.groupColumnHeader});
         this.personalListView.FullRowSelect = true;
         this.personalListView.Name = "personalListView";
         this.personalListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
         this.personalListView.UseCompatibleStateImageBehavior = false;
         this.personalListView.View = System.Windows.Forms.View.Details;
         this.personalListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.personalListView_KeyDown);
         this.personalListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.personalListView_MouseClick);
         // 
         // isincolumnHeader
         // 
         resources.ApplyResources(this.isincolumnHeader, "isincolumnHeader");
         // 
         // symbolColumnHeader
         // 
         resources.ApplyResources(this.symbolColumnHeader, "symbolColumnHeader");
         // 
         // nameColumnHeader
         // 
         resources.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
         // 
         // groupColumnHeader
         // 
         resources.ApplyResources(this.groupColumnHeader, "groupColumnHeader");
         // 
         // okButton
         // 
         resources.ApplyResources(this.okButton, "okButton");
         this.okButton.Name = "okButton";
         this.okButton.UseVisualStyleBackColor = true;
         this.okButton.Click += new System.EventHandler(this.okButton_Click);
         // 
         // label1
         // 
         resources.ApplyResources(this.label1, "label1");
         this.label1.Name = "label1";
         // 
         // isinTextBox
         // 
         resources.ApplyResources(this.isinTextBox, "isinTextBox");
         this.isinTextBox.Name = "isinTextBox";
         // 
         // testButton
         // 
         resources.ApplyResources(this.testButton, "testButton");
         this.testButton.Name = "testButton";
         this.testButton.UseVisualStyleBackColor = true;
         this.testButton.Click += new System.EventHandler(this.testButton_Click);
         // 
         // addButton
         // 
         resources.ApplyResources(this.addButton, "addButton");
         this.addButton.Name = "addButton";
         this.addButton.UseVisualStyleBackColor = true;
         this.addButton.Click += new System.EventHandler(this.addButton_Click);
         // 
         // label2
         // 
         resources.ApplyResources(this.label2, "label2");
         this.label2.Name = "label2";
         // 
         // nameTextBox
         // 
         resources.ApplyResources(this.nameTextBox, "nameTextBox");
         this.nameTextBox.Name = "nameTextBox";
         // 
         // groupComboBox
         // 
         resources.ApplyResources(this.groupComboBox, "groupComboBox");
         this.groupComboBox.FormattingEnabled = true;
         this.groupComboBox.Name = "groupComboBox";
         // 
         // step1GroupBox
         // 
         resources.ApplyResources(this.step1GroupBox, "step1GroupBox");
         this.step1GroupBox.Controls.Add(this.isinTextBox);
         this.step1GroupBox.Controls.Add(this.label1);
         this.step1GroupBox.Controls.Add(this.testButton);
         this.step1GroupBox.Name = "step1GroupBox";
         this.step1GroupBox.TabStop = false;
         // 
         // step2GroupBox
         // 
         resources.ApplyResources(this.step2GroupBox, "step2GroupBox");
         this.step2GroupBox.Controls.Add(this.label4);
         this.step2GroupBox.Controls.Add(this.label3);
         this.step2GroupBox.Controls.Add(this.nameTextBox);
         this.step2GroupBox.Controls.Add(this.backToStep1Button);
         this.step2GroupBox.Controls.Add(this.addButton);
         this.step2GroupBox.Controls.Add(this.groupComboBox);
         this.step2GroupBox.Name = "step2GroupBox";
         this.step2GroupBox.TabStop = false;
         // 
         // label4
         // 
         resources.ApplyResources(this.label4, "label4");
         this.label4.Name = "label4";
         // 
         // label3
         // 
         resources.ApplyResources(this.label3, "label3");
         this.label3.Name = "label3";
         // 
         // backToStep1Button
         // 
         resources.ApplyResources(this.backToStep1Button, "backToStep1Button");
         this.backToStep1Button.Name = "backToStep1Button";
         this.backToStep1Button.UseVisualStyleBackColor = true;
         this.backToStep1Button.Click += new System.EventHandler(this.backToStep1Button_Click);
         // 
         // contextMenuStrip
         // 
         resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
         this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
         this.contextMenuStrip.Name = "contextMenuStrip";
         // 
         // removeToolStripMenuItem
         // 
         resources.ApplyResources(this.removeToolStripMenuItem, "removeToolStripMenuItem");
         this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
         this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
         // 
         // cancelButton
         // 
         resources.ApplyResources(this.cancelButton, "cancelButton");
         this.cancelButton.Name = "cancelButton";
         this.cancelButton.UseVisualStyleBackColor = true;
         this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
         // 
         // EuronextDataProviderConfigDlg
         // 
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.step2GroupBox);
         this.Controls.Add(this.step1GroupBox);
         this.Controls.Add(this.okButton);
         this.Controls.Add(this.cancelButton);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.personalListView);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "EuronextDataProviderConfigDlg";
         this.step1GroupBox.ResumeLayout(false);
         this.step1GroupBox.PerformLayout();
         this.step2GroupBox.ResumeLayout(false);
         this.step2GroupBox.PerformLayout();
         this.contextMenuStrip.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.ListView personalListView;
      private System.Windows.Forms.Button okButton;
      private System.Windows.Forms.ColumnHeader symbolColumnHeader;
      private System.Windows.Forms.ColumnHeader nameColumnHeader;
      private System.Windows.Forms.ColumnHeader groupColumnHeader;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox isinTextBox;
      private System.Windows.Forms.Button testButton;
      private System.Windows.Forms.Button addButton;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox nameTextBox;
      private System.Windows.Forms.ComboBox groupComboBox;
      private System.Windows.Forms.GroupBox step1GroupBox;
      private System.Windows.Forms.GroupBox step2GroupBox;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
      private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
      private System.Windows.Forms.Button backToStep1Button;
      private System.Windows.Forms.Button cancelButton;
      private System.Windows.Forms.ColumnHeader isincolumnHeader;
   }
}