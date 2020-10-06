using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.CommentDlg
{
    public partial class AddCommentDialog : Form
    {
        public AddCommentDialog(DateTime date)
        {
            InitializeComponent();

            if (date == date.Date)
            {
                dateLbl.Text = date.ToShortDateString();
            }
            else
            {
                dateLbl.Text = date.ToShortDateString() + " " + date.ToShortTimeString();
            }
        }

        public string Comment => this.commentTextBox.Text;
    }
}
