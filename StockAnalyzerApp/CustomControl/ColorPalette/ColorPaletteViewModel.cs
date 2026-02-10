using StockAnalyzer;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace StockAnalyzerApp.CustomControl.ColorPalette
{
    public class ColorPaletteItemModel : NotifyPropertyChangedBase
    {
        public string Name { get; set; }

        private Color darkColor = Colors.LightGray;
        public Color DarkColor { get => darkColor; set { if (SetProperty(ref darkColor, value)) { OnPropertyChanged("DarkBrush"); } } }

        public Brush DarkBrush => new SolidColorBrush(this.DarkColor);

        private Color lightColor = Colors.DarkGray;
        public Color LightColor { get => lightColor; set { if (SetProperty(ref lightColor, value)) { OnPropertyChanged("LightBrush"); } } }

        public Brush LightBrush => new SolidColorBrush(this.LightColor);
    }

    public class ColorPaletteViewModel : NotifyPropertyChangedBase
    {
        public ColorPaletteViewModel()
        {
            this.Reload();

            this.CurrentItem = Palette.FirstOrDefault();

            BackgroundItem = Palette.FirstOrDefault(item => item.Name == "Graph.Background");
        }

        public ColorPaletteItemModel BackgroundItem { get; set; }

        public ObservableCollection<ColorPaletteItemModel> Palette { get; set; } = new ObservableCollection<ColorPaletteItemModel>();

        ColorPaletteItemModel currentItem;
        public ColorPaletteItemModel CurrentItem { get => currentItem; set => SetProperty(ref currentItem, value); }

        private bool useDark = true;
        public bool UseDark { get => useDark; set => SetProperty(ref useDark, value); }

        #region Save Command
        private CommandBase saveCmd;
        public ICommand SaveCommand => saveCmd ??= new CommandBase(Save);

        private void Save()
        {
            ColorManager.Palette.PaletteItems.Clear();
            foreach (var item in Palette)
            {
                ColorManager.Palette.PaletteItems.Add(item.Name, new PaletteItem() { Dark = ToHex(item.DarkColor), Light = ToHex(item.LightColor) });
            }
            ColorManager.Save();
        }
        #endregion

        #region Reload Command
        private CommandBase reloadCmd;
        public ICommand ReloadCommand => reloadCmd ??= new CommandBase(Reload);

        private void Reload()
        {
            Palette.Clear();
            foreach (var item in ColorManager.Palette.PaletteItems)
            {
                Palette.Add(new ColorPaletteItemModel() { Name = item.Key, DarkColor = FromHex(item.Value.Dark), LightColor = FromHex(item.Value.Light) });
            }
        }
        #endregion

        static public Color FromHex(string hex)
        {
            return Color.FromArgb(Convert.ToByte(hex.Substring(1, 2), 16), Convert.ToByte(hex.Substring(3, 2), 16), Convert.ToByte(hex.Substring(5, 2), 16), Convert.ToByte(hex.Substring(7, 2), 16));
        }

        static public string ToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public ObservableCollection<ColorPreset> OfficeColorPalettes => new ObservableCollection<ColorPreset>()
        {
            ColorPreset.Apex, ColorPreset.Aspect, ColorPreset.Civic, ColorPreset.Concourse, ColorPreset.Equity,
            ColorPreset.Flow, ColorPreset.Foundry, ColorPreset.Median, ColorPreset.Metro, ColorPreset.Module,
            ColorPreset.Office, ColorPreset.Opulent, ColorPreset.Oriel, ColorPreset.Origin, ColorPreset.Paper,
            ColorPreset.Solstice, ColorPreset.Standard, ColorPreset.Technique, ColorPreset.Trek, ColorPreset.Urban, ColorPreset.Verve
        };

        ColorPreset selectedPreset = ColorPreset.Office;
        public ColorPreset SelectedPreset { get => selectedPreset; set => SetProperty(ref selectedPreset, value); }

    }

}
