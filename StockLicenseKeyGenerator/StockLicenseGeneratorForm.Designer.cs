namespace StockLicenseKeyGenerator
{
    partial class StockLicenseGeneratorForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.expiryDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.machineIDTextBox = new System.Windows.Forms.TextBox();
            this.getMachineIDBtn = new System.Windows.Forms.Button();
            this.pseudoTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.licenseKeyTextBox = new System.Windows.Forms.TextBox();
            this.generateKeyButton = new System.Windows.Forms.Button();
            this.verifyKeyButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.featureTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.saveKeyButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.majorVersionTextBox = new System.Windows.Forms.TextBox();
            this.minorVersionTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Expiry date";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Machine ID";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Pseudo";
            // 
            // expiryDateTimePicker
            // 
            this.expiryDateTimePicker.Location = new System.Drawing.Point(77, 5);
            this.expiryDateTimePicker.Name = "expiryDateTimePicker";
            this.expiryDateTimePicker.Size = new System.Drawing.Size(125, 20);
            this.expiryDateTimePicker.TabIndex = 0;
            // 
            // machineIDTextBox
            // 
            this.machineIDTextBox.Location = new System.Drawing.Point(77, 38);
            this.machineIDTextBox.Name = "machineIDTextBox";
            this.machineIDTextBox.Size = new System.Drawing.Size(271, 20);
            this.machineIDTextBox.TabIndex = 1;
            // 
            // getMachineIDBtn
            // 
            this.getMachineIDBtn.Location = new System.Drawing.Point(354, 36);
            this.getMachineIDBtn.Name = "getMachineIDBtn";
            this.getMachineIDBtn.Size = new System.Drawing.Size(75, 23);
            this.getMachineIDBtn.TabIndex = 7;
            this.getMachineIDBtn.Text = "GetMachineID";
            this.getMachineIDBtn.UseVisualStyleBackColor = true;
            this.getMachineIDBtn.Click += new System.EventHandler(this.getMachineIDBtn_Click);
            // 
            // pseudoTextBox
            // 
            this.pseudoTextBox.Location = new System.Drawing.Point(78, 70);
            this.pseudoTextBox.Name = "pseudoTextBox";
            this.pseudoTextBox.Size = new System.Drawing.Size(103, 20);
            this.pseudoTextBox.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 244);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "License Key";
            // 
            // licenseKeyTextBox
            // 
            this.licenseKeyTextBox.Location = new System.Drawing.Point(77, 241);
            this.licenseKeyTextBox.Multiline = true;
            this.licenseKeyTextBox.Name = "licenseKeyTextBox";
            this.licenseKeyTextBox.Size = new System.Drawing.Size(271, 100);
            this.licenseKeyTextBox.TabIndex = 6;
            // 
            // generateKeyButton
            // 
            this.generateKeyButton.Location = new System.Drawing.Point(354, 239);
            this.generateKeyButton.Name = "generateKeyButton";
            this.generateKeyButton.Size = new System.Drawing.Size(91, 23);
            this.generateKeyButton.TabIndex = 8;
            this.generateKeyButton.Text = "Generate Key";
            this.generateKeyButton.UseVisualStyleBackColor = true;
            this.generateKeyButton.Click += new System.EventHandler(this.generateKeyButton_Click);
            // 
            // verifyKeyButton
            // 
            this.verifyKeyButton.Location = new System.Drawing.Point(451, 238);
            this.verifyKeyButton.Name = "verifyKeyButton";
            this.verifyKeyButton.Size = new System.Drawing.Size(91, 23);
            this.verifyKeyButton.TabIndex = 9;
            this.verifyKeyButton.Text = "Verify Key";
            this.verifyKeyButton.UseVisualStyleBackColor = true;
            this.verifyKeyButton.Click += new System.EventHandler(this.verifyKeyButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Features";
            // 
            // featureTextBox
            // 
            this.featureTextBox.Location = new System.Drawing.Point(77, 96);
            this.featureTextBox.Multiline = true;
            this.featureTextBox.Name = "featureTextBox";
            this.featureTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.featureTextBox.Size = new System.Drawing.Size(271, 139);
            this.featureTextBox.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(354, 268);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Load Key";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.loadKeyButton_Click);
            // 
            // saveKeyButton
            // 
            this.saveKeyButton.Location = new System.Drawing.Point(452, 268);
            this.saveKeyButton.Name = "saveKeyButton";
            this.saveKeyButton.Size = new System.Drawing.Size(86, 23);
            this.saveKeyButton.TabIndex = 11;
            this.saveKeyButton.Text = "Save Key";
            this.saveKeyButton.UseVisualStyleBackColor = true;
            this.saveKeyButton.Click += new System.EventHandler(this.saveKeyButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(187, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Version";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(262, 73);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(10, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = ".";
            // 
            // majorVersionTextBox
            // 
            this.majorVersionTextBox.Location = new System.Drawing.Point(243, 70);
            this.majorVersionTextBox.Name = "majorVersionTextBox";
            this.majorVersionTextBox.Size = new System.Drawing.Size(13, 20);
            this.majorVersionTextBox.TabIndex = 3;
            this.majorVersionTextBox.Text = "1";
            // 
            // minorVersionTextBox
            // 
            this.minorVersionTextBox.Location = new System.Drawing.Point(278, 70);
            this.minorVersionTextBox.Name = "minorVersionTextBox";
            this.minorVersionTextBox.Size = new System.Drawing.Size(13, 20);
            this.minorVersionTextBox.TabIndex = 4;
            this.minorVersionTextBox.Text = "0";
            // 
            // StockLicenseGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 353);
            this.Controls.Add(this.minorVersionTextBox);
            this.Controls.Add(this.majorVersionTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.saveKeyButton);
            this.Controls.Add(this.featureTextBox);
            this.Controls.Add(this.verifyKeyButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.generateKeyButton);
            this.Controls.Add(this.licenseKeyTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pseudoTextBox);
            this.Controls.Add(this.getMachineIDBtn);
            this.Controls.Add(this.machineIDTextBox);
            this.Controls.Add(this.expiryDateTimePicker);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "StockLicenseGeneratorForm";
            this.Text = "License Key Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker expiryDateTimePicker;
        private System.Windows.Forms.TextBox machineIDTextBox;
        private System.Windows.Forms.Button getMachineIDBtn;
        private System.Windows.Forms.TextBox pseudoTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox licenseKeyTextBox;
        private System.Windows.Forms.Button generateKeyButton;
        private System.Windows.Forms.Button verifyKeyButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox featureTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button saveKeyButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox majorVersionTextBox;
        private System.Windows.Forms.TextBox minorVersionTextBox;
    }
}

