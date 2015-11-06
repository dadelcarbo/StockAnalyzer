namespace StockAnalyzerApp.CustomControl.WatchListDlgs
{
   partial class StockSelectorDlg
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
         this.groupComboBox = new System.Windows.Forms.ComboBox();
         this.label1 = new System.Windows.Forms.Label();
         this.stockListBox = new System.Windows.Forms.CheckedListBox();
         this.cancelButton = new System.Windows.Forms.Button();
         this.okButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // groupComboBox
         // 
         this.groupComboBox.FormattingEnabled = true;
         this.groupComboBox.Location = new System.Drawing.Point(54, 13);
         this.groupComboBox.Name = "groupComboBox";
         this.groupComboBox.Size = new System.Drawing.Size(218, 21);
         this.groupComboBox.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 16);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(36, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Group";
         // 
         // stockListBox
         // 
         this.stockListBox.CheckOnClick = true;
         this.stockListBox.FormattingEnabled = true;
         this.stockListBox.Location = new System.Drawing.Point(12, 40);
         this.stockListBox.Name = "stockListBox";
         this.stockListBox.Size = new System.Drawing.Size(260, 544);
         this.stockListBox.TabIndex = 2;
         // 
         // cancelButton
         // 
         this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.cancelButton.Location = new System.Drawing.Point(198, 590);
         this.cancelButton.Name = "cancelButton";
         this.cancelButton.Size = new System.Drawing.Size(75, 23);
         this.cancelButton.TabIndex = 3;
         this.cancelButton.Text = "Cancel";
         this.cancelButton.UseVisualStyleBackColor = true;
         this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
         // 
         // okButton
         // 
         this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.okButton.Location = new System.Drawing.Point(117, 590);
         this.okButton.Name = "okButton";
         this.okButton.Size = new System.Drawing.Size(75, 23);
         this.okButton.TabIndex = 3;
         this.okButton.Text = "OK";
         this.okButton.UseVisualStyleBackColor = true;
         this.okButton.Click += new System.EventHandler(this.okButton_Click);
         // 
         // StockSelectorDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(285, 619);
         this.Controls.Add(this.okButton);
         this.Controls.Add(this.cancelButton);
         this.Controls.Add(this.stockListBox);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.groupComboBox);
         this.Name = "StockSelectorDlg";
         this.Text = "StockSelectorDlg";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ComboBox groupComboBox;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.CheckedListBox stockListBox;
      private System.Windows.Forms.Button cancelButton;
      private System.Windows.Forms.Button okButton;
   }
}