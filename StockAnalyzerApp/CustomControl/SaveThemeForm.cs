using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl
{
    public partial class SaveThemeForm : Form
    {
        private static readonly string WORK_THEME = "__NewTheme*";
        public string Theme
        {
            get
            {
                if (this.replaceRadioButton.Checked) return this.themeComboBox.SelectedItem.ToString();
                else return this.themeTextBox.Text;
            }
        }

        public SaveThemeForm(List<string> themeList)
        {
            InitializeComponent();
            foreach (string theme in themeList)
            {
                if (theme != WORK_THEME)
                {
                    this.themeComboBox.Items.Add(theme);
                }
            }
            if (this.themeComboBox.Items.Count == 0)
            {
                this.replaceRadioButton.Checked = false;
                this.replaceRadioButton.Enabled = false;
                this.newRadioButton.Checked = true;
            }
            else
            {
                this.themeComboBox.SelectedItem = this.themeComboBox.Items[0];
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            if (replaceRadioButton.Checked || this.themeComboBox.Items.Contains(this.themeTextBox.Text))
            {
                if (MessageBox.Show(this.areYouSureLabel.Text.Replace("$theme", this.themeComboBox.SelectedItem.ToString()), "Attention", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(this.themeTextBox.Text))
                {
                    MessageBox.Show(this.invalidThemeLabel.Text, Localisation.UltimateChartistStrings.DlgTitle_InvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Check input characters
                    if (this.themeTextBox.Text.IndexOfAny(new char[] { '!', '"', '%', '*', '(', ')', '{', '}', '[', ']', '?', '\\', '<', '>' }) != -1)
                    {
                        MessageBox.Show(Localisation.UltimateChartistStrings.Dlg_InvalidCarater, Localisation.UltimateChartistStrings.DlgTitle_InvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        this.Close();
                    }
                }
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void replaceRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (replaceRadioButton.Checked)
            {
                this.themeComboBox.Enabled = true;
                this.themeTextBox.Enabled = false;
            }
        }

        private void newRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (newRadioButton.Checked)
            {
                this.themeComboBox.Enabled = false;
                this.themeTextBox.Enabled = true;
            }
        }
    }
}
