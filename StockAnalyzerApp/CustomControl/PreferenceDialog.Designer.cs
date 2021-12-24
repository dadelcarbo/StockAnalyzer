using System;

namespace StockAnalyzerApp.CustomControl
{
    partial class PreferenceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferenceDialog));
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.downloadDataCheckBox = new System.Windows.Forms.CheckBox();
            this.generateBreadthCheckBox = new System.Windows.Forms.CheckBox();
            this.enableLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.userIDTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.getLicenseButton = new System.Windows.Forms.Button();
            this.msg1 = new System.Windows.Forms.Label();
            this.chartParamGroupBox = new System.Windows.Forms.GroupBox();
            this.barNumberUpDown = new System.Windows.Forms.NumericUpDown();
            this.showVariationCheckBox = new System.Windows.Forms.CheckBox();
            this.barNumberLabel = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.startYearTextBox = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.testButton = new System.Windows.Forms.Button();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.smtpTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.alertGroupBox = new System.Windows.Forms.GroupBox();
            this.alertFrequencyUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.alertActiveCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.generateDailyReportCheckBox = new System.Windows.Forms.CheckBox();
            this.showBarSmoothingCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rootFolderTextBox = new System.Windows.Forms.TextBox();
            this.browseRootButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.chartParamGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barNumberUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startYearTextBox)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.alertGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.alertFrequencyUpDown)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDlg
            // 
            this.openFileDlg.FileName = "openFileDialog1";
            // 
            // okBtn
            // 
            resources.ApplyResources(this.okBtn, "okBtn");
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Name = "okBtn";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            resources.ApplyResources(this.cancelBtn, "cancelBtn");
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // downloadDataCheckBox
            // 
            resources.ApplyResources(this.downloadDataCheckBox, "downloadDataCheckBox");
            this.downloadDataCheckBox.Name = "downloadDataCheckBox";
            this.downloadDataCheckBox.UseVisualStyleBackColor = true;
            this.downloadDataCheckBox.CheckedChanged += new System.EventHandler(this.downloadDataCheckBox_CheckedChanged);
            // 
            // generateBreadthCheckBox
            // 
            resources.ApplyResources(this.generateBreadthCheckBox, "generateBreadthCheckBox");
            this.generateBreadthCheckBox.Name = "generateBreadthCheckBox";
            this.generateBreadthCheckBox.UseVisualStyleBackColor = true;
            this.generateBreadthCheckBox.CheckedChanged += new System.EventHandler(this.generateBreadthCheckBox_CheckedChanged);
            // 
            // enableLoggingCheckBox
            // 
            resources.ApplyResources(this.enableLoggingCheckBox, "enableLoggingCheckBox");
            this.enableLoggingCheckBox.Name = "enableLoggingCheckBox";
            this.enableLoggingCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // userIDTextBox
            // 
            resources.ApplyResources(this.userIDTextBox, "userIDTextBox");
            this.userIDTextBox.Name = "userIDTextBox";
            this.userIDTextBox.TextChanged += new System.EventHandler(this.userIDTextBox_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.generateBreadthCheckBox);
            this.groupBox1.Controls.Add(this.downloadDataCheckBox);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
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
            // chartParamGroupBox
            // 
            this.chartParamGroupBox.Controls.Add(this.barNumberUpDown);
            this.chartParamGroupBox.Controls.Add(this.showVariationCheckBox);
            this.chartParamGroupBox.Controls.Add(this.barNumberLabel);
            resources.ApplyResources(this.chartParamGroupBox, "chartParamGroupBox");
            this.chartParamGroupBox.Name = "chartParamGroupBox";
            this.chartParamGroupBox.TabStop = false;
            // 
            // barNumberUpDown
            // 
            this.barNumberUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            resources.ApplyResources(this.barNumberUpDown, "barNumberUpDown");
            this.barNumberUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.barNumberUpDown.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.barNumberUpDown.Name = "barNumberUpDown";
            this.barNumberUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // showVariationCheckBox
            // 
            resources.ApplyResources(this.showVariationCheckBox, "showVariationCheckBox");
            this.showVariationCheckBox.Name = "showVariationCheckBox";
            this.showVariationCheckBox.UseVisualStyleBackColor = true;
            // 
            // barNumberLabel
            // 
            resources.ApplyResources(this.barNumberLabel, "barNumberLabel");
            this.barNumberLabel.Name = "barNumberLabel";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.startYearTextBox);
            this.groupBox2.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // startYearTextBox
            // 
            resources.ApplyResources(this.startYearTextBox, "startYearTextBox");
            this.startYearTextBox.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.startYearTextBox.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.startYearTextBox.Name = "startYearTextBox";
            this.startYearTextBox.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.testButton);
            this.groupBox3.Controls.Add(this.addressTextBox);
            this.groupBox3.Controls.Add(this.smtpTextBox);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // testButton
            // 
            resources.ApplyResources(this.testButton, "testButton");
            this.testButton.Name = "testButton";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // addressTextBox
            // 
            resources.ApplyResources(this.addressTextBox, "addressTextBox");
            this.addressTextBox.Name = "addressTextBox";
            // 
            // smtpTextBox
            // 
            resources.ApplyResources(this.smtpTextBox, "smtpTextBox");
            this.smtpTextBox.Name = "smtpTextBox";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // alertGroupBox
            // 
            this.alertGroupBox.Controls.Add(this.alertFrequencyUpDown);
            this.alertGroupBox.Controls.Add(this.label5);
            this.alertGroupBox.Controls.Add(this.alertActiveCheckBox);
            resources.ApplyResources(this.alertGroupBox, "alertGroupBox");
            this.alertGroupBox.Name = "alertGroupBox";
            this.alertGroupBox.TabStop = false;
            // 
            // alertFrequencyUpDown
            // 
            resources.ApplyResources(this.alertFrequencyUpDown, "alertFrequencyUpDown");
            this.alertFrequencyUpDown.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.alertFrequencyUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.alertFrequencyUpDown.Name = "alertFrequencyUpDown";
            this.alertFrequencyUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.alertFrequencyUpDown.ValueChanged += new System.EventHandler(this.alertFrequencyUpDown_ValueChanged);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // alertActiveCheckBox
            // 
            resources.ApplyResources(this.alertActiveCheckBox, "alertActiveCheckBox");
            this.alertActiveCheckBox.Name = "alertActiveCheckBox";
            this.alertActiveCheckBox.UseVisualStyleBackColor = true;
            this.alertActiveCheckBox.CheckedChanged += new System.EventHandler(this.AlertActiveCheckBox_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.generateDailyReportCheckBox);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // generateDailyReportCheckBox
            // 
            resources.ApplyResources(this.generateDailyReportCheckBox, "generateDailyReportCheckBox");
            this.generateDailyReportCheckBox.Name = "generateDailyReportCheckBox";
            this.generateDailyReportCheckBox.UseVisualStyleBackColor = true;
            this.generateDailyReportCheckBox.CheckedChanged += new System.EventHandler(this.generateDailyReportCheckBox_CheckedChanged);
            // 
            // showBarSmoothingCheckBox
            // 
            resources.ApplyResources(this.showBarSmoothingCheckBox, "showBarSmoothingCheckBox");
            this.showBarSmoothingCheckBox.Name = "showBarSmoothingCheckBox";
            this.showBarSmoothingCheckBox.UseVisualStyleBackColor = true;
            this.showBarSmoothingCheckBox.CheckedChanged += new System.EventHandler(this.showBarSmoothingCheckBox_CheckedChanged);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // rootFolderTextBox
            // 
            resources.ApplyResources(this.rootFolderTextBox, "rootFolderTextBox");
            this.rootFolderTextBox.Name = "rootFolderTextBox";
            this.rootFolderTextBox.TextChanged += new System.EventHandler(this.rootFolderTextBox_TextChanged);
            this.rootFolderTextBox.Validating += RootFolderTextBox_Validating;
            // 
            // browseRootButton
            // 
            this.browseRootButton.Image = global::StockAnalyzerApp.Properties.Resources.AddFolder;
            resources.ApplyResources(this.browseRootButton, "browseRootButton");
            this.browseRootButton.Name = "browseRootButton";
            this.browseRootButton.UseVisualStyleBackColor = true;
            this.browseRootButton.Click += new System.EventHandler(this.browseRootButton_Click);
            // 
            // PreferenceDialog
            // 
            this.AcceptButton = this.okBtn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.Controls.Add(this.browseRootButton);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.alertGroupBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.chartParamGroupBox);
            this.Controls.Add(this.msg1);
            this.Controls.Add(this.getLicenseButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.rootFolderTextBox);
            this.Controls.Add(this.userIDTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.showBarSmoothingCheckBox);
            this.Controls.Add(this.enableLoggingCheckBox);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreferenceDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.chartParamGroupBox.ResumeLayout(false);
            this.chartParamGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barNumberUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startYearTextBox)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.alertGroupBox.ResumeLayout(false);
            this.alertGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.alertFrequencyUpDown)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDlg;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.CheckBox downloadDataCheckBox;
        private System.Windows.Forms.CheckBox generateBreadthCheckBox;
        private System.Windows.Forms.CheckBox enableLoggingCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox userIDTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button getLicenseButton;
        private System.Windows.Forms.Label msg1;
        private System.Windows.Forms.GroupBox chartParamGroupBox;
        private System.Windows.Forms.NumericUpDown barNumberUpDown;
        private System.Windows.Forms.CheckBox showVariationCheckBox;
        private System.Windows.Forms.Label barNumberLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown startYearTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox smtpTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox alertGroupBox;
        private System.Windows.Forms.NumericUpDown alertFrequencyUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox alertActiveCheckBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox generateDailyReportCheckBox;
        private System.Windows.Forms.CheckBox showBarSmoothingCheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox rootFolderTextBox;
        private System.Windows.Forms.Button browseRootButton;
    }
}