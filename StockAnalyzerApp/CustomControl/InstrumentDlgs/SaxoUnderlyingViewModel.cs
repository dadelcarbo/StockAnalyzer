using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzer.StockData;
using System.Runtime.CompilerServices;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class SaxoUnderlyingViewModel : NotifyPropertyChangedBase
    {
        long id;
        public long Id => id;

        string saxoName;
        public string SaxoName => saxoName;

        StockInstrument instrument;
        public string InstrumentId
        {
            get { return instrument?.Id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.instrument = null;
                    OnPropertyChanged(nameof(InstrumentId));
                    OnPropertyChanged(nameof(InstrumentName));
                }
                else
                {
                    if (!StockDictionary.Instruments.TryGetValue(value, out this.instrument))
                    {
                        instrument = null;
                    }
                    OnPropertyChanged(nameof(InstrumentId));
                    OnPropertyChanged(nameof(InstrumentName));
                }
            }
        }
        public string InstrumentName => instrument?.Name;
        public SaxoUnderlyingViewModel(SaxoUnderlying saxoUnderlying)
        {
            this.id = saxoUnderlying.Id;
            this.saxoName = saxoUnderlying.SaxoName;

            if (!string.IsNullOrEmpty(saxoUnderlying.InstrumentId))
            {
                StockDictionary.Instruments.TryGetValue(saxoUnderlying.InstrumentId, out this.instrument);
            }
            else
            {
                var instrument = StockDictionary.GetInstrumentByName(saxoUnderlying.SaxoName);
                if (instrument != null)
                {
                    this.instrument = instrument;
                }
            }
        }

        public SaxoUnderlyingViewModel(long id, string saxoName)
        {
            this.id = id;
            this.saxoName = saxoName;

            var instrument = StockDictionary.GetInstrumentByName(saxoName);
            if (instrument != null)
            {
                this.instrument = instrument;
            }
        }
    }
}
