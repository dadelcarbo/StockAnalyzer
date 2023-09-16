using StockAnalyzer.StockDrawing;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    public enum GraphActionType
    {
        AddItem,
        DeleteItem,
        CutItem
    }
    public class GraphAction
    {
        public GraphActionType ActionType { get; private set; }
        public DrawingItem TargetItem { get; private set; }
        public DrawingItem TargetItem2 { get; private set; }

        public GraphAction(GraphActionType actionType, DrawingItem targetItem)
        {
            this.ActionType = actionType;
            this.TargetItem = targetItem;
            this.TargetItem2 = null;
        }
        public GraphAction(GraphActionType actionType, DrawingItem targetItem, DrawingItem targetItem2)
        {
            this.ActionType = actionType;
            this.TargetItem = targetItem;
            this.TargetItem2 = targetItem2;
        }
    }
}
