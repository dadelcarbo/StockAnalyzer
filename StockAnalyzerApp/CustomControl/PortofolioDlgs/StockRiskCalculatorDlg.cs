using System.Windows.Forms;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
   public partial class StockRiskCalculatorDlg : Form
   {
      public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

      private RiskCalculatorViewModel viewModel;
      public StockRiskCalculatorDlg()
      {
         InitializeComponent();

         viewModel = new RiskCalculatorViewModel();

         this.riskCalculatorBindingSource.DataSource = viewModel;

         StockAnalyzerForm.MainFrame.StockSerieChanged += MainFrame_StockSerieChanged;
      }

      void MainFrame_StockSerieChanged(StockSerie newSerie, bool ignoreLinkedTheme)
      {
         this.StockSerie = newSerie;
      }

      private StockSerie stockSerie;
      public StockSerie StockSerie
      {
         private get { return stockSerie; }
         set
         {
            stockSerie = value;

            this.viewModel.StockSerie = stockSerie;
         }
      }
   }
}
