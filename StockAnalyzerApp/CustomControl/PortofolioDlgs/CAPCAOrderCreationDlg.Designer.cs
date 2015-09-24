namespace StockAnalyzerApp.CustomControl
{
   partial class CAPCAOrderCreationDlg
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
         this.ordersTextBox = new System.Windows.Forms.TextBox();
         this.createOrdersBtn = new System.Windows.Forms.Button();
         this.closeBtn = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // ordersTextBox
         // 
         this.ordersTextBox.Location = new System.Drawing.Point(12, 12);
         this.ordersTextBox.Multiline = true;
         this.ordersTextBox.Name = "ordersTextBox";
         this.ordersTextBox.Size = new System.Drawing.Size(688, 218);
         this.ordersTextBox.TabIndex = 0;
         // 
         // createOrdersBtn
         // 
         this.createOrdersBtn.Location = new System.Drawing.Point(504, 237);
         this.createOrdersBtn.Name = "createOrdersBtn";
         this.createOrdersBtn.Size = new System.Drawing.Size(98, 23);
         this.createOrdersBtn.TabIndex = 1;
         this.createOrdersBtn.Text = "Create orders";
         this.createOrdersBtn.UseVisualStyleBackColor = true;
         this.createOrdersBtn.Click += new System.EventHandler(this.createOrdersBtn_Click);
         // 
         // closeBtn
         // 
         this.closeBtn.Location = new System.Drawing.Point(608, 237);
         this.closeBtn.Name = "closeBtn";
         this.closeBtn.Size = new System.Drawing.Size(92, 23);
         this.closeBtn.TabIndex = 2;
         this.closeBtn.Text = "Save and close";
         this.closeBtn.UseVisualStyleBackColor = true;
         this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
         // 
         // CAPCAOrderCreationDlg
         // 
         this.AcceptButton = this.createOrdersBtn;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(712, 272);
         this.Controls.Add(this.closeBtn);
         this.Controls.Add(this.createOrdersBtn);
         this.Controls.Add(this.ordersTextBox);
         this.Name = "CAPCAOrderCreationDlg";
         this.Text = "CA-CPA Order Creation";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox ordersTextBox;
      private System.Windows.Forms.Button createOrdersBtn;
      private System.Windows.Forms.Button closeBtn;
   }
}