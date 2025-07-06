using FrozenLake.Agents;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FrozenLake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private World world = new World();
        private IAgent agent;

        List<IAgent> agents = new List<IAgent> { new LearningNNAgent(), new Agent(), new GreedyAgent(), new LearningAgent(), new QLearningAgent() };

        public MainWindow()
        {
            InitializeComponent();

            agentComboBox.ItemsSource = agents;
            agentComboBox.SelectedItem = agents[0];

            PopulateGrid(false);
        }

        private void RunTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning)
                return;

            var move = agent.Move(allowVisitedCheckBox.IsChecked.Value);
            Debug.WriteLine($"X:{agent.X} Y:{agent.Y} move:{move}");
            PopulateGrid(false);

            if (move == MoveAction.None)
            {
                StopSimulation("Agent Stuck");
            }
            else
            {
                switch (world.Tiles(agent.X, agent.Y))
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
                    case Tile.Empty:
                        world.SetVisited(agent.X, agent.Y);
                        break;
                    case Tile.Visited:
                        break;
                    default:
                        StopSimulation($"Agent on unsupported tile Type ${world.Tiles(agent.X, agent.Y)}");
                        break;
                }
            }
        }


        int cellSize = 55;
        private void PopulateGrid(bool displayWeight)
        {
            ColorGrid.Children.Clear();
            ColorGrid.Rows = world.Size.Height;
            ColorGrid.Columns = world.Size.Width;
            ColorGrid.Width = world.Size.Width * cellSize;
            ColorGrid.Height = world.Size.Height * cellSize;

            for (int y = 0; y < world.Size.Height; y++)
            {
                for (int x = 0; x < world.Size.Width; x++)
                {
                    var tile = world.Tiles(x, y);

                    var grid = new Grid();
                    var rect = new Rectangle
                    {
                        Fill = GetBrushFromValue(tile),
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Height = cellSize,
                        Width = cellSize
                    };

                    grid.Children.Add(rect);
                    if (agent != null && agent.X == x && agent.Y == y)
                    {
                        var ellipse = new Ellipse
                        {
                            Fill = Brushes.Pink,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            Width = 30,
                            Height = 30
                        };
                        grid.Children.Add(ellipse);
                    }

                    if (displayWeight)
                    {
                        if (agent is LearningAgent)
                        {
                            var margin = new Thickness(3);
                            var learningAgent = agent as LearningAgent;
                            grid.Children.Add(new TextBlock { Text = learningAgent.Value[x, y].ToString(".###"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 9 });

                            grid.Children.Add(new TextBlock { Text = learningAgent.Policy[x, y][(int)MoveAction.Up].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = learningAgent.Policy[x, y][(int)MoveAction.Down].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = learningAgent.Policy[x, y][(int)MoveAction.Right].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = learningAgent.Policy[x, y][(int)MoveAction.Left].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, FontSize = 9, Margin = margin });
                        }
                        else if (agent is QLearningAgent)
                        {
                            var margin = new Thickness(3);
                            var learningAgent = agent as QLearningAgent;

                            grid.Children.Add(new TextBlock { Text = learningAgent.Q[x, y][(int)MoveAction.Up].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = learningAgent.Q[x, y][(int)MoveAction.Down].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = learningAgent.Q[x, y][(int)MoveAction.Right].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = learningAgent.Q[x, y][(int)MoveAction.Left].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, FontSize = 9, Margin = margin });
                        }
                        else if (agent is LearningNNAgent)
                        {
                            var margin = new Thickness(3);
                            var learningAgent = agent as LearningNNAgent;

                            var res = learningAgent.GetValuePolicy(x, y);

                            grid.Children.Add(new TextBlock { Text = res.Value.ToString(".###"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 9 });


                            grid.Children.Add(new TextBlock { Text = res.Policy[(int)MoveAction.Up].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = res.Policy[(int)MoveAction.Down].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = res.Policy[(int)MoveAction.Right].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, FontSize = 9, Margin = margin });
                            grid.Children.Add(new TextBlock { Text = res.Policy[(int)MoveAction.Left].ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, FontSize = 9, Margin = margin });
                        }
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
                Tile.Wall => Brushes.LightSlateGray,
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
                runButton.Content = "Run";
            }
            if (message != null)
            {
                MessageBox.Show(message);
            }
        }

        bool isRunning = false;
        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                runButton.Content = "Run";
            }
            else
            {
                runButton.Content = "Stop";
                world.Reset();

                agent.SetRandomLocation();
                Debug.WriteLine($"X:{agent.X} Y:{agent.Y} move:Start");

                isRunning = true;
                timer.Interval = TimeSpan.FromMilliseconds(250);
                timer.Tick += RunTimer_Tick;
                timer.Start();
            }
        }



        private void agentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.agent = agentComboBox.SelectedItem as IAgent;
            this.agent.Initialize(world, MathExtension.GetRandom(true));
            this.PopulateGrid(true);
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            for (int x = 0; x < world.Size.Width; x++)
            {
                for (int y = 0; y < world.Size.Height; y++)
                {
                    if (world.Tiles(x, y) != Tile.Empty)
                        continue;

                    Debug.WriteLine($"Testing from {x},{y}");

                    world.Reset();
                    agent.X = x; agent.Y = y;

                    bool pathComplete = false;
                    while (!pathComplete)
                    {
                        var move = agent.Move(allowVisitedCheckBox.IsChecked.Value);
                        if (move == MoveAction.None) // Stuck
                        {
                            pathComplete = true;
                            MessageBox.Show("Agent Stuck");
                            return;
                        }
                        else
                        {
                            switch (world.Tiles(agent.X, agent.Y))
                            {
                                case Tile.Wall:
                                    MessageBox.Show("Agent stepped into a wall");
                                    return;
                                case Tile.Reward:
                                    Debug.WriteLine($"Reward");
                                    pathComplete = true;
                                    break;
                                case Tile.Punish:
                                    MessageBox.Show("Agent stepped into a hole");
                                    return;
                                case Tile.Empty:
                                    world.SetVisited(agent.X, agent.Y);
                                    break;
                                case Tile.Visited:
                                    break;
                                default:
                                    MessageBox.Show($"Agent on unsupported tile Type ${world.Tiles(agent.X, agent.Y)}");
                                    return;
                            }
                        }
                    }
                }
            }

            MessageBox.Show("100% Successful !");
            PopulateGrid(true);
        }

        int iteration = 0;
        private (int X, int Y) trainLocation;
        private void trainButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                trainButton.Content = "Train";
            }
            else
            {
                var learningAgent = this.agent as ILearningAgent;
                if (learningAgent == null) { return; }
                if (iteration == 0)
                {
                    iteration = 1;

                    agent.SetRandomLocation();
                    agent.X = 0; agent.Y = 1;
                    trainLocation.X = agent.X;
                    trainLocation.Y = agent.Y;
                }
                else
                {
                    agent.SetRandomLocation();
                }

                var nbEpisodes = int.Parse(iterationTxtBox.Text);
                var learningRate = double.Parse(learningRateTxtBox.Text);
                var epsilon = double.Parse(epsilonTxtBox.Text);
                var discountFactor = double.Parse(discountTxtBox.Text);

                var sw = Stopwatch.StartNew();

                if (resetCheckBox.IsChecked.Value)
                {
                    this.agent.Initialize(world, MathExtension.GetRandom(true));
                }
                learningAgent.Train(nbEpisodes, learningRate, epsilon, discountFactor, allowVisitedCheckBox.IsChecked.Value);
                sw.Stop();
                MessageBox.Show($"Training completed in {sw.Elapsed.TotalSeconds}");

                PopulateGrid(true);
            }
        }
    }
}
