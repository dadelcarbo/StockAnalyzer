namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
   partial class StockMarketReplay
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
         this.nextButton = new System.Windows.Forms.Button();
         this.startButton = new System.Windows.Forms.Button();
         this.buyButton = new System.Windows.Forms.Button();
         this.sellButton = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.positionTextBox = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.openTextBox = new System.Windows.Forms.TextBox();
         this.label4 = new System.Windows.Forms.Label();
         this.currentValueTextBox = new System.Windows.Forms.TextBox();
         this.addedValueTextBox = new System.Windows.Forms.TextBox();
         this.label5 = new System.Windows.Forms.Label();
         this.totalTextBox = new System.Windows.Forms.TextBox();
         this.shortButton = new System.Windows.Forms.Button();
         this.coverButton = new System.Windows.Forms.Button();
         this.label6 = new System.Windows.Forms.Label();
         this.addedValuePercentTextBox = new System.Windows.Forms.TextBox();
         this.moveButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // nextButton
         // 
         this.nextButton.Enabled = false;
         this.nextButton.Location = new System.Drawing.Point(197, 12);
         this.nextButton.Name = "nextButton";
         this.nextButton.Size = new System.Drawing.Size(75, 23);
         this.nextButton.TabIndex = 0;
         this.nextButton.Text = "&Next";
         this.nextButton.UseVisualStyleBackColor = true;
         this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
         // 
         // startButton
         // 
         this.startButton.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
         this.startButton.Location = new System.Drawing.Point(197, 227);
         this.startButton.Name = "startButton";
         this.startButton.Size = new System.Drawing.Size(75, 23);
         this.startButton.TabIndex = 1;
         this.startButton.Text = "Start";
         this.startButton.UseVisualStyleBackColor = true;
         this.startButton.Click += new System.EventHandler(this.startButton_Click);
         // 
         // buyButton
         // 
         this.buyButton.Location = new System.Drawing.Point(12, 12);
         this.buyButton.Name = "buyButton";
         this.buyButton.Size = new System.Drawing.Size(75, 23);
         this.buyButton.TabIndex = 2;
         this.buyButton.Text = "&Buy";
         this.buyButton.UseVisualStyleBackColor = true;
         this.buyButton.Click += new System.EventHandler(this.buyButton_Click);
         // 
         // sellButton
         // 
         this.sellButton.Location = new System.Drawing.Point(93, 12);
         this.sellButton.Name = "sellButton";
         this.sellButton.Size = new System.Drawing.Size(75, 23);
         this.sellButton.TabIndex = 2;
         this.sellButton.Text = "&Sell";
         this.sellButton.UseVisualStyleBackColor = true;
         this.sellButton.Click += new System.EventHandler(this.sellButton_Click);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 80);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(44, 13);
         this.label1.TabIndex = 3;
         this.label1.Text = "Variation";
         // 
         // positionTextBox
         // 
         this.positionTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Variation", true));
         this.positionTextBox.Location = new System.Drawing.Point(67, 77);
         this.positionTextBox.Name = "positionTextBox";
         this.positionTextBox.Size = new System.Drawing.Size(68, 20);
         this.positionTextBox.TabIndex = 4;
         this.positionTextBox.Text = "0";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(12, 104);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(33, 13);
         this.label2.TabIndex = 5;
         this.label2.Text = "Open";
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(12, 133);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(41, 13);
         this.label3.TabIndex = 5;
         this.label3.Text = "Current";
         // 
         // openTextBox
         // 
         this.openTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "OpenValue", true));
         this.openTextBox.Location = new System.Drawing.Point(67, 104);
         this.openTextBox.Name = "openTextBox";
         this.openTextBox.Size = new System.Drawing.Size(68, 20);
         this.openTextBox.TabIndex = 6;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(12, 159);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(51, 13);
         this.label4.TabIndex = 5;
         this.label4.Text = "+/- Value";
         // 
         // currentValueTextBox
         // 
         this.currentValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "CurrentValue", true));
         this.currentValueTextBox.Location = new System.Drawing.Point(67, 130);
         this.currentValueTextBox.Name = "currentValueTextBox";
         this.currentValueTextBox.Size = new System.Drawing.Size(68, 20);
         this.currentValueTextBox.TabIndex = 7;
         // 
         // addedValueTextBox
         // 
         this.addedValueTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "AddedValue", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
         this.addedValueTextBox.Location = new System.Drawing.Point(81, 156);
         this.addedValueTextBox.Name = "addedValueTextBox";
         this.addedValueTextBox.Size = new System.Drawing.Size(54, 20);
         this.addedValueTextBox.TabIndex = 8;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(12, 211);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(31, 13);
         this.label5.TabIndex = 9;
         this.label5.Text = "Total";
         // 
         // totalTextBox
         // 
         this.totalTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "TotalValue", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
         this.totalTextBox.Location = new System.Drawing.Point(60, 208);
         this.totalTextBox.Name = "totalTextBox";
         this.totalTextBox.Size = new System.Drawing.Size(75, 20);
         this.totalTextBox.TabIndex = 10;
         // 
         // shortButton
         // 
         this.shortButton.Location = new System.Drawing.Point(12, 41);
         this.shortButton.Name = "shortButton";
         this.shortButton.Size = new System.Drawing.Size(75, 23);
         this.shortButton.TabIndex = 2;
         this.shortButton.Text = "S&hort";
         this.shortButton.UseVisualStyleBackColor = true;
         this.shortButton.Click += new System.EventHandler(this.shortButton_Click);
         // 
         // coverButton
         // 
         this.coverButton.Location = new System.Drawing.Point(93, 41);
         this.coverButton.Name = "coverButton";
         this.coverButton.Size = new System.Drawing.Size(75, 23);
         this.coverButton.TabIndex = 2;
         this.coverButton.Text = "&Cover";
         this.coverButton.UseVisualStyleBackColor = true;
         this.coverButton.Click += new System.EventHandler(this.sellButton_Click);
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(12, 185);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(62, 13);
         this.label6.TabIndex = 5;
         this.label6.Text = "+/- Value %";
         // 
         // addedValuePercentTextBox
         // 
         this.addedValuePercentTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "AddedValuePercent", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "P2"));
         this.addedValuePercentTextBox.Location = new System.Drawing.Point(81, 182);
         this.addedValuePercentTextBox.Name = "addedValuePercentTextBox";
         this.addedValuePercentTextBox.Size = new System.Drawing.Size(54, 20);
         this.addedValuePercentTextBox.TabIndex = 8;
         // 
         // moveButton
         // 
         this.moveButton.Enabled = false;
         this.moveButton.Location = new System.Drawing.Point(197, 41);
         this.moveButton.Name = "moveButton";
         this.moveButton.Size = new System.Drawing.Size(75, 23);
         this.moveButton.TabIndex = 0;
         this.moveButton.Text = "&Move 5";
         this.moveButton.UseVisualStyleBackColor = true;
         this.moveButton.Click += new System.EventHandler(this.moveButton_Click);
         // 
         // StockMarketReplay
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(284, 262);
         this.Controls.Add(this.totalTextBox);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.addedValuePercentTextBox);
         this.Controls.Add(this.addedValueTextBox);
         this.Controls.Add(this.currentValueTextBox);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.openTextBox);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.positionTextBox);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.coverButton);
         this.Controls.Add(this.shortButton);
         this.Controls.Add(this.sellButton);
         this.Controls.Add(this.buyButton);
         this.Controls.Add(this.startButton);
         this.Controls.Add(this.moveButton);
         this.Controls.Add(this.nextButton);
         this.Name = "StockMarketReplay";
         this.Text = "MarketReplay";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button nextButton;
      private System.Windows.Forms.Button startButton;
      private System.Windows.Forms.Button buyButton;
      private System.Windows.Forms.Button sellButton;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox positionTextBox;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.TextBox openTextBox;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox currentValueTextBox;
      private System.Windows.Forms.TextBox addedValueTextBox;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox totalTextBox;
      private System.Windows.Forms.Button coverButton;
      private System.Windows.Forms.Button shortButton;
      private System.Windows.Forms.TextBox addedValuePercentTextBox;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.Button moveButton;
   }
}