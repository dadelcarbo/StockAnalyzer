using System;

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
            this.label5 = new System.Windows.Forms.Label();
            this.totalTextBox = new System.Windows.Forms.TextBox();
            this.shortButton = new System.Windows.Forms.Button();
            this.coverButton = new System.Windows.Forms.Button();
            this.moveButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.stopTextBox = new System.Windows.Forms.TextBox();
            this.halfCheckBox = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.stockPositionUserControl1 = new StockPositionUserControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.createOrdersButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
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
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startButton.Location = new System.Drawing.Point(197, 367);
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
            this.label1.Location = new System.Drawing.Point(150, 186);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Variation";
            // 
            // positionTextBox
            // 
            this.positionTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Variation", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, "0", "P2"));
            this.positionTextBox.Location = new System.Drawing.Point(204, 183);
            this.positionTextBox.Name = "positionTextBox";
            this.positionTextBox.Size = new System.Drawing.Size(68, 20);
            this.positionTextBox.TabIndex = 4;
            this.positionTextBox.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 186);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Total Gain";
            // 
            // totalTextBox
            // 
            this.totalTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "TotalValue", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.totalTextBox.Location = new System.Drawing.Point(76, 183);
            this.totalTextBox.Name = "totalTextBox";
            this.totalTextBox.Size = new System.Drawing.Size(68, 20);
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Stop";
            // 
            // stopTextBox
            // 
            this.stopTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Stop", true));
            this.stopTextBox.Location = new System.Drawing.Point(45, 23);
            this.stopTextBox.Name = "stopTextBox";
            this.stopTextBox.Size = new System.Drawing.Size(68, 20);
            this.stopTextBox.TabIndex = 11;
            // 
            // halfCheckBox
            // 
            this.halfCheckBox.AutoSize = true;
            this.halfCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this, "HalfPosition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.halfCheckBox.Location = new System.Drawing.Point(45, 78);
            this.halfCheckBox.Name = "halfCheckBox";
            this.halfCheckBox.Size = new System.Drawing.Size(85, 17);
            this.halfCheckBox.TabIndex = 12;
            this.halfCheckBox.Text = "Half Position";
            this.halfCheckBox.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Target", true));
            this.textBox1.Location = new System.Drawing.Point(45, 52);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(68, 20);
            this.textBox1.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 55);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Target";
            // 
            // elementHost1
            // 
            this.elementHost1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.elementHost1.Location = new System.Drawing.Point(15, 212);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(257, 149);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.stockPositionUserControl1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.stopTextBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.halfCheckBox);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.createOrdersButton);
            this.groupBox1.Location = new System.Drawing.Point(13, 71);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 106);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Money Management";
            // 
            // createOrdersButton
            // 
            this.createOrdersButton.Enabled = false;
            this.createOrdersButton.Location = new System.Drawing.Point(161, 31);
            this.createOrdersButton.Name = "createOrdersButton";
            this.createOrdersButton.Size = new System.Drawing.Size(75, 23);
            this.createOrdersButton.TabIndex = 0;
            this.createOrdersButton.Text = "Create";
            this.createOrdersButton.UseVisualStyleBackColor = true;
            this.createOrdersButton.Click += new System.EventHandler(this.createOrderButton_Click);
            // 
            // StockMarketReplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 402);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.totalTextBox);
            this.Controls.Add(this.label5);
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
            this.Text = "Market Replay";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void createOrderButton_Click(object sender, EventArgs e)
        {
            if (this.Position.Number != 0)
            {
                if (this.Position.StopOrder == null)
                {
                    if (this.HalfPosition) { }
                }
            }
        }

        #endregion

        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button buyButton;
        private System.Windows.Forms.Button sellButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox positionTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox totalTextBox;
        private System.Windows.Forms.Button coverButton;
        private System.Windows.Forms.Button shortButton;
        private System.Windows.Forms.Button moveButton;
        private System.Windows.Forms.TextBox stopTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox halfCheckBox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private StockPositionUserControl stockPositionUserControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button createOrdersButton;
    }
}