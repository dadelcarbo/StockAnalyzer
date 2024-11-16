using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.DrawingDlg
{
    public class ViewModel : NotifyPropertyChangedBase
    {
        public ObservableCollection<DrawingViewModel> Drawings { get; set; }

        public ViewModel()
        {
            this.Drawings = new ObservableCollection<DrawingViewModel>();
            foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.StockAnalysis != null && s.StockAnalysis.DrawingItems.Count > 0))
            {
                foreach (var item in stockSerie.StockAnalysis.DrawingItems.Where(d => d.Value.Count(di => di.IsPersistent) > 0))
                {
                    this.Drawings.Add(new DrawingViewModel(stockSerie.StockName, item.Key));
                }
            }
        }
    }
}
