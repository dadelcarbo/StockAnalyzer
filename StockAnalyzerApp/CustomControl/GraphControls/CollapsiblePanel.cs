using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    public partial class CollapsiblePanel : Panel
    {
        public CollapsiblePanel()
        {
            InitializeComponent();
        }

        private bool _isCollapsed;
        private float _sizeRatio = 1.0f;
        private RatioType _ratioType = RatioType.Ratio;

        // Event to notify when the panel is collapsed or expanded
        public event EventHandler OnCollapsed;
        // Event to notify when the size ratio changes
        public event EventHandler OnSizeRatioChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set
            {
                if (_isCollapsed != value)
                {
                    _isCollapsed = value;
                    OnCollapsed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float SizeRatio
        {
            get => _sizeRatio;
            set
            {
                if (_sizeRatio != value)
                {
                    _sizeRatio = value;
                    OnSizeRatioChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RatioType RatioType
        {
            get => _ratioType;
            set
            {
                if (_ratioType != value)
                {
                    _ratioType = value;
                    OnSizeRatioChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        private void collapseBtn_Click(object sender, EventArgs e)
        {
            this.IsCollapsed = true;
        }
    }
}
