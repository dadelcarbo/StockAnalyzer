namespace StockAnalyzerApp.CustomControl
{
   partial class DrawingStyleForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DrawingStyleForm));
         this.label2 = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.thicknessComboBox = new System.Windows.Forms.ComboBox();
         this.colorPanel = new System.Windows.Forms.Panel();
         this.lineTypeComboBox = new System.Windows.Forms.ComboBox();
         this.okButton = new System.Windows.Forms.Button();
         this.cancelButton = new System.Windows.Forms.Button();
         this.label3 = new System.Windows.Forms.Label();
         this.SuspendLayout();
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
         // thicknessComboBox
         // 
         this.thicknessComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
         this.thicknessComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         resources.ApplyResources(this.thicknessComboBox, "thicknessComboBox");
         this.thicknessComboBox.FormattingEnabled = true;
         this.thicknessComboBox.Name = "thicknessComboBox";
         this.thicknessComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.thicknessComboBox_DrawItem);
         this.thicknessComboBox.SelectedIndexChanged += new System.EventHandler(this.thicknessComboBox_SelectedIndexChanged);
         // 
         // colorPanel
         // 
         this.colorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         resources.ApplyResources(this.colorPanel, "colorPanel");
         this.colorPanel.Name = "colorPanel";
         this.colorPanel.Click += new System.EventHandler(this.colorPanel_Click);
         // 
         // lineTypeComboBox
         // 
         this.lineTypeComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
         this.lineTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         resources.ApplyResources(this.lineTypeComboBox, "lineTypeComboBox");
         this.lineTypeComboBox.FormattingEnabled = true;
         this.lineTypeComboBox.Name = "lineTypeComboBox";
         this.lineTypeComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lineTypeComboBox_DrawItem);
         this.lineTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.lineTypeComboBox_SelectedIndexChanged);
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
         // label3
         // 
         resources.ApplyResources(this.label3, "label3");
         this.label3.Name = "label3";
         // 
         // DrawingStyleForm
         // 
         this.AcceptButton = this.okButton;
         resources.ApplyResources(this, "$this");
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.cancelButton;
         this.Controls.Add(this.okButton);
         this.Controls.Add(this.cancelButton);
         this.Controls.Add(this.lineTypeComboBox);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.thicknessComboBox);
         this.Controls.Add(this.colorPanel);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "DrawingStyleForm";
         this.Load += new System.EventHandler(this.DrawingStyleForm_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ComboBox thicknessComboBox;
      private System.Windows.Forms.Panel colorPanel;
      private System.Windows.Forms.ComboBox lineTypeComboBox;
      private System.Windows.Forms.Button okButton;
      private System.Windows.Forms.Button cancelButton;
      private System.Windows.Forms.Label label3;

   }
}