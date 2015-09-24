namespace StockAnalyzerApp.CustomControl
{
   partial class SaveThemeForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveThemeForm));
         this.cancelBtn = new System.Windows.Forms.Button();
         this.okBtn = new System.Windows.Forms.Button();
         this.replaceRadioButton = new System.Windows.Forms.RadioButton();
         this.newRadioButton = new System.Windows.Forms.RadioButton();
         this.themeComboBox = new System.Windows.Forms.ComboBox();
         this.themeTextBox = new System.Windows.Forms.TextBox();
         this.areYouSureLabel = new System.Windows.Forms.Label();
         this.invalidThemeLabel = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // cancelBtn
         // 
         resources.ApplyResources(this.cancelBtn, "cancelBtn");
         this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.cancelBtn.Name = "cancelBtn";
         this.cancelBtn.UseVisualStyleBackColor = true;
         this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
         // 
         // okBtn
         // 
         resources.ApplyResources(this.okBtn, "okBtn");
         this.okBtn.Name = "okBtn";
         this.okBtn.UseVisualStyleBackColor = true;
         this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
         // 
         // replaceRadioButton
         // 
         resources.ApplyResources(this.replaceRadioButton, "replaceRadioButton");
         this.replaceRadioButton.Checked = true;
         this.replaceRadioButton.Name = "replaceRadioButton";
         this.replaceRadioButton.TabStop = true;
         this.replaceRadioButton.UseVisualStyleBackColor = true;
         this.replaceRadioButton.CheckedChanged += new System.EventHandler(this.replaceRadioButton_CheckedChanged);
         // 
         // newRadioButton
         // 
         resources.ApplyResources(this.newRadioButton, "newRadioButton");
         this.newRadioButton.Name = "newRadioButton";
         this.newRadioButton.UseVisualStyleBackColor = true;
         this.newRadioButton.CheckedChanged += new System.EventHandler(this.newRadioButton_CheckedChanged);
         // 
         // themeComboBox
         // 
         this.themeComboBox.FormattingEnabled = true;
         resources.ApplyResources(this.themeComboBox, "themeComboBox");
         this.themeComboBox.Name = "themeComboBox";
         // 
         // themeTextBox
         // 
         resources.ApplyResources(this.themeTextBox, "themeTextBox");
         this.themeTextBox.Name = "themeTextBox";
         // 
         // areYouSureLabel
         // 
         resources.ApplyResources(this.areYouSureLabel, "areYouSureLabel");
         this.areYouSureLabel.Name = "areYouSureLabel";
         // 
         // invalidThemeLabel
         // 
         resources.ApplyResources(this.invalidThemeLabel, "invalidThemeLabel");
         this.invalidThemeLabel.Name = "invalidThemeLabel";
         // 
         // SaveThemeForm
         // 
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.cancelBtn;
         this.Controls.Add(this.invalidThemeLabel);
         this.Controls.Add(this.areYouSureLabel);
         this.Controls.Add(this.themeTextBox);
         this.Controls.Add(this.themeComboBox);
         this.Controls.Add(this.newRadioButton);
         this.Controls.Add(this.replaceRadioButton);
         this.Controls.Add(this.cancelBtn);
         this.Controls.Add(this.okBtn);
         this.Name = "SaveThemeForm";
         this.TopMost = true;
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button cancelBtn;
      private System.Windows.Forms.Button okBtn;
      private System.Windows.Forms.RadioButton replaceRadioButton;
      private System.Windows.Forms.RadioButton newRadioButton;
      private System.Windows.Forms.ComboBox themeComboBox;
      private System.Windows.Forms.TextBox themeTextBox;
      private System.Windows.Forms.Label areYouSureLabel;
      private System.Windows.Forms.Label invalidThemeLabel;
   }
}