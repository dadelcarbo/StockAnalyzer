namespace StockAnalyzerApp.CustomControl
{
    partial class GetLicenseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetLicenseForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.userIdTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.getLicenseButton = new System.Windows.Forms.Button();
            this.msg1 = new System.Windows.Forms.Label();
            this.msg2 = new System.Windows.Forms.Label();
            this.msg5 = new System.Windows.Forms.Label();
            this.msg6 = new System.Windows.Forms.Label();
            this.err1 = new System.Windows.Forms.Label();
            this.err2 = new System.Windows.Forms.Label();
            this.msg3 = new System.Windows.Forms.Label();
            this.msg4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // userIdTextBox
            // 
            resources.ApplyResources(this.userIdTextBox, "userIdTextBox");
            this.userIdTextBox.Name = "userIdTextBox";
            // 
            // passwordTextBox
            // 
            resources.ApplyResources(this.passwordTextBox, "passwordTextBox");
            this.passwordTextBox.Name = "passwordTextBox";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // getLicenseButton
            // 
            resources.ApplyResources(this.getLicenseButton, "getLicenseButton");
            this.getLicenseButton.Name = "getLicenseButton";
            this.getLicenseButton.UseVisualStyleBackColor = true;
            this.getLicenseButton.Click += new System.EventHandler(this.getLicenseButton_Click);
            // 
            // msg1
            // 
            resources.ApplyResources(this.msg1, "msg1");
            this.msg1.Name = "msg1";
            // 
            // msg2
            // 
            resources.ApplyResources(this.msg2, "msg2");
            this.msg2.Name = "msg2";
            // 
            // msg5
            // 
            resources.ApplyResources(this.msg5, "msg5");
            this.msg5.Name = "msg5";
            // 
            // msg6
            // 
            resources.ApplyResources(this.msg6, "msg6");
            this.msg6.Name = "msg6";
            // 
            // err1
            // 
            resources.ApplyResources(this.err1, "err1");
            this.err1.Name = "err1";
            // 
            // err2
            // 
            resources.ApplyResources(this.err2, "err2");
            this.err2.Name = "err2";
            // 
            // msg3
            // 
            resources.ApplyResources(this.msg3, "msg3");
            this.msg3.Name = "msg3";
            // 
            // msg4
            // 
            resources.ApplyResources(this.msg4, "msg4");
            this.msg4.Name = "msg4";
            // 
            // GetLicenseForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.msg4);
            this.Controls.Add(this.msg3);
            this.Controls.Add(this.err2);
            this.Controls.Add(this.err1);
            this.Controls.Add(this.msg6);
            this.Controls.Add(this.msg5);
            this.Controls.Add(this.msg2);
            this.Controls.Add(this.msg1);
            this.Controls.Add(this.getLicenseButton);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.userIdTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetLicenseForm";
            this.Shown += new System.EventHandler(this.GetLicenseForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox userIdTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button getLicenseButton;
        private System.Windows.Forms.Label msg1;
        private System.Windows.Forms.Label msg2;
        private System.Windows.Forms.Label msg5;
        private System.Windows.Forms.Label msg6;
        private System.Windows.Forms.Label err1;
        private System.Windows.Forms.Label err2;
        private System.Windows.Forms.Label msg3;
        private System.Windows.Forms.Label msg4;
    }
}