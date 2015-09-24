namespace StockAnalyzerApp.CustomControl
{
   partial class StockEventSelectorDlg
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
         this.eventSelectorCheckedListBox = new System.Windows.Forms.CheckedListBox();
         this.OKButton = new System.Windows.Forms.Button();
         this.cancelButton = new System.Windows.Forms.Button();
         this.allEventsBtn = new System.Windows.Forms.RadioButton();
         this.oneEventBtn = new System.Windows.Forms.RadioButton();
         this.SuspendLayout();
         // 
         // eventSelectorCheckedListBox
         // 
         this.eventSelectorCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.eventSelectorCheckedListBox.CheckOnClick = true;
         this.eventSelectorCheckedListBox.FormattingEnabled = true;
         this.eventSelectorCheckedListBox.Location = new System.Drawing.Point(12, 12);
         this.eventSelectorCheckedListBox.Name = "eventSelectorCheckedListBox";
         this.eventSelectorCheckedListBox.Size = new System.Drawing.Size(210, 244);
         this.eventSelectorCheckedListBox.TabIndex = 0;
         // 
         // OKButton
         // 
         this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.OKButton.Location = new System.Drawing.Point(147, 262);
         this.OKButton.Name = "OKButton";
         this.OKButton.Size = new System.Drawing.Size(75, 23);
         this.OKButton.TabIndex = 1;
         this.OKButton.Text = "OK";
         this.OKButton.UseVisualStyleBackColor = true;
         this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
         // 
         // cancelButton
         // 
         this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.cancelButton.Location = new System.Drawing.Point(147, 291);
         this.cancelButton.Name = "cancelButton";
         this.cancelButton.Size = new System.Drawing.Size(75, 23);
         this.cancelButton.TabIndex = 2;
         this.cancelButton.Text = "Cancel";
         this.cancelButton.UseVisualStyleBackColor = true;
         // 
         // allEventsBtn
         // 
         this.allEventsBtn.AutoSize = true;
         this.allEventsBtn.Checked = true;
         this.allEventsBtn.Location = new System.Drawing.Point(12, 262);
         this.allEventsBtn.Name = "allEventsBtn";
         this.allEventsBtn.Size = new System.Drawing.Size(36, 17);
         this.allEventsBtn.TabIndex = 3;
         this.allEventsBtn.TabStop = true;
         this.allEventsBtn.Text = "All";
         this.allEventsBtn.UseVisualStyleBackColor = true;
         // 
         // oneEventBtn
         // 
         this.oneEventBtn.AutoSize = true;
         this.oneEventBtn.Location = new System.Drawing.Point(12, 291);
         this.oneEventBtn.Name = "oneEventBtn";
         this.oneEventBtn.Size = new System.Drawing.Size(87, 17);
         this.oneEventBtn.TabIndex = 3;
         this.oneEventBtn.Text = "At Least One";
         this.oneEventBtn.UseVisualStyleBackColor = true;
         // 
         // StockEventSelectorDlg
         // 
         this.AcceptButton = this.OKButton;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.cancelButton;
         this.ClientSize = new System.Drawing.Size(234, 325);
         this.ControlBox = false;
         this.Controls.Add(this.oneEventBtn);
         this.Controls.Add(this.allEventsBtn);
         this.Controls.Add(this.cancelButton);
         this.Controls.Add(this.OKButton);
         this.Controls.Add(this.eventSelectorCheckedListBox);
         this.Name = "StockEventSelectorDlg";
         this.ShowInTaskbar = false;
         this.Text = "Event Selector";
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.CheckedListBox eventSelectorCheckedListBox;
      private System.Windows.Forms.Button OKButton;
      private System.Windows.Forms.Button cancelButton;
      private System.Windows.Forms.RadioButton allEventsBtn;
      private System.Windows.Forms.RadioButton oneEventBtn;
   }
}