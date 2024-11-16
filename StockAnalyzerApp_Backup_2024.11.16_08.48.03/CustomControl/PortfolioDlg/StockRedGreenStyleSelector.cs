using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public class StockRedGreenStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var gridCell = container as GridViewCell;
            if (gridCell?.DataColumn?.DataMemberBinding == null)
                return null;

            PropertyInfo property = item.GetType().GetProperty(gridCell?.DataColumn.DataMemberBinding.Path.Path);
            if (property == null) return null;

            var propValue = property.GetValue(item);
            if (propValue == null) return null;

            var value = (float)propValue;
            if (value < 0.0f)
            {
                return RedCellStyle;
            }

            return GreenCellStyle;
        }

        public Style RedCellStyle { get; set; }
        public Style GreenCellStyle { get; set; }
    }
}
