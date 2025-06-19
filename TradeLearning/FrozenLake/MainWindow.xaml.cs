using FrozenLake.Agents;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FrozenLake
{
    public enum Tile
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

        List<IAgent> agents = new List<IAgent> { new Agent(), new GreedyAgent(), new LearningAgent() };

        public MainWindow()
        {
            InitializeComponent();

            agentComboBox.ItemsSource = agents;
            agentComboBox.SelectedItem = agents[2];

            PopulateGrid();
        }

        private void RunTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning)
                return;

            var move = agent.Move();
            Debug.WriteLine($"X:{agent.X} Y:{agent.Y} move:{move}");
            PopulateGrid();

            if (move == MoveAction.None)
            {
                StopSimulation("Agent Stuck");
            }
            else
            {
                switch (world.Tiles[agent.X, agent.Y])
                {
                    case Tile.Wall:
                        StopSimulation("Agent stepped into a wall");
                        break;
                    case Tile.Reward:
                        StopSimulation("Agent won Congratualtions");
                        break;
                    case Tile.Punish:
                        StopSimulation("Agent Dead");
                        break;
                    case Tile.Agent:
                        StopSimulation("Agent on itself, that's a bug");
                        break;
                    case Tile.Empty:
                        world.Tiles[agent.X, agent.Y] = Tile.Visited;
                        break;
                    case Tile.Visited:
                        break;
                    default:
                        StopSimulation($"Agent on unsupported tile Type ${world.Tiles[agent.X, agent.Y]}");
                        break;
                }
            }
        }

        private void PopulateGrid()
        {
            ColorGrid.Children.Clear();
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var tile = world.Tiles[i, j];
                    if (agent != null && agent.X == i && agent.Y == j)
                    {
                        tile = Tile.Agent;
                    }

                    var grid = new Grid
                    {
                    };
                    var rect = new Rectangle
                    {
                        Fill = GetBrushFromValue(tile),
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    grid.Children.Add(rect);
                    if (agent is LearningAgent)
                    {
                        var learningAgent = agent as LearningAgent;
                        grid.Children.Add(new TextBlock { Text = learningAgent.Value[i, j].ToString("F3"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
                    }

                    ColorGrid.Children.Add(grid);
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

        void StopSimulation(string message = null)
        {
            isRunning = false;
            if (timer.IsEnabled)
            {
                timer.Stop();
                startButton.Content = "Start";
            }
            if (message != null)
            {
                MessageBox.Show(message);
            }
        }

        bool isRunning = false;
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                startButton.Content = "Start";
            }
            else
            {
                startButton.Content = "Stop";
                world.Reset();

                this.agent = agentComboBox.SelectedItem as IAgent;

                agent.Initialize(world, MathExtension.GetRandom(true));
                agent.SetRandomLocation();
                Debug.WriteLine($"X:{agent.X} Y:{agent.Y} move:Start");

                isRunning = true;
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += RunTimer_Tick;
                timer.Start();
            }
        }

        private void trainButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                trainButton.Content = "Train";
            }
            else
            {
                this.agent = agentComboBox.SelectedItem as IAgent;

                var learningAgent = this.agent as LearningAgent;
                if (learningAgent == null) { return; }

                trainButton.Content = "Stop";

                agent.Initialize(world, MathExtension.GetRandom(true));
                iteration = 1;

                var error = learningAgent.TrainingIteration();
                Debug.WriteLine($"Iteration: {iteration} Error: {error}");

                PopulateGrid();

                timer.Interval = TimeSpan.FromMilliseconds(5000);
                timer.Tick += TrainTimer_Tick;
                timer.Start();
            }
        }

        int iteration = 0;
        private void TrainTimer_Tick(object sender, EventArgs e)
        {
            var learningAgent = this.agent as LearningAgent;
            if (learningAgent == null) { return; }

            var error = learningAgent.TrainingIteration();
            Debug.WriteLine($"Iteration: {iteration} Error: {error}");

            PopulateGrid();
        }
    }
}
