using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    public partial class FullGraphUserControl : UserControl
    {
        public event MouseValueChangedHandler OnMouseDateChanged;

        public FullGraphUserControl(BarDuration duration)
        {
            InitializeComponent();

            this.durationComboBox.Items.AddRange(StockBarDuration.BarDurations.Cast<object>().ToArray());
            this.durationComboBox.SelectedItem = duration;
            this.durationComboBox.SelectedValueChanged += durationComboBox_SelectedValueChanged;

            // this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(graphScrollerControl_ZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphCloseControl.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator2Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator3Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphIndicator1Control.OnZoomChanged);
            this.graphScrollerControl.ZoomChanged += new OnZoomChangedHandler(this.graphVolumeControl.OnZoomChanged);

            // Fill the control list
            GraphControl.DrawingPen = GraphCurveType.PenFromString(Settings.Default.DrawingPen);
            this.graphList.Add(this.graphCloseControl);
            this.graphList.Add(this.graphScrollerControl);
            this.graphList.Add(this.graphIndicator1Control);
            this.graphList.Add(this.graphIndicator2Control);
            this.graphList.Add(this.graphIndicator3Control);
            this.graphList.Add(this.graphVolumeControl);

            foreach (var graph in graphList)
            {
                graph.MouseMove += this.MouseMoveOverGraphControl;

                if (graph != this.graphScrollerControl)
                {
                    graph.OnMouseValueChanged += graph_OnMouseDateChanged;
                }
            }
        }

        void graph_OnMouseDateChanged(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            if (this.OnMouseDateChanged != null)
            {
                this.OnMouseDateChanged(this, date, value, crossMode);
            }
        }

        public void SetDuration(BarDuration duration)
        {
            this.durationComboBox.SelectedItem = duration;
        }
        void durationComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (currentStockSerie != null)
            {
                this.ApplyTheme();
            }
        }
        private readonly List<GraphControl> graphList = new List<GraphControl>();

        private Point lastMouseLocation = Point.Empty;
        void MouseMoveOverGraphControl(object sender, MouseEventArgs e)
        {
            if (lastMouseLocation != e.Location)
            {
                try
                {
                    GraphControl graphControl = (GraphControl)sender;
                    if (graphControl.GraphRectangle.Contains(e.Location) && e.Location.X > graphControl.GraphRectangle.X)
                    {
                        if (graphControl == this.graphScrollerControl && graphControl.IsInitialized)
                        {
                            graphControl.MouseMoveOverControl(e, Control.ModifierKeys, true);
                        }
                        else
                        {
                            foreach (GraphControl graph in graphList)
                            {
                                if (graph.IsInitialized)
                                {
                                    graph.MouseMoveOverControl(e, Control.ModifierKeys, graph == sender);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (graphControl.GraphRectangle.Contains(lastMouseLocation))
                        {
                            foreach (GraphControl graph in graphList)
                            {
                                if (graph.IsInitialized)
                                {
                                    graph.PaintForeground();
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    StockLog.Write(exception);
                }
                lastMouseLocation = e.Location;

            }
        }

        private void DeactivateGraphControls(string msg)
        {
            this.graphCloseControl.Deactivate(msg, false);
            this.graphScrollerControl.Deactivate(msg, false);
            this.graphIndicator1Control.Deactivate(msg, false);
            this.graphIndicator2Control.Deactivate(msg, false);
            this.graphIndicator3Control.Deactivate(msg, false);
            this.graphVolumeControl.Deactivate(msg, false);
            this.Cursor = Cursors.Arrow;
        }

        private int startIndex;
        public int StartIndex
        {
            get { return startIndex; }
            set { startIndex = value; }
        }

        private int endIndex;
        public int EndIndex
        {
            get { return endIndex; }
            set { endIndex = value; }
        }

        private StockSerie currentStockSerie;
        public StockSerie CurrentStockSerie
        {
            get { return currentStockSerie; }
            set { currentStockSerie = value; }
        }

        public void ApplyTheme(string themeName = null)
        {
            using MethodLogger ml = new MethodLogger(this);
            try
            {
                var currentTheme = string.IsNullOrEmpty(themeName) ? StockAnalyzerForm.MainFrame.GetCurrentTheme() : StockAnalyzerForm.MainFrame.GetTheme(themeName);

                if (currentTheme == null || currentStockSerie == null) return;
                if (!currentStockSerie.IsInitialised)
                {
                    this.Refresh();
                }
                if (!currentStockSerie.Initialise() || currentStockSerie.Count == 0)
                {
                    this.DeactivateGraphControls("Data for " + currentStockSerie.StockName + " cannot be initialised");
                    return;
                }

                // Set the bar duration
                currentStockSerie.BarDuration = (BarDuration)this.durationComboBox.SelectedItem;

                this.StartIndex = Math.Max(0, currentStockSerie.Count - Settings.Default.DefaultBarNumber);
                this.EndIndex = currentStockSerie.Count - 1;

                if (currentStockSerie.StockAnalysis.DeleteTransientDrawings() > 0)
                {
                    currentStockSerie.PaintBarCache = null;
                }

                // Force resetting the secondary serie.
                if (currentTheme["CloseGraph"].FindIndex(s => s.StartsWith("SECONDARY")) == -1)
                {
                    if (this.graphCloseControl.SecondaryFloatSerie != null)
                    {
                        currentTheme["CloseGraph"].Add("SECONDARY|" + this.graphCloseControl.SecondaryFloatSerie.Name);
                    }
                    else
                    {
                        currentTheme["CloseGraph"].Add("SECONDARY|NONE");
                    }
                }

                DateTime[] dateSerie = currentStockSerie.Keys.ToArray();
                GraphCurveTypeList curveList;
                bool skipEntry = false;
                foreach (string entry in currentTheme.Keys)
                {
                    if (entry.ToUpper().EndsWith("GRAPH"))
                    {
                        GraphControl graphControl = null;
                        curveList = new GraphCurveTypeList();
                        switch (entry.ToUpper())
                        {
                            case "CLOSEGRAPH":
                                graphControl = this.graphCloseControl;
                                this.graphCloseControl.Agenda = currentStockSerie.Agenda;
                                this.graphCloseControl.Dividends = currentStockSerie.Dividend;
                                break;
                            case "SCROLLGRAPH":
                                graphControl = this.graphScrollerControl;
                                break;
                            case "INDICATOR1GRAPH":
                                graphControl = this.graphIndicator1Control;
                                break;
                            case "INDICATOR2GRAPH":
                                graphControl = this.graphIndicator2Control;
                                break;
                            case "INDICATOR3GRAPH":
                                graphControl = this.graphIndicator3Control;
                                break;
                            case "VOLUMEGRAPH":
                                if (currentStockSerie.HasVolume)
                                {
                                    graphControl = this.graphVolumeControl;
                                    curveList.Add(new GraphCurveType(currentStockSerie.GetSerie(StockDataType.EXCHANGED), Pens.Green, true));
                                }
                                else
                                {
                                    this.graphVolumeControl.Deactivate("This serie has no volume data", false);
                                    skipEntry = true;
                                }
                                break;
                            default:
                                continue;
                        }

                        if (skipEntry)
                        {
                            skipEntry = false;
                            continue;
                        }
                        try
                        {
                            List<HLine> horizontalLines = new List<HLine>();

                            foreach (string line in currentTheme[entry])
                            {
                                string[] fields = line.Split('|');
                                switch (fields[0].ToUpper())
                                {
                                    case "GRAPH":
                                        string[] colorItem = fields[1].Split(':');
                                        graphControl.BackgroundColor = Color.FromArgb(int.Parse(colorItem[0]),
                                            int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                        colorItem = fields[2].Split(':');
                                        graphControl.TextBackgroundColor = Color.FromArgb(int.Parse(colorItem[0]),
                                            int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                        graphControl.ShowGrid = bool.Parse(fields[3]);
                                        colorItem = fields[4].Split(':');
                                        graphControl.GridColor = Color.FromArgb(int.Parse(colorItem[0]),
                                            int.Parse(colorItem[1]), int.Parse(colorItem[2]), int.Parse(colorItem[3]));
                                        graphControl.ChartMode =
                                            (GraphChartMode)Enum.Parse(typeof(GraphChartMode), fields[5]);
                                        if (entry.ToUpper() == "CLOSEGRAPH")
                                        {
                                            if (fields.Length >= 7)
                                            {
                                                this.graphCloseControl.SecondaryPen =
                                                    GraphCurveType.PenFromString(fields[6]);
                                            }
                                            else
                                            {
                                                this.graphCloseControl.SecondaryPen = new Pen(Color.DarkGoldenrod, 1);
                                            }
                                        }
                                        break;
                                    case "SECONDARY":
                                        if (currentStockSerie.SecondarySerie != null)
                                        {
                                            this.graphCloseControl.SecondaryFloatSerie = currentStockSerie.GenerateSecondarySerieFromOtherSerie(currentStockSerie.SecondarySerie);
                                        }
                                        else
                                        {
                                            if (fields[1].ToUpper() == "NONE" ||
                                                !StockDictionary.Instance.ContainsKey(fields[1]))
                                            {
                                                this.graphCloseControl.SecondaryFloatSerie = null;
                                            }
                                            else
                                            {
                                                if (StockDictionary.Instance.ContainsKey(fields[1]))
                                                {
                                                    this.graphCloseControl.SecondaryFloatSerie = currentStockSerie.GenerateSecondarySerieFromOtherSerie(StockDictionary.Instance[fields[1]]);
                                                }
                                            }
                                        }
                                        break;
                                    case "DATA":
                                        curveList.Add(
                                            new GraphCurveType(
                                                currentStockSerie.GetSerie(
                                                    (StockDataType)Enum.Parse(typeof(StockDataType), fields[1])),
                                         fields[2], bool.Parse(fields[3])));
                                        break;
                                    case "TRAIL":
                                    case "INDICATOR":
                                        {
                                            IStockIndicator stockIndicator =
                                                (IStockIndicator)
                                                    StockViewableItemsManager.GetViewableItem(line,
                                                        currentStockSerie);
                                            if (stockIndicator != null)
                                            {
                                                if (entry.ToUpper() != "CLOSEGRAPH")
                                                {
                                                    if (stockIndicator.DisplayTarget ==
                                                        IndicatorDisplayTarget.RangedIndicator)
                                                    {
                                                        IRange range = (IRange)stockIndicator;
                                                        ((GraphRangedControl)graphControl).RangeMin = range.Min;
                                                        ((GraphRangedControl)graphControl).RangeMax = range.Max;
                                                    }
                                                    else
                                                    {
                                                        ((GraphRangedControl)graphControl).RangeMin = float.NaN;
                                                        ((GraphRangedControl)graphControl).RangeMax = float.NaN;
                                                    }
                                                }
                                                if (
                                                    !(stockIndicator.RequiresVolumeData &&
                                                      !currentStockSerie.HasVolume))
                                                {
                                                    curveList.Indicators.Add(stockIndicator);
                                                }
                                            }
                                        }
                                        break;
                                    case "CLOUD":
                                        {
                                            var stockCloud = (IStockCloud)StockViewableItemsManager.GetViewableItem(line, this.CurrentStockSerie);
                                            if (stockCloud != null)
                                            {
                                                curveList.Cloud = stockCloud;
                                            }
                                        }
                                        break;
                                    case "PAINTBAR":
                                        {
                                            IStockPaintBar paintBar = (IStockPaintBar)StockViewableItemsManager.GetViewableItem(line, currentStockSerie);
                                            curveList.PaintBar = paintBar;
                                        }
                                        break;
                                    case "AUTODRAWING":
                                        {
                                            IStockAutoDrawing autoDrawing = (IStockAutoDrawing)StockViewableItemsManager.GetViewableItem(line, this.CurrentStockSerie);
                                            curveList.AutoDrawing = autoDrawing;
                                        }
                                        break;
                                    case "DECORATOR":
                                        {
                                            IStockDecorator decorator = (IStockDecorator)StockViewableItemsManager.GetViewableItem(line, currentStockSerie);
                                            curveList.Decorator = decorator;
                                            this.graphCloseControl.CurveList.ShowMes.Add(decorator);
                                        }
                                        break;
                                    case "TRAILSTOP":
                                        {
                                            IStockTrailStop trailStop =
                                                (IStockTrailStop)
                                                    StockViewableItemsManager.GetViewableItem(line,
                                                        currentStockSerie);
                                            curveList.TrailStop = trailStop;
                                        }
                                        break;
                                    case "LINE":
                                        horizontalLines.Add(new HLine(float.Parse(fields[1]),
                                            GraphCurveType.PenFromString(fields[2])));
                                        break;
                                    default:
                                        continue;
                                }
                            }
                            if (curveList.FindIndex(c => c.DataSerie.Name == "CLOSE") < 0)
                            {
                                curveList.Insert(0,
                                    new GraphCurveType(currentStockSerie.GetSerie(StockDataType.CLOSE), Pens.Black,
                                        false));
                            }
                            if (graphControl == this.graphCloseControl)
                            {
                                if (curveList.FindIndex(c => c.DataSerie.Name == "LOW") < 0)
                                {
                                    curveList.Insert(0,
                                        new GraphCurveType(currentStockSerie.GetSerie(StockDataType.LOW), Pens.Black,
                                            false));
                                }
                                if (curveList.FindIndex(c => c.DataSerie.Name == "HIGH") < 0)
                                {
                                    curveList.Insert(0,
                                        new GraphCurveType(currentStockSerie.GetSerie(StockDataType.HIGH), Pens.Black,
                                            false));
                                }
                                if (curveList.FindIndex(c => c.DataSerie.Name == "OPEN") < 0)
                                {
                                    curveList.Insert(0,
                                        new GraphCurveType(currentStockSerie.GetSerie(StockDataType.OPEN), Pens.Black,
                                            false));
                                }
                            }
                            if (!currentStockSerie.StockAnalysis.DrawingItems.ContainsKey(currentStockSerie.BarDuration))
                            {
                                currentStockSerie.StockAnalysis.DrawingItems.Add(currentStockSerie.BarDuration, new StockDrawingItems());
                            }
                            graphControl.Initialize(curveList, horizontalLines, dateSerie,
                                currentStockSerie,
                                currentStockSerie.StockAnalysis.DrawingItems[currentStockSerie.BarDuration],
                                startIndex, endIndex);
                        }
                        catch (Exception e)
                        {
                            StockLog.Write(e);
                        }
                    }
                }

                // Reinitialise drawing
                this.Cursor = Cursors.Arrow;
                this.OnZoomChanged(startIndex, endIndex);
            }

            catch (Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
        }

        private void OnZoomChanged(int startIndex, int endIndex)
        {
            foreach (var graph in this.graphList)
            {
                graph.OnZoomChanged(startIndex, endIndex);
            }
        }

        internal void MouseDateChanged(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            if (sender != this)
            {
                foreach (GraphControl graph in graphList)
                {
                    if (!(graph is GraphScrollerControl) && graph.IsInitialized)
                    {
                        graph.MouseDateChanged(date, value, crossMode);
                    }
                }
            }
        }
    }
}
