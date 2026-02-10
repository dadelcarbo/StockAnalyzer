using StockAnalyzer.StockClasses.StockDataProviders.Yahoo;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Policy;
using System.Text.Json;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.ColorPalette
{
    public class PaletteItem
    {
        public string Dark { get; set; }
        public string Light { get; set; }
    }

    public class ColorPalette
    {
        public SortedDictionary<string, PaletteItem> PaletteItems { get; set; } = new SortedDictionary<string, PaletteItem>();
    }

    public enum ColorScheme
    {
        Light,
        Dark
    }
    public class ColorManager
    {
        static SortedDictionary<string, Pen> penCache = new SortedDictionary<string, Pen>();

        public static Pen GetPen(string name, ColorScheme scheme, float width = 1.0f, DashStyle dashStyle = DashStyle.Solid)
        {
            if (!palette.PaletteItems.ContainsKey(name))
                throw new ArgumentException($"Color '{name}' is not defined in the palette.");

            string penName = $"{name}_{scheme}_{width}_{dashStyle}";
            if (!penCache.ContainsKey(penName))
            {
                var colorHex = scheme == ColorScheme.Dark ? palette.PaletteItems[name].Dark : palette.PaletteItems[name].Light;
                var pen = new Pen(FromHex(colorHex), width) { DashStyle = dashStyle };
                penCache[penName] = pen;
            }
            return penCache[penName];
        }

        static SortedDictionary<string, Brush> brushCache = new SortedDictionary<string, Brush>();
        public static Brush GetBrush(string name, ColorScheme scheme)
        {
            if (!palette.PaletteItems.ContainsKey(name))
                throw new ArgumentException($"Color '{name}' is not defined in the palette.");
            string brushName = $"{name}_{scheme}";
            if (!brushCache.ContainsKey(brushName))
            {
                var colorHex = scheme == ColorScheme.Dark ? palette.PaletteItems[name].Dark : palette.PaletteItems[name].Light;
                var brush = new SolidBrush(FromHex(colorHex));
                brushCache[brushName] = brush;
            }
            return brushCache[brushName];
        }

        static ColorPalette palette;
        public static ColorPalette Palette
        {
            get
            {
                if (palette == null)
                {
                    Initialize();
                }
                return palette;
            }
        }

        static void Initialize()
        {
            if (!File.Exists(Folders.ColorPalette))
                palette = new ColorPalette();
            else
                palette = JsonSerializer.Deserialize<ColorPalette>(File.ReadAllText(Folders.ColorPalette));
        }

        public static void Save()
        {
            if (palette == null)
                throw new Exception("Palette is not initialized");
            try
            {
                var jsonData = JsonSerializer.Serialize(palette, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Folders.ColorPalette, jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Palette saving error");
            }
        }

        static public Color FromHex(string hex)
        {
            return Color.FromArgb(Convert.ToInt32(hex.Substring(1, 2), 16), Convert.ToInt32(hex.Substring(3, 2), 16), Convert.ToInt32(hex.Substring(5, 2), 16), Convert.ToInt32(hex.Substring(7, 2), 16));
        }

    }
}
