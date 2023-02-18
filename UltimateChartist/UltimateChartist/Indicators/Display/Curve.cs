using System.Windows.Media;
using Telerik.Windows.Controls;

namespace UltimateChartist.Indicators.Display;

public class Curve : ViewModelBase
{
    private string name;
    public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }

    private Brush stroke;
    public Brush Stroke { get => stroke; set { if (stroke != value) { stroke = value; RaisePropertyChanged(); } } }

    private double thickness;
    public double Thickness { get => thickness; set { if (thickness != value) { thickness = value; RaisePropertyChanged(); } } }
}
