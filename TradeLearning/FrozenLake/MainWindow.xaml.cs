using FrozenLake.Agents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FrozenLake
{
    enum Tile
    {
        Empty = 0,
        Wall = 1,
        Reward = 2,
        Punish = 3,
        Agent = 4,
        Visited = 5
    };
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private World world = new World();
        private IAgent agent;
        public MainWindow()
        {
            InitializeComponent();

            agent = new LearningAgent();
            agent.Initialize(world);
            agent.X = 0;
            agent.Y = 3;

            PopulateGrid();

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var move = agent.Move();
            PopulateGrid();

            if (move == MoveAction.Completed)
            {
                timer.Stop();
                MessageBox.Show("Done");
            }
        }

        private void PopulateGrid()
        {
            ColorGrid.Children.Clear();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var tile = world.Tiles[i, j];
                    if (agent.X == i && agent.Y == j)
                    {
                        tile = Tile.Agent;
                    }
                    var rect = new Rectangle
                    {
                        Fill = GetBrushFromValue(tile),
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    ColorGrid.Children.Add(rect);
                }
            }
        }

        private Brush GetBrushFromValue(Tile value)
        {
            // Map int values to colors
            return value switch
            {
                Tile.Empty => Brushes.White,
                Tile.Reward => Brushes.Gold,
                Tile.Punish => Brushes.Red,
                Tile.Wall => Brushes.DarkSlateGray,
                Tile.Agent => Brushes.Indigo,
                Tile.Visited => Brushes.LightGray,
                _ => throw new NotSupportedException($"Enum value for Tile {value} not supported")
            };
        }
    }
}
