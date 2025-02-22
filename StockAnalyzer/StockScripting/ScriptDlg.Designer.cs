using System.Windows.Navigation;

namespace StockAnalyzerApp.StockScripting
{
    partial class ScriptDlg
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
            this.scriptTextBox = new System.Windows.Forms.TextBox();
            this.CompileBtn = new System.Windows.Forms.Button();
            this.resultTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // scriptTextBox
            // 
            this.scriptTextBox.Location = new System.Drawing.Point(12, 12);
            this.scriptTextBox.Multiline = true;
            this.scriptTextBox.Name = "scriptTextBox";
            this.scriptTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.scriptTextBox.Size = new System.Drawing.Size(669, 293);
            this.scriptTextBox.TabIndex = 0;
            this.scriptTextBox.WordWrap = false;
            this.scriptTextBox.Text = "return true";
            // 
            // CompileBtn
            // 
            this.CompileBtn.Location = new System.Drawing.Point(699, 12);
            this.CompileBtn.Name = "CompileBtn";
            this.CompileBtn.Size = new System.Drawing.Size(75, 23);
            this.CompileBtn.TabIndex = 1;
            this.CompileBtn.Text = "Compile";
            this.CompileBtn.UseVisualStyleBackColor = true;
            this.CompileBtn.Click += new System.EventHandler(this.CompileBtn_Click);
            // 
            // resultTextBox
            // 
            this.resultTextBox.Location = new System.Drawing.Point(12, 311);
            this.resultTextBox.Multiline = true;
            this.resultTextBox.Name = "resultTextBox";
            this.resultTextBox.Size = new System.Drawing.Size(669, 246);
            this.resultTextBox.TabIndex = 0;
            // 
            // ScriptDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 569);
            this.Controls.Add(this.CompileBtn);
            this.Controls.Add(this.resultTextBox);
            this.Controls.Add(this.scriptTextBox);
            this.Name = "ScriptDlg";
            this.Text = "ScriptDlg";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox scriptTextBox;
        private System.Windows.Forms.Button CompileBtn;
        private System.Windows.Forms.TextBox resultTextBox;
    }
}