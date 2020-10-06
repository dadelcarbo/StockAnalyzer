namespace StockAnalyzerApp.CustomControl
{
    partial class CommentDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommentDialog));
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.commentControl1 = new CustomControl.CommentDlg.CommentControl();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            resources.ApplyResources(this.elementHost1, "elementHost1");
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Child = this.commentControl1;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // CommentDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.elementHost1);
            this.Name = "CommentDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.TopLevel = true;
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private CustomControl.CommentDlg.CommentControl commentControl1;
        private System.Windows.Forms.Button okButton;
    }
}