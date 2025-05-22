namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
   partial class AddAutoDrawingDlg
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddAutoDrawingDlg));
         this.autoDrawingComboBox = new System.Windows.Forms.ComboBox();
         this.okButton = new System.Windows.Forms.Button();
         this.cancelButton = new System.Windows.Forms.Button();
         this.descriptionTextBox = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // autoDrawingComboBox
         // 
         this.autoDrawingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.autoDrawingComboBox.FormattingEnabled = true;
         resources.ApplyResources(this.autoDrawingComboBox, "autoDrawingComboBox");
         this.autoDrawingComboBox.Name = "autoDrawingComboBox";
         this.autoDrawingComboBox.SelectedIndexChanged += new System.EventHandler(this.autoDrawingComboBox_SelectedIndexChanged);
         // 
         // okButton
         // 
         this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
         resources.ApplyResources(this.okButton, "okButton");
         this.okButton.Name = "okButton";
         this.okButton.UseVisualStyleBackColor = true;
         // 
         // cancelButton
         // 
         this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         resources.ApplyResources(this.cancelButton, "cancelButton");
         this.cancelButton.Name = "cancelButton";
         this.cancelButton.UseVisualStyleBackColor = true;
         // 
         // descriptionTextBox
         // 
         resources.ApplyResources(this.descriptionTextBox, "descriptionTextBox");
         this.descriptionTextBox.Name = "descriptionTextBox";
         // 
         // label2
         // 
         resources.ApplyResources(this.label2, "label2");
         this.label2.Name = "label2";
         // 
         // label1
         // 
         resources.ApplyResources(this.label1, "label1");
         this.label1.Name = "label1";
         // 
         // AddAutoDrawingDlg
         // 
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ControlBox = false;
         this.Controls.Add(this.label1);
         this.Controls.Add(this.descriptionTextBox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.cancelButton);
         this.Controls.Add(this.okButton);
         this.Controls.Add(this.autoDrawingComboBox);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "AddAutoDrawingDlg";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ComboBox autoDrawingComboBox;
      private System.Windows.Forms.Button okButton;
      private System.Windows.Forms.Button cancelButton;
      private System.Windows.Forms.TextBox descriptionTextBox;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
   }
}