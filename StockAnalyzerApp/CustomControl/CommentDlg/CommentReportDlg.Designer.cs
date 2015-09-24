namespace StockAnalyzerApp.CustomControl.CommentDlg
{
   partial class CommentReportDlg
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
         this.startDateTimePicker = new System.Windows.Forms.DateTimePicker();
         this.endDateTimePicker = new System.Windows.Forms.DateTimePicker();
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.commentReportTextBox = new System.Windows.Forms.TextBox();
         this.closeBtn = new System.Windows.Forms.Button();
         this.sendEmailBtn = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // startDateTimePicker
         // 
         this.startDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
         this.startDateTimePicker.Location = new System.Drawing.Point(49, 8);
         this.startDateTimePicker.Name = "startDateTimePicker";
         this.startDateTimePicker.Size = new System.Drawing.Size(90, 20);
         this.startDateTimePicker.TabIndex = 0;
         this.startDateTimePicker.ValueChanged += new System.EventHandler(this.startDateTimePicker_ValueChanged);
         // 
         // endDateTimePicker
         // 
         this.endDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
         this.endDateTimePicker.Location = new System.Drawing.Point(171, 8);
         this.endDateTimePicker.Name = "endDateTimePicker";
         this.endDateTimePicker.Size = new System.Drawing.Size(89, 20);
         this.endDateTimePicker.TabIndex = 0;
         this.endDateTimePicker.ValueChanged += new System.EventHandler(this.endDateTimePicker_ValueChanged);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(13, 12);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(30, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "From";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(145, 12);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(20, 13);
         this.label2.TabIndex = 1;
         this.label2.Text = "To";
         // 
         // commentReportTextBox
         // 
         this.commentReportTextBox.AcceptsReturn = true;
         this.commentReportTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.commentReportTextBox.Location = new System.Drawing.Point(12, 34);
         this.commentReportTextBox.Multiline = true;
         this.commentReportTextBox.Name = "commentReportTextBox";
         this.commentReportTextBox.Size = new System.Drawing.Size(392, 298);
         this.commentReportTextBox.TabIndex = 2;
         // 
         // closeBtn
         // 
         this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.closeBtn.Location = new System.Drawing.Point(328, 338);
         this.closeBtn.Name = "closeBtn";
         this.closeBtn.Size = new System.Drawing.Size(75, 23);
         this.closeBtn.TabIndex = 3;
         this.closeBtn.Text = "Close";
         this.closeBtn.UseVisualStyleBackColor = true;
         this.closeBtn.Click += new System.EventHandler(this.button1_Click);
         // 
         // sendEmailBtn
         // 
         this.sendEmailBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.sendEmailBtn.Location = new System.Drawing.Point(247, 338);
         this.sendEmailBtn.Name = "sendEmailBtn";
         this.sendEmailBtn.Size = new System.Drawing.Size(75, 23);
         this.sendEmailBtn.TabIndex = 3;
         this.sendEmailBtn.Text = "Send email";
         this.sendEmailBtn.UseVisualStyleBackColor = true;
         this.sendEmailBtn.Click += new System.EventHandler(this.button2_Click);
         // 
         // CommentReportDlg
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(416, 373);
         this.Controls.Add(this.sendEmailBtn);
         this.Controls.Add(this.closeBtn);
         this.Controls.Add(this.commentReportTextBox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.endDateTimePicker);
         this.Controls.Add(this.startDateTimePicker);
         this.Name = "CommentReportDlg";
         this.Text = "Comment Report";
         this.Load += new System.EventHandler(this.CommentReportDlg_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.DateTimePicker startDateTimePicker;
      private System.Windows.Forms.DateTimePicker endDateTimePicker;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox commentReportTextBox;
      private System.Windows.Forms.Button closeBtn;
      private System.Windows.Forms.Button sendEmailBtn;
   }
}