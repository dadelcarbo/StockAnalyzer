using StockAnalyzer.StockClasses;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.AgendaDlg
{
    public partial class StockAgendaForm : Form
    {
        public StockAgendaForm(StockSerie stockSerie)
        {
            InitializeComponent();

            this.Text = "Agenda for " + stockSerie.ShortName + " - " + stockSerie.StockName;
            stockSerie.Agenda.SortDescending();
            this.stockAgendaUserControl1.DataContext = stockSerie.Agenda;
        }
    }
}
