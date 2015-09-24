namespace StockAnalyzerApp.CustomControl
{
   partial class CommentDialog
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommentDialog));
         this.commentBox = new System.Windows.Forms.RichTextBox();
         this.clearAllBtn = new System.Windows.Forms.Button();
         this.dateComboBox = new System.Windows.Forms.ComboBox();
         this.closeButton = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // commentBox
         // 
         this.commentBox.AcceptsTab = true;
         resources.ApplyResources(this.commentBox, "commentBox");
         this.commentBox.AllowDrop = true;
         this.commentBox.Name = "commentBox";
         this.commentBox.Validated += new System.EventHandler(this.commentBox_Validated);
         // 
         // clearAllBtn
         // 
         resources.ApplyResources(this.clearAllBtn, "clearAllBtn");
         this.clearAllBtn.Name = "clearAllBtn";
         this.clearAllBtn.UseVisualStyleBackColor = true;
         this.clearAllBtn.Click += new System.EventHandler(this.clearAllBtn_Click);
         // 
         // dateComboBox
         // 
         resources.ApplyResources(this.dateComboBox, "dateComboBox");
         this.dateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.dateComboBox.FormattingEnabled = true;
         this.dateComboBox.Name = "dateComboBox";
         this.dateComboBox.SelectedIndexChanged += new System.EventHandler(this.dateComboBox_SelectedIndexChanged);
         // 
         // closeButton
         // 
         resources.ApplyResources(this.closeButton, "closeButton");
         this.closeButton.Name = "closeButton";
         this.closeButton.UseVisualStyleBackColor = true;
         this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
         // 
         // label1
         // 
         resources.ApplyResources(this.label1, "label1");
         this.label1.Name = "label1";
         // 
         // CommentDialog
         // 
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.label1);
         this.Controls.Add(this.closeButton);
         this.Controls.Add(this.dateComboBox);
         this.Controls.Add(this.clearAllBtn);
         this.Controls.Add(this.commentBox);
         this.Name = "CommentDialog";
         this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
         this.TopMost = true;
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      public System.Windows.Forms.RichTextBox commentBox;
      private System.Windows.Forms.Button clearAllBtn;
      private System.Windows.Forms.ComboBox dateComboBox;
      private System.Windows.Forms.Button closeButton;
      private System.Windows.Forms.Label label1;
   }
}