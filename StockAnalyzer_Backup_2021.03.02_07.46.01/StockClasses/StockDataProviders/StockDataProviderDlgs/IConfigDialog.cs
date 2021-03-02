using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
   public interface IConfigDialog
   {
      DialogResult ShowDialog(StockDictionary stockDico);
      string DisplayName { get; }
   }
}
