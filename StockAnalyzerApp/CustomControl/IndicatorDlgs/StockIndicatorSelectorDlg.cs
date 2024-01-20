using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.CustomControl.CloudDlgs;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzerApp.Properties;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Resources;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.IndicatorDlgs
{
    public partial class StockIndicatorSelectorDlg : Form
    {
        public event StockAnalyzerForm.OnThemeEditedHandler ThemeEdited;

        private readonly ColorDialog colorDlg;

        readonly List<GroupBox> groupBoxList = new List<GroupBox>();
        private readonly Dictionary<string, List<string>> theme;

        #region STOCKNODE DEFINITION
        public enum NodeType
        {
            Graph,
            Indicator,
            Cloud,
            Curve,
            Event, // For decorators
            Line,
            PaintBars,
            TrailStops,
            Decorator,
            Trail,
            AutoDrawings
        }
        public abstract class StockNode : TreeNode
        {
            public Pen CurvePen { get; set; }
            public bool Visible { get; set; }
            public NodeType Type { get; set; }
            public StockNode(string name, NodeType type, ContextMenuStrip menuStrip)
                : base(name)
            {
                this.Type = type;
                this.ContextMenuStrip = menuStrip;
                this.ImageIndex = -1;
                this.SelectedImageIndex = -1;
            }
            public abstract string ToThemeString();
        }
        public class GraphNode : StockNode
        {
            public Color GraphBackgroundColor { get; set; }
            public Color GraphTextBackgroundColor { get; set; }
            public bool GraphShowGrid { get; set; }
            public Color GraphGridColor { get; set; }
            public GraphChartMode GraphMode { get; set; }
            public Pen SecondaryPen { get; set; }

            public GraphNode(string name, ContextMenuStrip menuStrip,
                Color graphBackgroundColor,
                Color graphTextBackgroundColor,
                bool showGrid,
                Color graphGridColor,
                GraphChartMode mode)
                : base(name, NodeType.Graph, menuStrip)
            {
                this.GraphBackgroundColor = graphBackgroundColor;
                this.GraphTextBackgroundColor = graphTextBackgroundColor;
                this.Type = NodeType.Graph;
                this.GraphShowGrid = showGrid;
                this.GraphGridColor = graphGridColor;
                this.GraphMode = mode;
                this.ImageKey = "CHART";
                this.SelectedImageKey = "CHART";
                this.SecondaryPen = null;
            }

            public override string ToThemeString()
            {
                string themeString = "GRAPH|" +
                    this.GraphBackgroundColor.A.ToString() + ":" + this.GraphBackgroundColor.R.ToString() + ":" + this.GraphBackgroundColor.G.ToString() + ":" + this.GraphBackgroundColor.B.ToString() + "|" +
                    this.GraphTextBackgroundColor.A.ToString() + ":" + this.GraphTextBackgroundColor.R.ToString() + ":" + this.GraphTextBackgroundColor.G.ToString() + ":" + this.GraphTextBackgroundColor.B.ToString() + "|" +
                    this.GraphShowGrid.ToString() + "|" +
                    this.GraphGridColor.A.ToString() + ":" + this.GraphGridColor.R.ToString() + ":" + this.GraphGridColor.G.ToString() + ":" + this.GraphGridColor.B.ToString() + "|" +
                    this.GraphMode.ToString();
                if (this.SecondaryPen != null)
                {
                    themeString += "|" + GraphCurveType.PenToString(this.SecondaryPen);
                }
                return themeString;
            }
        }
        public abstract class ViewableItemNode : StockNode
        {
            public IStockViewableSeries ViewableItem { get; set; }
            public ViewableItemNode(string name, NodeType type, ContextMenuStrip menuStrip, IStockViewableSeries par)
                : base(name, type, menuStrip)
            {
                this.ViewableItem = par;
            }
            public override string ToThemeString()
            {
                return ViewableItem.ToThemeString();
            }
        }
        public class IndicatorNode : ViewableItemNode
        {
            public IndicatorNode(string name, ContextMenuStrip menuStrip, IStockIndicator stockIndicator)
                : base(name, NodeType.Indicator, menuStrip, (IStockViewableSeries)stockIndicator)
            {
                if (name.Contains("SR("))
                {
                    this.ImageKey = "SR";
                    this.SelectedImageKey = "SR";
                }
                else
                {
                    this.ImageIndex = -1;
                    this.SelectedImageIndex = -1;
                }
            }
        }
        public class CloudNode : ViewableItemNode
        {
            public CloudNode(string name, ContextMenuStrip menuStrip, IStockCloud stockCloud)
                : base(name, NodeType.Cloud, menuStrip, (IStockViewableSeries)stockCloud)
            {
                this.ImageKey = "CLOUD";
                this.SelectedImageKey = "CLOUD";
            }
        }
        public class DecoratorNode : ViewableItemNode
        {
            public DecoratorNode(string name, ContextMenuStrip menuStrip, IStockDecorator stockDecorator)
                : base(name, NodeType.Decorator, menuStrip, (IStockViewableSeries)stockDecorator)
            {
                this.ImageKey = "DECO";
                this.SelectedImageKey = "DECO";
            }
        }
        public class TrailNode : ViewableItemNode
        {
            public TrailNode(string name, ContextMenuStrip menuStrip, IStockTrail stockTrail)
                : base(name, NodeType.Trail, menuStrip, (IStockViewableSeries)stockTrail)
            {
                this.ImageKey = "Trail";
                this.SelectedImageKey = "Trail";
            }
        }
        public class TrailStopsNode : ViewableItemNode
        {
            public TrailStopsNode(string name, ContextMenuStrip menuStrip, IStockTrailStop stockTrailStop)
                : base(name, NodeType.TrailStops, menuStrip, (IStockViewableSeries)stockTrailStop)
            {
                this.ImageKey = "Trail";
                this.SelectedImageKey = "Trail";
            }
        }
        public class PaintBarsNode : ViewableItemNode
        {
            public PaintBarsNode(string name, ContextMenuStrip menuStrip, IStockPaintBar stockPaintBar)
                : base(name, NodeType.PaintBars, menuStrip, (IStockViewableSeries)stockPaintBar)
            {
                this.ImageKey = "PB";
                this.SelectedImageKey = "PB";
            }
        }
        public class AutoDrawingsNode : ViewableItemNode
        {
            public AutoDrawingsNode(string name, ContextMenuStrip menuStrip, IStockAutoDrawing stockAutoDrawing)
                : base(name, NodeType.AutoDrawings, menuStrip, (IStockViewableSeries)stockAutoDrawing)
            {
                this.ImageKey = "AD";
                this.SelectedImageKey = "AD";
            }
        }
        public class LineNode : StockNode
        {
            public float LineValue { get; set; }
            public LineNode(string name, ContextMenuStrip menuStrip, Pen linePen, float value)
                : base(name, NodeType.Line, menuStrip)
            {
                this.CurvePen = linePen;
                this.LineValue = value;
                this.LineValue = value;
                this.ImageKey = "LINE";
                this.SelectedImageKey = "LINE";
            }

            public override string ToThemeString()
            {
                return "LINE|" + this.LineValue.ToString() + "|" + GraphCurveType.PenToString(this.CurvePen);
            }
        }
        public class CurveNode : StockNode
        {
            public bool SupportVisibility { get; set; }
            public CurveNode(string name, ContextMenuStrip menuStrip, Pen curvePen, bool isVisible)
                : base(name, NodeType.Curve, menuStrip)
            {
                this.CurvePen = curvePen;
                this.Visible = isVisible;
                this.SupportVisibility = true;
            }
            public CurveNode(string name, ContextMenuStrip menuStrip, Pen curvePen, bool supportVisibility, bool isVisible)
                : base(name, NodeType.Curve, menuStrip)
            {
                this.CurvePen = curvePen;
                this.Visible = true;
                this.SupportVisibility = supportVisibility;
                this.Visible = isVisible;
            }
            public override string ToThemeString()
            {
                return "DATA|" + this.Text + "|" + GraphCurveType.PenToString(this.CurvePen) + "|" + this.Visible.ToString();
            }
        }
        public class EventNode : StockNode
        {
            public bool SupportVisibility { get; set; }
            public EventNode(string name, ContextMenuStrip menuStrip, Pen curvePen, bool isVisible)
                : base(name, NodeType.Event, menuStrip)
            {
                this.CurvePen = curvePen;
                this.Visible = isVisible;
                this.SupportVisibility = true;
                this.ImageKey = "DECO";
                this.SelectedImageKey = "DECO";
            }
            public EventNode(string name, ContextMenuStrip menuStrip, Pen curvePen, bool supportVisibility, bool isVisible)
                : base(name, NodeType.Event, menuStrip)
            {
                this.CurvePen = curvePen;
                this.Visible = true;
                this.SupportVisibility = supportVisibility;
                this.Visible = isVisible;
                this.ImageKey = "DECO";
                this.SelectedImageKey = "DECO";
            }
            public override string ToThemeString()
            {
                return "DATA|" + this.Text + "|" + GraphCurveType.PenToString(this.CurvePen) + "|" + this.Visible.ToString();
            }
        }
        #endregion

        private bool suspendPreview;
        public StockIndicatorSelectorDlg(Dictionary<string, List<string>> theme)
        {
            InitializeComponent();
            colorDlg = new ColorDialog();
            colorDlg.AllowFullOpen = true;
            colorDlg.CustomColors = this.GetCustomColors();

            this.theme = theme;

            this.treeView1.ImageList = new ImageList();
            this.treeView1.ImageList.Images.Add("CURVE", Resources.Curve);
            this.treeView1.ImageList.Images.Add("CHART", Resources.Chart);
            this.treeView1.ImageList.Images.Add("SR", Resources.SR);
            this.treeView1.ImageList.Images.Add("Trail", Resources.trail);
            this.treeView1.ImageList.Images.Add("PB", Resources.PaintBar);
            this.treeView1.ImageList.Images.Add("VH", Resources.VolumeHistogram);
            this.treeView1.ImageList.Images.Add("LINE", Resources.Line);
            this.treeView1.ImageList.Images.Add("DECO", Resources.Decorator);
            this.treeView1.ImageList.Images.Add("CLOUD", Resources.Cloud);
            this.treeView1.ImageList.Images.Add("AD", Resources.AD);

            this.groupBoxList.Add(indicatorConfigBox);
            this.groupBoxList.Add(curveConfigBox);
            this.groupBoxList.Add(lineConfigBox);
            this.groupBoxList.Add(graphConfigBox);
            this.groupBoxList.Add(paintBarGroupBox);
            this.groupBoxList.Add(cloudGroupBox);
            this.groupBoxList.Add(trailStopGroupBox);

            suspendPreview = false;
        }
        void StockIndicatorSelectorDlg_Load(object sender, System.EventArgs e)
        {
            this.suspendPreview = true;
            // Initialise chart mode combo box
            foreach (string s in Enum.GetNames(typeof(GraphChartMode)))
            {
                this.chartModeComboBox.Items.Add(s);
            }
            this.chartModeComboBox.SelectedIndex = 0;

            // Initialise dash type combo box
            foreach (string s in Enum.GetNames(typeof(DashStyle)))
            {
                this.lineTypeComboBox.Items.Add(s);
            }
            this.lineTypeComboBox.SelectedIndex = 0;

            // Initialise thickness combo box
            for (int i = 1; i <= 5; i++)
            {
                this.thicknessComboBox.Items.Add(i);
                this.secondaryThicknessComboBox.Items.Add(i);
            }
            this.thicknessComboBox.SelectedIndex = 0;
            this.secondaryThicknessComboBox.SelectedIndex = 0;

            InitializeTreeView();

            this.suspendPreview = false;
        }
        private void MakeVisible(GroupBox groupBox)
        {
            this.SuspendLayout();
            foreach (GroupBox gb in this.groupBoxList)
            {
                gb.Visible = gb == groupBox;
            }
            this.applyToAllButton.Visible = groupBox == graphConfigBox;
            this.ResumeLayout();
        }
        private int[] GetCustomColors()
        {
            string[] colors = Settings.Default.CustomColors.Split(',');
            int[] res = new int[colors.Length];
            int i = 0;
            foreach (string color in colors)
            {
                res[i++] = int.Parse(color);
            }
            return res;
        }
        private void SaveCustomColors(int[] colors)
        {
            string colorSetting = string.Empty;
            int i;
            for (i = 0; i < colors.Length - 1; i++)
            {
                colorSetting += colors[i].ToString() + ",";
            }
            colorSetting += colors[i].ToString();
            Settings.Default.CustomColors = colorSetting;
            Settings.Default.Save();
        }

        private void InitializeTreeView()
        {
            StockNode treeNode1;

            foreach (string entry in this.theme.Keys)
            {
                if (entry.ToUpper().EndsWith("GRAPH"))
                {
                    GraphNode treeNode = new GraphNode(entry, this.graphMenuStrip, Color.White, Color.LightGray, true, Color.LightGray, GraphChartMode.BarChart);
                    if (entry.ToUpper().Contains("VOLUME"))
                    {
                        treeNode.ImageKey = "VH";
                        treeNode.SelectedImageKey = "VH";
                    }
                    this.treeView1.Nodes.Add(treeNode);

                    foreach (string line in this.theme[entry])
                    {
                        try
                        {
                            string[] fields = line.Split('|');
                            switch (fields[0].ToUpper())
                            {
                                case "GRAPH":
                                    string[] colorItem = fields[1].Split(':');
                                    treeNode.GraphBackgroundColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                    colorItem = fields[2].Split(':');
                                    treeNode.GraphTextBackgroundColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                    treeNode.GraphShowGrid = bool.Parse(fields[3]);
                                    colorItem = fields[4].Split(':');
                                    treeNode.GraphGridColor = Color.FromArgb(int.Parse(colorItem[0]), int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                    treeNode.GraphMode = (GraphChartMode)Enum.Parse(typeof(GraphChartMode), fields[5]);
                                    if (treeNode.Text.ToUpper() == "CLOSEGRAPH")
                                    {
                                        if (fields.Length >= 7)
                                        {
                                            treeNode.SecondaryPen = GraphCurveType.PenFromString(fields[6]);
                                        }
                                        else
                                        {
                                            treeNode.SecondaryPen = new Pen(Color.DarkGoldenrod, 1);
                                        }
                                    }
                                    break;
                                case "DATA":
                                    if (treeNode.Text.ToUpper() == "CLOSEGRAPH")
                                    {
                                        if (fields[1] == "CLOSE")
                                        {
                                            treeNode1 = new CurveNode(fields[1], null, GraphCurveType.PenFromString(fields[2]), bool.Parse(fields[3]));
                                            treeNode.Nodes.Add(treeNode1);
                                        }
                                        else
                                        {
                                            if (bool.Parse(fields[3])) // Normaly Other than close is not visible...
                                            {
                                                treeNode1 = new CurveNode(fields[1], null, GraphCurveType.PenFromString(fields[2]), bool.Parse(fields[3]));
                                                treeNode.Nodes.Add(treeNode1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (fields[1] != "VOLUME" && bool.Parse(fields[3])) // Normaly Other than close is not visible...
                                        {
                                            treeNode1 = new CurveNode(fields[1], null, GraphCurveType.PenFromString(fields[2]), bool.Parse(fields[3]));
                                            treeNode.Nodes.Add(treeNode1);
                                        }
                                    }
                                    break;
                                case "INDICATOR":
                                    {
                                        IStockIndicator stockIndicator = (IStockIndicator)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new IndicatorNode(stockIndicator.Name, this.indicatorMenuStrip, stockIndicator);
                                        for (int i = 0; i < stockIndicator.SeriesCount; i++)
                                        {
                                            CurveNode curveNode = new CurveNode(stockIndicator.SerieNames[i], null, stockIndicator.SeriePens[i], true, stockIndicator.SerieVisibility[i]);
                                            treeNode1.Nodes.Add(curveNode);

                                            curveNode.ImageKey = treeNode1.ImageKey;
                                            curveNode.SelectedImageKey = treeNode1.SelectedImageKey;
                                        }
                                        treeNode.Nodes.Add(treeNode1);
                                    }
                                    break;
                                case "CLOUD":
                                    {
                                        var stockCloud = (IStockCloud)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new CloudNode(stockCloud.Name, this.indicatorMenuStrip, stockCloud);
                                        for (int i = 0; i < stockCloud.SeriesCount; i++)
                                        {
                                            CurveNode curveNode = new CurveNode(stockCloud.SerieNames[i], null, stockCloud.SeriePens[i], true, stockCloud.SerieVisibility[i]);
                                            treeNode1.Nodes.Add(curveNode);

                                            curveNode.ImageKey = treeNode1.ImageKey;
                                            curveNode.SelectedImageKey = treeNode1.SelectedImageKey;
                                        }
                                        treeNode.Nodes.Add(treeNode1);
                                    }
                                    break;
                                case "PAINTBAR":
                                    {
                                        IStockPaintBar stockPaintBar = (IStockPaintBar)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new PaintBarsNode(stockPaintBar.Name, this.indicatorMenuStrip, stockPaintBar);
                                        for (int i = 0; i < stockPaintBar.SeriesCount; i++)
                                        {
                                            CurveNode curveNode = new CurveNode(stockPaintBar.SerieNames[i], null, stockPaintBar.SeriePens[i], true, stockPaintBar.SerieVisibility[i]);
                                            treeNode1.Nodes.Add(curveNode);

                                            curveNode.ImageKey = treeNode1.ImageKey;
                                            curveNode.SelectedImageKey = treeNode1.SelectedImageKey;
                                        }
                                        treeNode.Nodes.Add(treeNode1);
                                    }
                                    break;
                                case "AUTODRAWING":
                                    {
                                        IStockAutoDrawing stockAutoDrawing = (IStockAutoDrawing)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new AutoDrawingsNode(stockAutoDrawing.Name, this.indicatorMenuStrip, stockAutoDrawing);
                                        for (int i = 0; i < stockAutoDrawing.SeriesCount; i++)
                                        {
                                            CurveNode curveNode = new CurveNode(stockAutoDrawing.SerieNames[i], null, stockAutoDrawing.SeriePens[i], true, stockAutoDrawing.SerieVisibility[i]);
                                            treeNode1.Nodes.Add(curveNode);

                                            curveNode.ImageKey = treeNode1.ImageKey;
                                            curveNode.SelectedImageKey = treeNode1.SelectedImageKey;
                                        }
                                        treeNode.Nodes.Add(treeNode1);
                                    }
                                    break;
                                case "TRAILSTOP":
                                    {
                                        IStockTrailStop stockTrailStop = (IStockTrailStop)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new TrailStopsNode(stockTrailStop.Name, this.indicatorMenuStrip, stockTrailStop);
                                        for (int i = 0; i < stockTrailStop.SeriesCount; i++)
                                        {
                                            CurveNode curveNode = new CurveNode(stockTrailStop.SerieNames[i], null, stockTrailStop.SeriePens[i], i >= 2, stockTrailStop.SerieVisibility[i]);
                                            treeNode1.Nodes.Add(curveNode);

                                            curveNode.ImageKey = treeNode1.ImageKey;
                                            curveNode.SelectedImageKey = treeNode1.SelectedImageKey;
                                        }
                                        treeNode.Nodes.Add(treeNode1);
                                    }
                                    break;
                                case "DECORATOR":
                                    {
                                        IStockDecorator stockDecorator = (IStockDecorator)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new DecoratorNode(stockDecorator.Name, this.indicatorMenuStrip, stockDecorator);
                                        for (int i = 0; i < stockDecorator.SeriesCount; i++)
                                        {
                                            treeNode1.Nodes.Add(new CurveNode(stockDecorator.SerieNames[i], null, stockDecorator.SeriePens[i], true, stockDecorator.SerieVisibility[i]));
                                        }
                                        for (int i = 0; i < stockDecorator.EventCount; i++)
                                        {
                                            treeNode1.Nodes.Add(new EventNode(stockDecorator.EventNames[i], null, stockDecorator.EventPens[i], true, stockDecorator.EventVisibility[i]));
                                        }
                                        foreach (TreeNode childNode in treeNode.Nodes)
                                        {
                                            if (childNode.Text == stockDecorator.DecoratedItem)
                                            {
                                                childNode.Nodes.Add(treeNode1);
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                case "TRAIL":
                                    {
                                        IStockTrail stockTrail = (IStockTrail)StockViewableItemsManager.GetViewableItem(line);
                                        treeNode1 = new TrailNode(stockTrail.Name, this.indicatorMenuStrip, stockTrail);
                                        for (int i = 0; i < stockTrail.SeriesCount; i++)
                                        {
                                            treeNode1.Nodes.Add(new CurveNode(stockTrail.SerieNames[i], null, stockTrail.SeriePens[i], true, true));
                                        }
                                        foreach (TreeNode childNode in treeNode.Nodes)
                                        {
                                            if (childNode.Text == stockTrail.TrailedItem)
                                            {
                                                childNode.Nodes.Add(treeNode1);
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                case "LINE":
                                    treeNode.Nodes.Add(new LineNode("LINE_" + fields[1], this.indicatorMenuStrip, GraphCurveType.PenFromString(fields[2]), float.Parse(fields[1])));
                                    break;
                                default:
                                    continue;
                            }
                        }
                        catch (Exception e)
                        {
                            StockLog.Write(e);
                        }
                    }
                }
            }
            foreach (TreeNode node in this.treeView1.Nodes)
            {
                node.Expand();
            }
            this.treeView1.SelectedNode = treeView1.Nodes[0];
        }
        public Dictionary<string, List<string>> GetTheme()
        {
            Dictionary<string, List<string>> dico = new Dictionary<string, List<string>>();
            foreach (TreeNode treeNode in this.treeView1.Nodes)
            {
                List<string> curveList = new List<string>();
                dico.Add(treeNode.Text, curveList);

                // Add Graph config
                GraphNode graphNode = (GraphNode)treeNode;
                curveList.Add(graphNode.ToThemeString());

                AppendNodeToThemeString(treeNode, curveList);
            }
            return dico;
        }
        private static void AppendNodeToThemeString(TreeNode treeNode, List<string> curveList)
        {
            if (treeNode.Nodes == null) return;
            foreach (TreeNode node in treeNode.Nodes)
            {
                StockNode stockNode = (StockNode)node;
                switch (stockNode.Type)
                {
                    case NodeType.Cloud:
                    case NodeType.PaintBars:
                    case NodeType.AutoDrawings:
                    case NodeType.TrailStops:
                    case NodeType.Line:
                    case NodeType.Curve:
                        curveList.Add(stockNode.ToThemeString());
                        break;
                    case NodeType.Decorator:
                    case NodeType.Trail:
                        break;
                    case NodeType.Indicator:
                        curveList.Add(stockNode.ToThemeString());
                        foreach (StockNode childNode in stockNode.Nodes)
                        {
                            if (childNode.Type == NodeType.Decorator)
                            {
                                IStockDecorator stockDecorator = (IStockDecorator)((ViewableItemNode)childNode).ViewableItem;
                                stockDecorator.DecoratedItem = stockNode.Text;
                                curveList.Add(childNode.ToThemeString());
                            }
                            if (childNode.Type == NodeType.Trail)
                            {
                                IStockTrail stockTrail = (IStockTrail)((ViewableItemNode)childNode).ViewableItem;
                                stockTrail.TrailedItem = stockNode.Text;
                                curveList.Add(childNode.ToThemeString());
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("Invalid StockNode Type: " + stockNode.Type);
                }
            }
        }

        void removeStripMenuItem_Click(object sender, System.EventArgs e)
        {
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            this.treeView1.Nodes.Remove(stockNode);
        }
        void copyStripMenuItem_Click(object sender, System.EventArgs e)
        {
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            Clipboard.SetText(stockNode.Text);
        }
        void addDecoratorToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            AddDecoratorDlg addDlg;
            StockNode treeNode = (StockNode)this.treeView1.SelectedNode;
            switch (treeNode.Text)
            {
                case "CloseGraph":
                    return;
                case "VolumeGraph":
                    return;
                default:
                    addDlg = new AddDecoratorDlg();
                    break;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
                if (stockNode.Type == NodeType.Decorator) return;

                IStockDecorator stockDecorator = StockDecoratorManager.CreateDecorator(addDlg.DecoratorName, stockNode.Name);
                if (stockDecorator == null)
                {
                    return;
                }
                if (stockDecorator.DisplayTarget != IndicatorDisplayTarget.PriceIndicator)
                {
                    // Only one decorator per graph (except data serie) (multiple ema, BB, ....)
                    int index = 0;
                    bool found = false;
                    foreach (StockNode node in stockNode.Nodes)
                    {
                        if (node.Type == NodeType.Decorator)
                        {
                            found = true;
                            break;
                        }
                        index++;

                    }
                    if (found)
                    {
                        stockNode.Nodes.RemoveAt(index);
                    }
                }
                StockNode decoratorNode = new DecoratorNode(stockDecorator.Name, this.indicatorMenuStrip, stockDecorator);
                stockNode.Nodes.Add(decoratorNode);
                int i = 0;
                foreach (string curveName in stockDecorator.SerieNames)
                {
                    decoratorNode.Nodes.Add(new CurveNode(curveName, null, stockDecorator.SeriePens[i], true));
                    i++;
                }
                i = 0;
                foreach (string eventName in stockDecorator.EventNames)
                {
                    decoratorNode.Nodes.Add(new EventNode(eventName, null, stockDecorator.EventPens[i], true));
                    i++;
                }
                this.treeView1.SelectedNode = decoratorNode;
                decoratorNode.Expand();
            }
        }
        void addTrailToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            AddTrailDlg addDlg;
            StockNode treeNode = (StockNode)this.treeView1.SelectedNode;
            switch (treeNode.Text)
            {
                case "CloseGraph":
                    return;
                case "VolumeGraph":
                    return;
                default:
                    addDlg = new AddTrailDlg();
                    break;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
                if (stockNode.Type == NodeType.Trail) return;

                IStockTrail stockTrail = StockTrailManager.CreateTrail(addDlg.TrailName, stockNode.Name);
                if (stockTrail == null)
                {
                    return;
                }
                if (stockTrail.DisplayTarget != IndicatorDisplayTarget.PriceIndicator)
                {
                    // Only one trail per graph (except data serie) (multiple ema, BB, ....)
                    int index = 0;
                    bool found = false;
                    foreach (StockNode node in stockNode.Nodes)
                    {
                        if (node.Type == NodeType.Trail)
                        {
                            found = true;
                            break;
                        }
                        index++;

                    }
                    if (found)
                    {
                        stockNode.Nodes.RemoveAt(index);
                    }
                }
                StockNode trailNode = new TrailNode(stockTrail.Name, this.indicatorMenuStrip, stockTrail);
                stockNode.Nodes.Add(trailNode);
                int i = 0;
                foreach (string curveName in stockTrail.SerieNames)
                {
                    trailNode.Nodes.Add(new CurveNode(curveName, null, stockTrail.SeriePens[i], true));
                    i++;
                }
                this.treeView1.SelectedNode = trailNode;
                trailNode.Expand();
            }
        }
        void addIndicatorToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            AddIndicatorDlg addDlg;
            StockNode treeNode = (StockNode)this.treeView1.SelectedNode;
            switch (treeNode.Text)
            {
                case "CloseGraph":
                    addDlg = new AddIndicatorDlg(true);
                    break;
                case "VolumeGraph":
                    return;
                default:
                    addDlg = new AddIndicatorDlg(false);
                    break;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
                IStockIndicator stockIndicator = StockIndicatorManager.CreateIndicator(addDlg.IndicatorName);
                if (stockIndicator == null)
                {
                    return;
                }
                if (stockIndicator.DisplayTarget != IndicatorDisplayTarget.PriceIndicator)
                {
                    // Only one indicator1Name per graph (except data serie) (multiple ema, BB, ....)
                    List<StockNode> nodeList = new List<StockNode>();
                    foreach (StockNode node in stockNode.Nodes)
                    {
                        if (node.Type == NodeType.Indicator || node.Type == NodeType.Line)
                        {
                            nodeList.Add(node);
                        }
                    }
                    foreach (StockNode node in nodeList)
                    {
                        stockNode.Nodes.Remove(node);
                    }
                }
                StockNode indicatorNode = new IndicatorNode(stockIndicator.Name, this.indicatorMenuStrip, stockIndicator);
                stockNode.Nodes.Add(indicatorNode);
                int i = 0;
                foreach (string curveName in stockIndicator.SerieNames)
                {
                    indicatorNode.Nodes.Add(new CurveNode(curveName, null, stockIndicator.SeriePens[i], true));
                    i++;
                }
                indicatorNode.Expand();
                if (stockIndicator.HorizontalLines != null)
                {
                    foreach (HLine line in stockIndicator.HorizontalLines)
                    {
                        string lineName = "LINE_" + line.Level;
                        bool alreadyExist = false;
                        foreach (TreeNode node in stockNode.Nodes)
                        {
                            if (node.Text == lineName)
                            {
                                alreadyExist = true;
                                break;
                            }
                        }
                        if (!alreadyExist)
                        {
                            stockNode.Nodes.Add(new LineNode(lineName, this.indicatorMenuStrip, line.LinePen, line.Level));
                        }
                    }
                }
                this.treeView1.SelectedNode = indicatorNode;
            }
        }
        void addCloudToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            AddCloudDlg addDlg;
            StockNode treeNode = (StockNode)this.treeView1.SelectedNode;
            switch (treeNode.Text)
            {
                case "CloseGraph":
                    addDlg = new AddCloudDlg();
                    break;
                case "VolumeGraph":
                default:
                    return;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
                var stockCloud = StockCloudManager.CreateCloud(addDlg.CloudName);
                if (stockCloud == null)
                {
                    return;
                }

                // Only one cloud1Name per graph (except data serie) (multiple ema, BB, ....)
                List<StockNode> nodeList = new List<StockNode>();
                foreach (StockNode node in stockNode.Nodes)
                {
                    if (node.Type == NodeType.Cloud || node.Type == NodeType.Line)
                    {
                        nodeList.Add(node);
                    }
                }
                foreach (StockNode node in nodeList)
                {
                    stockNode.Nodes.Remove(node);
                }
                StockNode cloudNode = new CloudNode(stockCloud.Name, this.indicatorMenuStrip, stockCloud);
                stockNode.Nodes.Add(cloudNode);
                int i = 0;
                foreach (string curveName in stockCloud.SerieNames)
                {
                    cloudNode.Nodes.Add(new CurveNode(curveName, null, stockCloud.SeriePens[i], true));
                    i++;
                }
                cloudNode.Expand();
                this.treeView1.SelectedNode = cloudNode;
            }
        }
        void addHorizontalLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LineNode stockNode = new LineNode("LINE_0", this.indicatorMenuStrip, new Pen(Color.Black), 0);
            this.treeView1.SelectedNode.Nodes.Add(stockNode);
            this.treeView1.SelectedNode = stockNode;
        }
        #region Curve Config Methods
        private void ActivateCurveConfigPanel(CurveNode curveNode)
        {
            this.MakeVisible(curveConfigBox);
            this.suspendPreview = true;

            StockNode parentNode = (StockNode)this.treeView1.SelectedNode.Parent;
            this.lineTypeComboBox.Enabled = !(
                (parentNode.Type == NodeType.PaintBars) ||
                (parentNode.Type == NodeType.AutoDrawings) ||
                (parentNode.Type == NodeType.Trail) ||
                (parentNode.Type == NodeType.Indicator && ((IndicatorNode)parentNode).ViewableItem.DisplayStyle == IndicatorDisplayStyle.SupportResistance));
            this.lineTypeComboBox.Parent = curveConfigBox;
            this.thicknessComboBox.Parent = curveConfigBox;
            this.lineColorPanel.Parent = curveConfigBox;

            if (curveNode.SupportVisibility)
            {
                this.visibleCheckBox.Parent = curveConfigBox;
                this.visibleCheckBox.Visible = true;
                this.visibleCheckBox.Checked = curveNode.Visible;
            }
            else
            {
                this.visibleCheckBox.Visible = false;
            }

            this.lineTypeComboBox.SelectedItem = curveNode.CurvePen.DashStyle.ToString();
            this.thicknessComboBox.SelectedItem = (int)curveNode.CurvePen.Width;
            this.lineColorPanel.BackColor = curveNode.CurvePen.Color;

            this.curvePreviewLabel.Parent = this.curveConfigBox;
            this.previewPanel.Parent = this.curveConfigBox;

            this.suspendPreview = false;
            this.previewPanel.Refresh();
        }
        private void ActivateCurveConfigPanel(EventNode eventNode)
        {
            this.MakeVisible(curveConfigBox);
            this.suspendPreview = true;

            this.lineTypeComboBox.Enabled = false;
            this.lineTypeComboBox.Parent = curveConfigBox;
            this.thicknessComboBox.Parent = curveConfigBox;
            this.lineColorPanel.Parent = curveConfigBox;

            if (eventNode.SupportVisibility)
            {
                this.visibleCheckBox.Parent = curveConfigBox;
                this.visibleCheckBox.Visible = true;
                this.visibleCheckBox.Checked = eventNode.Visible;
            }
            else
            {
                this.visibleCheckBox.Visible = false;
            }

            this.lineTypeComboBox.SelectedItem = eventNode.CurvePen.DashStyle.ToString();
            this.thicknessComboBox.SelectedItem = (int)eventNode.CurvePen.Width;
            this.lineColorPanel.BackColor = eventNode.CurvePen.Color;

            this.curvePreviewLabel.Parent = this.curveConfigBox;
            this.previewPanel.Parent = this.curveConfigBox;

            this.suspendPreview = false;
            this.previewPanel.Refresh();
        }
        private void previewPanel_Paint(object sender, PaintEventArgs e)
        {
            if (this.suspendPreview) return;
            // Find graph background color
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode.Parent;
            StockNode parentNode = stockNode;
            GraphNode graphNode = null;
            while (graphNode == null)
            {
                if (stockNode.Type == NodeType.Graph)
                {
                    graphNode = (GraphNode)stockNode;
                }
                else
                {
                    stockNode = (StockNode)stockNode.Parent;
                }
            }
            Graphics g = previewPanel.CreateGraphics();
            g.Clear(graphNode.GraphBackgroundColor);
            g.DrawRectangle(Pens.Black, 0, 0, g.VisibleClipBounds.Width - 1, g.VisibleClipBounds.Height - 1);

            StockNode curveNode = (StockNode)this.treeView1.SelectedNode;


            if (parentNode.Type == NodeType.PaintBars || parentNode.Type == NodeType.AutoDrawings || (parentNode.Type == NodeType.Graph && parentNode.Text.ToUpper() == "CLOSEGRAPH" && ((GraphNode)parentNode).GraphMode == GraphChartMode.BarChart))
            {
                PaintPreviewWithPaintBars(g, curveNode.CurvePen);
            }
            else if (parentNode.Type == NodeType.Cloud || (parentNode.Type == NodeType.Graph && parentNode.Text.ToUpper() == "CLOSEGRAPH" && ((GraphNode)parentNode).GraphMode == GraphChartMode.BarChart))
            {
                PaintPreviewWithCloud(g, curveNode.CurvePen, parentNode);
            }
            else
            {
                if (parentNode.Type == NodeType.Graph && parentNode.Text.ToUpper() == "CLOSEGRAPH" && ((GraphNode)parentNode).GraphMode == GraphChartMode.CandleStick)
                {
                    PaintPreviewWithCandleSticks(g, curveNode.CurvePen);
                }
                else
                {

                    double x, y;
                    PointF[] points = new PointF[(int)Math.Floor(g.VisibleClipBounds.Width)];
                    int i = 0;
                    for (x = g.VisibleClipBounds.Left; x < g.VisibleClipBounds.Right; x++)
                    {
                        y = (Math.Sin(x * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                        points[i].X = (float)x;
                        points[i++].Y = (float)(y - (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / 2.0f);
                    }
                    if (parentNode.Type == NodeType.Decorator && curveNode.Type == NodeType.Event)
                    {
                        using (Brush brush = new SolidBrush(curveNode.CurvePen.Color))
                        {
                            g.DrawLines(Pens.Black, points);
                            for (i = 5; i < points.Length - 5; i++)
                            {
                                if ((points[i - 1].Y > points[i].Y && points[i].Y <= points[i + 1].Y) || (points[i - 1].Y < points[i].Y && points[i].Y >= points[i + 1].Y))
                                {
                                    g.FillEllipse(brush, points[i].X - curveNode.CurvePen.Width * 1.5f, points[i].Y - curveNode.CurvePen.Width * 1.5f, curveNode.CurvePen.Width * 3f, curveNode.CurvePen.Width * 3f);

                                }
                            }
                        }
                    }
                    else if (parentNode.Type == NodeType.Indicator && ((IndicatorNode)parentNode).ViewableItem.DisplayStyle == IndicatorDisplayStyle.SupportResistance)
                    {   // Show support/resistance
                        using (Brush brush = new SolidBrush(curveNode.CurvePen.Color))
                        {
                            g.DrawLines(Pens.Black, points);
                            float pointSize = curveNode.CurvePen.Width;
                            for (i = 5; i < points.Length - 5; i++)
                            {
                                if ((points[i - 1].Y > points[i].Y && points[i].Y <= points[i + 1].Y) || (points[i - 1].Y < points[i].Y && points[i].Y >= points[i + 1].Y))
                                {
                                    for (int j = 0; j < 6; j++)
                                    {
                                        g.FillEllipse(brush, points[i].X - pointSize + j * 3 * pointSize, points[i].Y - pointSize, 2 * pointSize, 2 * pointSize);
                                    }
                                    i += 6;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (curveNode.CurvePen.DashStyle == DashStyle.Custom)
                        {
                            float center = (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / -2.0f;
                            for (x = g.VisibleClipBounds.Left + 3; x < g.VisibleClipBounds.Right; x += 8)
                            {
                                y = (Math.Sin(x * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                                if (y < 0)
                                {
                                    g.FillRectangle(Brushes.Green, (float)x, (float)y + center, 6.0f, (float)-y);
                                }
                                else
                                {
                                    g.FillRectangle(Brushes.Red, (float)x, center, 6.0f, (float)y);
                                }
                            }
                        }
                        else
                        {
                            g.DrawLines(curveNode.CurvePen, points);
                        }
                    }
                }
            }
        }
        private static void PaintPreviewWithPaintBars(Graphics g, Pen pen)
        {
            OHLCBar bar;
            double x, y;
            PointF[] points = new PointF[(int)Math.Floor(g.VisibleClipBounds.Width)];
            int i = 0;
            double min = double.MaxValue;
            double max = double.MinValue;
            double open = double.NaN;

            for (x = g.VisibleClipBounds.Left; x < g.VisibleClipBounds.Right; x++)
            {

                y = (Math.Sin(x * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                points[i].X = (float)x;
                points[i].Y = (float)(y - (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / 2.0f);
                if (double.IsNaN(open))
                {
                    open = points[i].Y;
                }
                min = Math.Min(min, points[i].Y);
                max = Math.Max(max, points[i].Y);

                if (i % 8 == 0)
                {
                    bar = new OHLCBar((float)points[i].X, (float)open, (float)(min - (max - min) / 3), (float)(max + (max - min) / 3), (float)points[i].Y);
                    bar.Width = 3f;
                    bar.Draw(g, pen);
                    open = points[i].Y; min = double.MaxValue;
                    max = double.MinValue;
                }
                i++;
            }
        }
        private static void PaintPreviewWithCloud(Graphics g, Pen pen, StockNode parentNode)
        {
            var tag = parentNode.Nodes[0] as StockNode;
            var bullColor = Color.FromArgb(127, tag.CurvePen.Color.R, tag.CurvePen.Color.G, tag.CurvePen.Color.B);
            var bullBrush = new SolidBrush(bullColor);
            var bullPen = tag.CurvePen;

            tag = parentNode.Nodes[1] as StockNode;
            var bearColor = Color.FromArgb(127, tag.CurvePen.Color.R, tag.CurvePen.Color.G, tag.CurvePen.Color.B);
            var bearBrush = new SolidBrush(bearColor);
            var bearPen = tag.CurvePen;

            double x, y;
            PointF[] bullPoints = new PointF[(int)Math.Floor(g.VisibleClipBounds.Width)];
            PointF[] bearPoints = new PointF[(int)Math.Floor(g.VisibleClipBounds.Width)];
            int i = 0;
            for (x = g.VisibleClipBounds.Left; x < g.VisibleClipBounds.Right; x++)
            {
                y = (Math.Sin(x * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                bullPoints[i].X = (float)x;
                bullPoints[i].Y = (float)(y - (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / 2.0f);
                y = (Math.Sin((x * 1.12 + 25) * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                bearPoints[i].X = (float)x;
                bearPoints[i++].Y = (float)(y - (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / 2.0f);
            }

            bool isBull = bullPoints[0].Y >= bearPoints[0].Y;
            var nbPoints = bullPoints.Length;
            var upPoints = new List<PointF>() { bullPoints[0] };
            var downPoints = new List<PointF>() { bearPoints[0] };
            for (i = 1; i < nbPoints; i++)
            {
                if (isBull && bullPoints[i].Y >= bearPoints[i].Y) // Bull cloud continuing
                {
                    upPoints.Add(bullPoints[i]);
                    downPoints.Insert(0, bearPoints[i]);
                }
                else if (!isBull && bullPoints[i].Y < bearPoints[i].Y) // Bear cloud continuing
                {
                    upPoints.Add(bullPoints[i]);
                    downPoints.Insert(0, bearPoints[i]);
                }
                else // Cloud reversing, need a draw
                {
                    if (upPoints.Count > 0)
                    {
                        upPoints.Add(bullPoints[i]);
                        downPoints.Insert(0, bearPoints[i]);
                        g.DrawLines(isBull ? bullPen : bearPen, upPoints.ToArray());
                        g.DrawLines(isBull ? bullPen : bearPen, downPoints.ToArray());
                        upPoints.AddRange(downPoints);
                        g.FillPolygon(isBull ? bullBrush : bearBrush, upPoints.ToArray());
                    }
                    isBull = !isBull;
                    upPoints.Clear();
                    downPoints.Clear();
                    upPoints = new List<PointF>() { bullPoints[i] };
                    downPoints = new List<PointF>() { bearPoints[i] };
                }
            }
            if (upPoints.Count > 0)
            {
                g.DrawLines(isBull ? bullPen : bearPen, upPoints.ToArray());
                g.DrawLines(isBull ? bullPen : bearPen, downPoints.ToArray());
                upPoints.AddRange(downPoints);
                g.FillPolygon(isBull ? bullBrush : bearBrush, upPoints.ToArray());
                upPoints.Clear();
                downPoints.Clear();
            }
        }
        private static void PaintPreviewWithCandleSticks(Graphics g, Pen pen)
        {
            CandleStick bar;
            double x, y;
            PointF[] points = new PointF[(int)Math.Floor(g.VisibleClipBounds.Width)];
            int i = 0;
            double min = double.MaxValue;
            double max = double.MinValue;
            double open = double.NaN;

            for (x = g.VisibleClipBounds.Left; x < g.VisibleClipBounds.Right; x++)
            {

                y = (Math.Sin(x * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                points[i].X = (float)x;
                points[i].Y = (float)(y - (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / 2.0f);
                if (double.IsNaN(open))
                {
                    open = points[i].Y;
                }
                min = Math.Min(min, points[i].Y);
                max = Math.Max(max, points[i].Y);

                if (i % 8 == 0)
                {
                    bar = new CandleStick((float)points[i].X, (float)open, (float)(min - (max - min) / 3), (float)(max + (max - min) / 3), (float)points[i].Y);
                    bar.Width = 3;
                    bar.Draw(g, pen, null);
                    open = points[i].Y; min = double.MaxValue;
                    max = double.MinValue;
                }
                i++;
            }
        }

        private void visibleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            stockNode.Visible = this.visibleCheckBox.Checked;
            switch (((StockNode)stockNode.Parent).Type)
            {
                case NodeType.PaintBars:
                case NodeType.AutoDrawings:
                case NodeType.Indicator:
                case NodeType.Trail:
                case NodeType.TrailStops:
                case NodeType.Cloud:
                    {
                        ViewableItemNode viewableItemNode = (ViewableItemNode)stockNode.Parent;
                        IStockVisibility viewableItem = (IStockVisibility)viewableItemNode.ViewableItem;
                        viewableItem.SerieVisibility[stockNode.Index] = stockNode.Visible;
                    }
                    break;
                case NodeType.Decorator:
                    if (stockNode.Type == NodeType.Event)
                    {
                        ViewableItemNode viewableItemNode = (ViewableItemNode)stockNode.Parent;
                        IStockDecorator viewableItem = (IStockDecorator)viewableItemNode.ViewableItem;

                        int curveCount = 0;
                        foreach (TreeNode node in stockNode.Parent.Nodes)
                        {
                            if (node is CurveNode) curveCount++;
                        }
                        int index = stockNode.Index - curveCount;

                        viewableItem.EventVisibility[index] = stockNode.Visible;
                    }
                    else
                    {
                        ViewableItemNode viewableItemNode = (ViewableItemNode)stockNode.Parent;
                        IStockVisibility viewableItem = (IStockVisibility)viewableItemNode.ViewableItem;
                        viewableItem.SerieVisibility[stockNode.Index] = stockNode.Visible;
                    }
                    break;
            }
            this.previewPanel.Refresh();
        }
        private void lineTypeComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), ((ComboBox)sender).Items[e.Index].ToString());
                    if (pen.DashStyle == DashStyle.Custom)
                    {
                        int barWidth = rect.Width / 10 - 2;
                        for (int x = rect.X + barWidth; x < rect.Width - barWidth; x += rect.Width / 10)
                        {
                            g.FillRectangle(Brushes.Black, x, rect.Y + 1, barWidth, rect.Height - 1);
                        }
                    }
                    else
                    {
                        g.DrawLine(pen, rect.X + 10, rect.Y + rect.Height / 2, rect.Width - 10, rect.Y + rect.Height / 2);
                    }
                }
            }
        }
        private void thicknessComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                int n = (int)((ComboBox)sender).Items[e.Index];
                using (Pen pen = new Pen(Color.Black, n))
                {
                    g.DrawLine(pen, rect.X + 10, rect.Y + rect.Height / 2,
                                    rect.Width - 10, rect.Y + rect.Height / 2);
                }
            }
        }
        private void lineTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            if (stockNode != null && stockNode.CurvePen != null)
            {
                stockNode.CurvePen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), this.lineTypeComboBox.SelectedItem.ToString());
                switch (((StockNode)stockNode.Parent).Type)
                {
                    case NodeType.PaintBars:
                    case NodeType.AutoDrawings:
                    case NodeType.Indicator:
                    case NodeType.Cloud:
                    case NodeType.Decorator:
                    case NodeType.Trail:
                    case NodeType.TrailStops:
                        if (stockNode.Type != NodeType.Event)
                        {
                            ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                            vn.ViewableItem.SeriePens[stockNode.Index].DashStyle = stockNode.CurvePen.DashStyle;
                        }
                        break;
                }
                this.previewPanel.Refresh();
            }
        }
        private void thicknessComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            if (stockNode != null && stockNode.CurvePen != null)
            {
                stockNode.CurvePen.Width = (int)this.thicknessComboBox.SelectedItem;
                switch (((StockNode)stockNode.Parent).Type)
                {
                    case NodeType.PaintBars:
                    case NodeType.AutoDrawings:
                    case NodeType.Indicator:
                    case NodeType.Cloud:
                    case NodeType.Trail:
                    case NodeType.TrailStops:
                        {
                            ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                            vn.ViewableItem.SeriePens[stockNode.Index].Width = stockNode.CurvePen.Width;
                            break;
                        }
                    case NodeType.Decorator:
                        if (stockNode.Type == NodeType.Event)
                        {
                            ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                            IStockDecorator decorator = (IStockDecorator)vn.ViewableItem;
                            int curveCount = 0;
                            foreach (TreeNode node in stockNode.Parent.Nodes)
                            {
                                if (node is CurveNode) curveCount++;
                            }
                            int index = stockNode.Index - curveCount;
                            decorator.EventPens[index].Width = stockNode.CurvePen.Width;
                        }
                        else
                        {
                            ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                            vn.ViewableItem.SeriePens[stockNode.Index].Width = stockNode.CurvePen.Width;
                        }
                        break;
                }
                this.previewPanel.Refresh();
            }
        }
        private void lineColorPanel_Click(object sender, EventArgs e)
        {
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            if (stockNode != null && stockNode.CurvePen != null)
            {
                colorDlg.Color = stockNode.CurvePen.Color;
                if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    stockNode.CurvePen.Color = colorDlg.Color;
                    switch (((StockNode)stockNode.Parent).Type)
                    {
                        case NodeType.PaintBars:
                        case NodeType.AutoDrawings:
                        case NodeType.Indicator:
                        case NodeType.Cloud:
                        case NodeType.Trail:
                        case NodeType.TrailStops:
                            {
                                ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                                vn.ViewableItem.SeriePens[stockNode.Index].Color = stockNode.CurvePen.Color;
                            }
                            break;
                        case NodeType.Decorator:
                            if (stockNode.Type == NodeType.Event)
                            {
                                ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                                IStockDecorator decorator = (IStockDecorator)vn.ViewableItem;
                                int curveCount = 0;
                                foreach (TreeNode node in stockNode.Parent.Nodes)
                                {
                                    if (node is CurveNode) curveCount++;
                                }
                                int index = stockNode.Index - curveCount;
                                decorator.EventPens[index].Color = stockNode.CurvePen.Color;
                            }
                            else
                            {
                                ViewableItemNode vn = (ViewableItemNode)stockNode.Parent;
                                vn.ViewableItem.SeriePens[stockNode.Index].Color = stockNode.CurvePen.Color;
                            }
                            break;
                    }

                    this.lineColorPanel.BackColor = colorDlg.Color;
                    this.SaveCustomColors(this.colorDlg.CustomColors);
                }
                this.previewPanel.Refresh();
            }
        }
        #endregion
        #region Indicator1 Config
        private void ActivateIndicatorConfigPanel(string groupBoxText)
        {
            IStockViewableSeries viewableItem = ((ViewableItemNode)this.treeView1.SelectedNode).ViewableItem;

            ResourceManager resources = new ResourceManager(typeof(IndicatorDlgs));

            this.indicatorConfigBox.Text = resources.GetString(groupBoxText);
            this.MakeVisible(indicatorConfigBox);
            this.paramListView.Items.Clear();

            ListViewItem[] viewItems = new ListViewItem[viewableItem.ParameterCount];
            for (int i = 0; i < viewableItem.ParameterCount; i++)
            {
                viewItems[i] = new ListViewItem(new string[] { viewableItem.Parameters[i].ToString(), viewableItem.ParameterNames[i], viewableItem.ParameterRanges[i].MinValue.ToString(), viewableItem.ParameterRanges[i].MaxValue.ToString(), resources.GetString(viewableItem.ParameterTypes[i].Name) });
            }
            this.paramListView.Items.AddRange(viewItems);
        }

        void paramListView_Click(object sender, System.EventArgs e)
        {
            if (this.paramListView.SelectedIndices.Count != 0)
            {
                int index = this.paramListView.SelectedIndices[0];
                this.paramListView.SelectedItems[0].BeginEdit();
            }
        }
        private void paramListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ViewableItemNode ParamNode = (ViewableItemNode)this.treeView1.SelectedNode;
            if (ParamNode.ViewableItem == null || e.Label == null) return;

            ListViewItem item = this.paramListView.Items[e.Item];
            string type = ParamNode.ViewableItem.ParameterTypes[e.Item].Name;
            if (ParamNode.ViewableItem.ParameterRanges[e.Item].isValidString(e.Label))
            {
                switch (type)
                {
                    case "Single":
                        ParamNode.ViewableItem.Parameters[e.Item] = float.Parse(e.Label);
                        e.CancelEdit = false;
                        RefreshNode(ParamNode);
                        break;
                    case "Int32":
                        ParamNode.ViewableItem.Parameters[e.Item] = int.Parse(e.Label);
                        e.CancelEdit = false;
                        RefreshNode(ParamNode);
                        break;
                    case "Boolean":
                        ParamNode.ViewableItem.Parameters[e.Item] = bool.Parse(e.Label);
                        e.CancelEdit = false;
                        RefreshNode(ParamNode);
                        break;
                    case "String":
                        ParamNode.ViewableItem.Parameters[e.Item] = e.Label.ToUpper();
                        e.CancelEdit = false;
                        RefreshNode(ParamNode);
                        break;
                    case "DateTime":
                        ParamNode.ViewableItem.Parameters[e.Item] = DateTime.Parse(e.Label);
                        e.CancelEdit = false;
                        RefreshNode(ParamNode);
                        break;
                    default:
                        throw new NotImplementedException($"Type {type} not implemented");
                }
            }
            else
            {
                ResourceManager resources = new ResourceManager(typeof(IndicatorDlgs));
                string msgBoxTitle = resources.GetString("InvalidIndicatorParameter");
                string msgBoxErrorMsg;
                if (type == "Boolean")
                {
                    msgBoxErrorMsg = resources.GetString("ExpectedIndicatorParameterBool");
                }
                else if (type == "String")
                {
                    msgBoxErrorMsg = resources.GetString("ExpectedIndicatorParameterStringList");
                }
                else
                {

                    msgBoxErrorMsg = resources.GetString("ExpectedIndicatorParameterRange");
                    msgBoxErrorMsg = msgBoxErrorMsg.Replace("$type", resources.GetString(type));
                    msgBoxErrorMsg = msgBoxErrorMsg.Replace("$min", ParamNode.ViewableItem.ParameterRanges[e.Item].MinValue.ToString());
                    msgBoxErrorMsg = msgBoxErrorMsg.Replace("$max", ParamNode.ViewableItem.ParameterRanges[e.Item].MaxValue.ToString());
                }
                MessageBox.Show(msgBoxErrorMsg, msgBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.CancelEdit = true;
            }
        }
        private void RefreshNode(ViewableItemNode paramNode)
        {
            int i = 0;
            foreach (StockNode stockNode in paramNode.Nodes)
            {
                if (stockNode.Type == NodeType.Curve)
                {
                    stockNode.Text = paramNode.ViewableItem.SerieNames[i++];
                }
            }
            paramNode.Text = paramNode.ViewableItem.Name;

            if (paramNode.Type == NodeType.Indicator)
            {
                // Replace Lines
                List<StockNode> nodeList = new List<StockNode>();
                foreach (StockNode node in paramNode.Parent.Nodes)
                {
                    if (node.Type == NodeType.Line)
                    {
                        nodeList.Add(node);
                    }
                }
                foreach (StockNode node in nodeList)
                {
                    paramNode.Parent.Nodes.Remove(node);
                }

                IStockIndicator stockIndicator = (IStockIndicator)paramNode.ViewableItem;
                if (stockIndicator.HorizontalLines != null)
                {
                    foreach (HLine line in stockIndicator.HorizontalLines)
                    {
                        string lineName = "LINE_" + line.Level;
                        paramNode.Parent.Nodes.Add(new LineNode(lineName, this.indicatorMenuStrip, line.LinePen, line.Level));
                    }
                }
            }
        }
        #endregion
        #region Indicator1 Config
        private void ActivateCloudConfigPanel(string groupBoxText)
        {
            IStockViewableSeries viewableItem = ((ViewableItemNode)this.treeView1.SelectedNode).ViewableItem;

            ResourceManager resources = new ResourceManager(typeof(IndicatorDlgs));

            this.indicatorConfigBox.Text = resources.GetString(groupBoxText);
            this.MakeVisible(indicatorConfigBox);
            this.paramListView.Items.Clear();

            ListViewItem[] viewItems = new ListViewItem[viewableItem.ParameterCount];
            for (int i = 0; i < viewableItem.ParameterCount; i++)
            {
                viewItems[i] = new ListViewItem(new string[] { viewableItem.Parameters[i].ToString(), viewableItem.ParameterNames[i], viewableItem.ParameterRanges[i].MinValue.ToString(), viewableItem.ParameterRanges[i].MaxValue.ToString(), resources.GetString(viewableItem.ParameterTypes[i].Name) });
            }
            this.paramListView.Items.AddRange(viewItems);
        }
        #endregion
        #region Horizontal Lines config
        private void ActivateLineConfigPanel(LineNode lineNode)
        {
            this.MakeVisible(lineConfigBox);

            this.lineTypeComboBox.Enabled = true;
            this.lineTypeComboBox.Parent = lineConfigBox;
            this.thicknessComboBox.Parent = lineConfigBox;
            this.lineColorPanel.Parent = lineConfigBox;

            this.lineTypeComboBox.SelectedItem = lineNode.CurvePen.DashStyle.ToString();
            this.thicknessComboBox.SelectedItem = (int)lineNode.CurvePen.Width;
            this.lineColorPanel.BackColor = lineNode.CurvePen.Color;
            this.lineValueTextBox.Text = lineNode.LineValue.ToString();
        }

        void lineConfigBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            float lineLevel;
            if (float.TryParse(this.lineValueTextBox.Text, out lineLevel))
            {
                ((LineNode)this.treeView1.SelectedNode).LineValue = lineLevel;
                ((LineNode)this.treeView1.SelectedNode).Text = "LINE_" + this.lineValueTextBox.Text;
                this.lineValueTextBox.BackColor = this.treeView1.BackColor;
                e.Cancel = false;
            }
            else
            {
                this.lineValueTextBox.BackColor = Color.Red;
                this.lineValueTextBox.Focus();
                this.lineValueTextBox.SelectionStart = 0;
                this.lineValueTextBox.SelectionLength = this.lineValueTextBox.Text.Length;
                e.Cancel = true;
            }
        }
        #endregion
        #region Treeview events
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            StockNode treeNode = (StockNode)e.Node;
            switch (treeNode.Type)
            {
                case NodeType.Graph:
                    ActivateGraphConfigPanel((GraphNode)treeNode);
                    if (treeNode.Text == "CloseGraph")
                    {
                        this.addIndicatorToolStripMenuItem.Visible = true;
                        this.addHorizontalLineToolStripMenuItem.Visible = true;
                        this.addPaintBarsToolStripMenuItem.Visible = true;
                        this.addAutoDrawingsToolStripMenuItem.Visible = true;
                        this.addTrailStopsToolStripMenuItem.Visible = true;
                    }
                    else if (treeNode.Text.StartsWith("Indicator"))
                    {
                        this.addIndicatorToolStripMenuItem.Visible = true;
                        this.addHorizontalLineToolStripMenuItem.Visible = true;
                        this.addPaintBarsToolStripMenuItem.Visible = false;
                        this.addAutoDrawingsToolStripMenuItem.Visible = false;
                        this.addTrailStopsToolStripMenuItem.Visible = false;
                    }
                    else
                    {
                        this.addIndicatorToolStripMenuItem.Visible = false;
                        this.addHorizontalLineToolStripMenuItem.Visible = false;
                        this.addPaintBarsToolStripMenuItem.Visible = false;
                        this.addAutoDrawingsToolStripMenuItem.Visible = false;
                        this.addTrailStopsToolStripMenuItem.Visible = false;
                    }
                    break;
                case NodeType.Trail:
                    ActivateIndicatorConfigPanel("TrailStopParam");
                    this.removeStripMenuItem.Visible = true;
                    this.addDecoratorToolStripMenuItem.Visible = false;
                    this.addTrailToolStripMenuItem.Visible = false;
                    break;
                case NodeType.Decorator:
                    ActivateIndicatorConfigPanel("DecoratorParam");
                    this.removeStripMenuItem.Visible = true;
                    this.addDecoratorToolStripMenuItem.Visible = false;
                    this.addTrailToolStripMenuItem.Visible = false;
                    break;
                case NodeType.Indicator:
                    {
                        ActivateIndicatorConfigPanel("IndicatorParam");
                        ViewableItemNode viewableItemNode = (ViewableItemNode)treeNode;
                        if (viewableItemNode.ViewableItem.DisplayTarget == IndicatorDisplayTarget.PriceIndicator)
                        {
                            this.removeStripMenuItem.Visible = true;
                            this.addDecoratorToolStripMenuItem.Visible = false;
                            this.addTrailToolStripMenuItem.Visible = false;
                        }
                        else
                        {
                            this.removeStripMenuItem.Visible = true;
                            this.addDecoratorToolStripMenuItem.Visible = true;
                            this.addTrailToolStripMenuItem.Visible = true;
                        }
                    }
                    break;
                case NodeType.Cloud:
                    {
                        ActivateCloudConfigPanel("CloudParam");

                        this.removeStripMenuItem.Visible = true;
                        this.copyStripMenuItem.Visible = true;
                        this.addDecoratorToolStripMenuItem.Visible = false;
                        this.addTrailToolStripMenuItem.Visible = false;
                    }
                    break;
                case NodeType.PaintBars:
                    {
                        ActivateIndicatorConfigPanel("PaintBarParam");
                        ViewableItemNode viewableItemNode = (ViewableItemNode)treeNode;
                        this.removeStripMenuItem.Visible = true;
                        this.copyStripMenuItem.Visible = true;
                        if (viewableItemNode.ViewableItem.DisplayTarget == IndicatorDisplayTarget.PriceIndicator)
                        {
                            this.addDecoratorToolStripMenuItem.Visible = false;
                            this.addTrailToolStripMenuItem.Visible = false;
                        }
                        else
                        {
                            this.addDecoratorToolStripMenuItem.Visible = true;
                            this.addTrailToolStripMenuItem.Visible = true;
                        }
                    }
                    break;
                case NodeType.AutoDrawings:
                    {
                        ActivateIndicatorConfigPanel("AutoDrawingParam");
                        ViewableItemNode viewableItemNode = (ViewableItemNode)treeNode;
                        this.removeStripMenuItem.Visible = true;
                        this.copyStripMenuItem.Visible = true;
                        this.addDecoratorToolStripMenuItem.Visible = false;
                        this.addTrailToolStripMenuItem.Visible = false;
                    }
                    break;
                case NodeType.TrailStops:
                    {
                        ActivateIndicatorConfigPanel("TrailStopParam");
                        ViewableItemNode viewableItemNode = (ViewableItemNode)treeNode;
                        this.removeStripMenuItem.Visible = true;
                        this.copyStripMenuItem.Visible = true;
                        if (viewableItemNode.ViewableItem.DisplayTarget == IndicatorDisplayTarget.PriceIndicator)
                        {
                            this.addDecoratorToolStripMenuItem.Visible = false;
                            this.addTrailToolStripMenuItem.Visible = false;
                        }
                        else
                        {
                            this.addDecoratorToolStripMenuItem.Visible = true;
                            this.addTrailToolStripMenuItem.Visible = true;
                        }
                    }
                    break;
                case NodeType.Curve:
                    if (((StockNode)treeNode.Parent).Type == NodeType.PaintBars)
                    {
                        ActivatePaintBarConfigPanel((CurveNode)treeNode);
                    }
                    if (((StockNode)treeNode.Parent).Type == NodeType.Cloud)
                    {
                        ActivateCloudConfigPanel((CurveNode)treeNode);
                    }
                    else
                    {
                        ActivateCurveConfigPanel((CurveNode)treeNode);
                    }
                    break;
                case NodeType.Event:
                    ActivateCurveConfigPanel((EventNode)treeNode);
                    break;
                case NodeType.Line:
                    ActivateLineConfigPanel((LineNode)treeNode);
                    this.removeStripMenuItem.Visible = true;
                    this.addDecoratorToolStripMenuItem.Visible = false;
                    this.addTrailToolStripMenuItem.Visible = false;
                    break;
                default:
                    break;
            }
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.treeView1.SelectedNode = e.Node;
            }
        }
        #endregion
        #region Graph parameters
        private void ActivateGraphConfigPanel(GraphNode graphNode)
        {
            this.MakeVisible(graphConfigBox);
            this.suspendPreview = true;

            if (graphNode.Text.ToUpper() == "CLOSEGRAPH")
            {
                this.label8.Visible = true;
                this.chartModeComboBox.Visible = true;
                this.chartModeComboBox.SelectedItem = graphNode.GraphMode.ToString();
                this.secondarySerieGroupBox.Visible = true;
                this.secondaryColorPanel.BackColor = graphNode.SecondaryPen.Color;
                this.secondaryThicknessComboBox.SelectedItem = (int)graphNode.SecondaryPen.Width;
            }
            else
            {
                this.label8.Visible = false;
                this.chartModeComboBox.Visible = false;
                this.secondarySerieGroupBox.Visible = false;
            }

            this.backgroundColorPanel.BackColor = graphNode.GraphBackgroundColor;
            this.textBackgroundColorPanel.BackColor = graphNode.GraphTextBackgroundColor;
            this.gridColorPanel.BackColor = graphNode.GraphGridColor;
            this.showGridCheckBox.Checked = graphNode.GraphShowGrid;

            this.suspendPreview = false;
            this.graphPreviewPanel.Refresh();
        }
        private void showGridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GraphNode graphNode = (GraphNode)this.treeView1.SelectedNode;
            if (graphNode != null)
            {
                graphNode.GraphShowGrid = this.showGridCheckBox.Checked;
            }
            this.graphPreviewPanel.Invalidate();
        }
        private void chartModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GraphNode graphNode = (GraphNode)this.treeView1.SelectedNode;
            if (graphNode != null)
            {
                graphNode.GraphMode = (GraphChartMode)Enum.Parse(typeof(GraphChartMode), this.chartModeComboBox.SelectedItem.ToString());
            }
            this.graphPreviewPanel.Invalidate();
        }
        private void applyToAllButton_Click(object sender, EventArgs e)
        {
            GraphNode graphNode = ((GraphNode)this.treeView1.SelectedNode);
            foreach (TreeNode treeNode in this.treeView1.Nodes)
            {
                if (((StockNode)treeNode).Type == NodeType.Graph)
                {
                    ((GraphNode)treeNode).GraphBackgroundColor = graphNode.GraphBackgroundColor;
                    ((GraphNode)treeNode).GraphTextBackgroundColor = graphNode.GraphTextBackgroundColor;
                    ((GraphNode)treeNode).GraphShowGrid = graphNode.GraphShowGrid;
                    ((GraphNode)treeNode).GraphGridColor = graphNode.GraphGridColor;
                }
            }
            this.applyButton_Click(sender, e);
        }
        private void graphPreviewPanel_Paint(object sender, PaintEventArgs e)
        {
            if (this.suspendPreview) return;
            GraphNode graphNode = (GraphNode)this.treeView1.SelectedNode;
            if (graphNode == null)
            {
                return;
            }

            Graphics g = this.graphPreviewPanel.CreateGraphics();
            g.Clear(graphNode.GraphBackgroundColor);
            g.DrawRectangle(Pens.Black, 0, 0, g.VisibleClipBounds.Width - 1, g.VisibleClipBounds.Height - 1);

            using (Pen gridPen = new Pen(graphNode.GraphGridColor))
            {
                if (graphNode.GraphShowGrid)
                {

                    {
                        {
                            for (int xx = 0; xx < g.VisibleClipBounds.Width; xx += 33)
                            {
                                g.DrawLine(gridPen, xx, g.VisibleClipBounds.Bottom, xx, g.VisibleClipBounds.Top);
                            }
                            for (int yy = 0; yy < g.VisibleClipBounds.Height; yy += 33)
                            {
                                g.DrawLine(gridPen, g.VisibleClipBounds.Left, yy, g.VisibleClipBounds.Right, yy);
                            }
                        }
                    }

                    if (graphNode.Text.ToUpper() == "CLOSEGRAPH" && graphNode.GraphMode == GraphChartMode.BarChart)
                    {
                        PaintPreviewWithPaintBars(g, Pens.Black);
                    }
                    else
                    {
                        if (graphNode.Text.ToUpper() == "CLOSEGRAPH" && graphNode.GraphMode == GraphChartMode.CandleStick)
                        {
                            PaintPreviewWithCandleSticks(g, Pens.Black);
                        }
                        else
                        {
                            float x, y;
                            Point[] points = new Point[(int)Math.Floor(g.VisibleClipBounds.Width)];
                            int i = 0;
                            for (x = g.VisibleClipBounds.Left; x < g.VisibleClipBounds.Right; x++)
                            {
                                y = (int)(Math.Sin(x * Math.PI * 6.0 / g.VisibleClipBounds.Width) * 0.4f * g.VisibleClipBounds.Height);
                                points[i].X = (int)x;
                                points[i++].Y = (int)(y - (g.VisibleClipBounds.Top - g.VisibleClipBounds.Bottom) / 2.0f);
                            }
                            g.DrawLines(Pens.Black, points);
                        }
                    }

                    using (Brush brush = new SolidBrush(graphNode.GraphTextBackgroundColor))
                    {
                        using (Brush textBrush = new SolidBrush(Color.Black))
                        {
                            using (Font font = new Font(FontFamily.GenericSansSerif, 9))
                            {

                                SizeF size = g.MeasureString(this.someTextLabel.Text, font);
                                PointF location = new PointF((g.VisibleClipBounds.Left + g.VisibleClipBounds.Right) / 2.0f + 15f, (g.VisibleClipBounds.Top + g.VisibleClipBounds.Bottom) / 2.0f + 30f);
                                g.FillRectangle(brush, location.X - 1, location.Y - 1, size.Width, size.Height + 2);
                                g.DrawRectangle(gridPen, location.X - 1, location.Y - 1, size.Width, size.Height + 2);
                                g.DrawString(this.someTextLabel.Text, font, textBrush, location);
                            }
                        }
                    }
                }
            }
        }
        private void backgroundColorPanel_Click(object sender, EventArgs e)
        {
            colorDlg.Color = ((GraphNode)this.treeView1.SelectedNode).GraphBackgroundColor;
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((GraphNode)this.treeView1.SelectedNode).GraphBackgroundColor = colorDlg.Color;
                this.backgroundColorPanel.BackColor = colorDlg.Color;

                this.SaveCustomColors(this.colorDlg.CustomColors);
            }
        }
        private void textBackgroundColorPanel_Click(object sender, EventArgs e)
        {
            colorDlg.Color = ((GraphNode)this.treeView1.SelectedNode).GraphTextBackgroundColor;
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((GraphNode)this.treeView1.SelectedNode).GraphTextBackgroundColor = colorDlg.Color;
                this.textBackgroundColorPanel.BackColor = colorDlg.Color;

                this.SaveCustomColors(this.colorDlg.CustomColors);
            }
        }
        private void gridColorPanel_Click(object sender, EventArgs e)
        {
            colorDlg.Color = ((GraphNode)this.treeView1.SelectedNode).GraphGridColor;
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((GraphNode)this.treeView1.SelectedNode).GraphGridColor = colorDlg.Color;
                this.gridColorPanel.BackColor = colorDlg.Color;

                this.SaveCustomColors(this.colorDlg.CustomColors);
            }
        }
        #endregion
        #region PAINTBAR PARAMETERS
        private void ActivatePaintBarConfigPanel(CurveNode curveNode)
        {
            this.MakeVisible(this.paintBarGroupBox);
            this.suspendPreview = true;

            this.lineTypeComboBox.Parent = this.paintBarGroupBox;
            this.thicknessComboBox.Parent = this.paintBarGroupBox;
            this.lineColorPanel.Parent = this.paintBarGroupBox;

            if (curveNode.SupportVisibility)
            {
                this.visibleCheckBox.Parent = paintBarGroupBox;
                this.visibleCheckBox.Visible = true;
                this.visibleCheckBox.Checked = curveNode.Visible;
            }
            else
            {
                this.visibleCheckBox.Visible = false;
            }

            this.lineTypeComboBox.SelectedItem = curveNode.CurvePen.DashStyle.ToString();
            this.thicknessComboBox.SelectedItem = (int)curveNode.CurvePen.Width;
            this.lineColorPanel.BackColor = curveNode.CurvePen.Color;

            this.curvePreviewLabel.Parent = this.paintBarGroupBox;
            this.previewPanel.Parent = this.paintBarGroupBox;
            this.suspendPreview = false;
            this.previewPanel.Refresh();
        }

        #endregion

        #region CLOUD PARAMATERS
        private void ActivateCloudConfigPanel(CurveNode curveNode)
        {
            this.MakeVisible(this.cloudGroupBox);
            this.suspendPreview = true;

            this.lineTypeComboBox.Parent = this.cloudGroupBox;
            this.thicknessComboBox.Parent = this.cloudGroupBox;
            this.lineColorPanel.Parent = this.cloudGroupBox;

            if (curveNode.SupportVisibility)
            {
                this.visibleCheckBox.Parent = cloudGroupBox;
                this.visibleCheckBox.Visible = true;
                this.visibleCheckBox.Checked = curveNode.Visible;
            }
            else
            {
                this.visibleCheckBox.Visible = false;
            }

            this.lineTypeComboBox.SelectedItem = curveNode.CurvePen.DashStyle.ToString();
            this.thicknessComboBox.SelectedItem = (int)curveNode.CurvePen.Width;
            this.lineColorPanel.BackColor = curveNode.CurvePen.Color;

            this.curvePreviewLabel.Parent = this.cloudGroupBox;
            this.previewPanel.Parent = this.cloudGroupBox;
            this.suspendPreview = false;
            this.previewPanel.Refresh();
        }

        #endregion
        private void applyButton_Click(object sender, EventArgs e)
        {
            if (this.ThemeEdited != null)
            {
                this.ThemeEdited(this.GetTheme());
            }
        }
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
                if (stockNode.Type == NodeType.Graph
                    || stockNode.Type == NodeType.Curve
                    || stockNode.Type == NodeType.Event)
                {
                    return;
                }
                this.treeView1.Nodes.Remove(stockNode);
            }
        }

        private void addPaintBarsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPaintBarDlg addDlg;
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            switch (stockNode.Text)
            {
                case "CloseGraph":
                    addDlg = new AddPaintBarDlg();
                    break;
                default:
                    return;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Only one paint bar per graph
                int index = 0;
                bool found = false;
                foreach (StockNode node in stockNode.Nodes)
                {
                    if (node.Type == NodeType.PaintBars)
                    {
                        found = true;
                        break;
                    }
                    index++;
                }
                if (found)
                {
                    stockNode.Nodes.RemoveAt(index);
                }

                // Add new paint bar
                IStockPaintBar stockPaintBar = StockPaintBarManager.CreatePaintBar(addDlg.PaintBarName);
                StockNode paintBarNode = new PaintBarsNode(stockPaintBar.Name, this.indicatorMenuStrip, stockPaintBar);
                stockNode.Nodes.Add(paintBarNode);
                int i = 0;
                foreach (string paintBarName in stockPaintBar.SerieNames)
                {
                    paintBarNode.Nodes.Add(new CurveNode(paintBarName, null, stockPaintBar.SeriePens[i], stockPaintBar.SerieVisibility[i]));
                    i++;
                }
                this.treeView1.SelectedNode = paintBarNode;
                paintBarNode.Expand();
            }
        }

        private void addAutoDrawingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddAutoDrawingDlg addDlg;
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            switch (stockNode.Text)
            {
                case "CloseGraph":
                    addDlg = new AddAutoDrawingDlg();
                    break;
                default:
                    return;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Only one paint bar per graph
                int index = 0;
                bool found = false;
                foreach (StockNode node in stockNode.Nodes)
                {
                    if (node.Type == NodeType.AutoDrawings)
                    {
                        found = true;
                        break;
                    }
                    index++;
                }
                if (found)
                {
                    stockNode.Nodes.RemoveAt(index);
                }

                // Add new paint bar
                IStockAutoDrawing stockAutoDrawing = StockAutoDrawingManager.CreateAutoDrawing(addDlg.AutoDrawingName);
                StockNode autoDrawingNode = new AutoDrawingsNode(stockAutoDrawing.Name, this.indicatorMenuStrip, stockAutoDrawing);
                stockNode.Nodes.Add(autoDrawingNode);
                int i = 0;
                foreach (string autoDrawingName in stockAutoDrawing.SerieNames)
                {
                    autoDrawingNode.Nodes.Add(new CurveNode(autoDrawingName, null, stockAutoDrawing.SeriePens[i], stockAutoDrawing.SerieVisibility[i]));
                    i++;
                }
                this.treeView1.SelectedNode = autoDrawingNode;
                autoDrawingNode.Expand();
            }
        }
        private void addTrailStopsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTrailStopDlg addDlg;
            StockNode stockNode = (StockNode)this.treeView1.SelectedNode;
            switch (stockNode.Text)
            {
                case "CloseGraph":
                    addDlg = new AddTrailStopDlg();
                    break;
                default:
                    return;
            }
            if (addDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Only one trail stop per graph
                int index = 0;
                bool found = false;
                foreach (StockNode node in stockNode.Nodes)
                {
                    if (node.Type == NodeType.TrailStops)
                    {
                        found = true;
                        break;
                    }
                    index++;
                }
                if (found)
                {
                    stockNode.Nodes.RemoveAt(index);
                }

                // Add new trail stop
                IStockTrailStop stockTrailStop = StockTrailStopManager.CreateTrailStop(addDlg.TrailStopName);
                StockNode trailStopNode = new TrailStopsNode(stockTrailStop.Name, this.indicatorMenuStrip, stockTrailStop);
                stockNode.Nodes.Add(trailStopNode);
                int i = 0;
                foreach (string trailStopName in stockTrailStop.SerieNames)
                {
                    trailStopNode.Nodes.Add(new CurveNode(trailStopName, null, stockTrailStop.SeriePens[i], true));
                    i++;
                }
                this.treeView1.SelectedNode = trailStopNode;
                trailStopNode.Expand();
            }
        }

        private void secondaryThicknessComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GraphNode stockNode = (GraphNode)this.treeView1.SelectedNode;
            if (stockNode != null && stockNode.SecondaryPen != null)
            {
                stockNode.SecondaryPen.Width = (int)this.secondaryThicknessComboBox.SelectedItem;
            }
        }
        void secondaryColorPanel_Click(object sender, System.EventArgs e)
        {
            colorDlg.Color = ((GraphNode)this.treeView1.SelectedNode).SecondaryPen.Color;
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((GraphNode)this.treeView1.SelectedNode).SecondaryPen.Color = colorDlg.Color;
                this.secondaryColorPanel.BackColor = colorDlg.Color;

                this.SaveCustomColors(this.colorDlg.CustomColors);
            }
        }
    }
}