namespace StockAnalyzerApp.CustomControl
{
   partial class StockbrokersOrderCreationDlg
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
         this.closebtn = new System.Windows.Forms.Button();
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
         this.createOrdersBtn.Location = new System.Drawing.Point(501, 237);
         this.createOrdersBtn.Name = "createOrdersBtn";
         this.createOrdersBtn.Size = new System.Drawing.Size(98, 23);
         this.createOrdersBtn.TabIndex = 1;
         this.createOrdersBtn.Text = "Create orders";
         this.createOrdersBtn.UseVisualStyleBackColor = true;
         this.createOrdersBtn.Click += new System.EventHandler(this.createOrdersBtn_Click);
         // 
         // closebtn
         // 
         this.closebtn.Location = new System.Drawing.Point(602, 237);
         this.closebtn.Name = "closebtn";
         this.closebtn.Size = new System.Drawing.Size(98, 23);
         this.closebtn.TabIndex = 1;
         this.closebtn.Text = "Save and Close";
         this.closebtn.UseVisualStyleBackColor = true;
         this.closebtn.Click += new System.EventHandler(this.closeBtn_Click);
         // 
         // StockbrokersOrderCreationDlg
         // 
         this.AcceptButton = this.createOrdersBtn;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(712, 272);
         this.Controls.Add(this.closebtn);
         this.Controls.Add(this.createOrdersBtn);
         this.Controls.Add(this.ordersTextBox);
         this.Name = "StockbrokersOrderCreationDlg";
         this.Text = "Stock Brokers Order Creation";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox ordersTextBox;
      private System.Windows.Forms.Button createOrdersBtn;
      private System.Windows.Forms.Button closebtn;
   }
}