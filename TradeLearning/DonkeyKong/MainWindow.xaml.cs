using DonkeyKong.Model;
using DonkeyKong.Model.Agents;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DonkeyKong
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KeyboardAgent keyboardAgent = new KeyboardAgent();

        ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            this.viewModel = (ViewModel)this.Resources["ViewModel"];

            Level.Load();

            this.Loaded += MainWindow_Loaded;

            this.viewModel.PropertyChanged += ViewModel_PropertyChanged;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private DateTime lastUpdate;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (viewModel.State != EngineState.Playing)
                return;

            var now = DateTime.Now;
            double dt = (now - lastUpdate).TotalMilliseconds;

            if (dt < world.Level.Interval)
                return;

            GameTick(this, null);

            lastUpdate = now;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cellWidth = gameCanvas.ActualWidth / 10;
            cellHeight = gameCanvas.ActualHeight / 12;

            #region CREATE PLAYER SPRITES

            PlayerLeft = new Image
            {
                Width = cellWidth,
                Height = cellHeight,
                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/RunLeft.png")),
                Visibility = Visibility.Visible
            };

            PlayerRight = new Image
            {
                Width = cellWidth,
                Height = cellHeight,
                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/RunRight.png")),
                Visibility = Visibility.Visible
            };
            PlayerJumpLeft = new Image
            {
                Width = cellWidth,
                Height = cellHeight,
                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/JumpLeft.png")),
                Visibility = Visibility.Visible
            };
            PlayerJumpRight = new Image
            {
                Width = cellWidth,
                Height = cellHeight,
                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/JumpRight.png")),
                Visibility = Visibility.Visible
            };

            playerShapes.Add(PlayerJumpLeft);
            playerShapes.Add(PlayerJumpRight);
            playerShapes.Add(PlayerLeft);
            playerShapes.Add(PlayerRight);

            #endregion

            startGameBtn_Click(null, null);
        }

        private void OnPlayerDead()
        {
            viewModel.State = EngineState.Idle;
            startGameBtn.Content = "Start";
            MessageBox.Show("Game Over !!!");
        }
        private void OnLevelCompleted()
        {
            viewModel.State = EngineState.Idle;

            if (this.world.NextLevel())
            {
                MessageBox.Show($"You completed level !!! Let go to next.");
                RenderWorldBackground();

                viewModel.State = EngineState.Playing;
            }
            else
            {
                MessageBox.Show($"You won the game, congratulation !!!");
            }
        }

        double cellWidth;
        double cellHeight;
        double ennemyOffsetX;
        double ennemyOffsetY;
        double ennemySizeX;
        double ennemySizeY;

        private World world = World.Instance;

        private void RenderWorldBackground()
        {
            this.gameCanvas.Children.Clear();

            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Height; j++)
                {
                    UIElement shape = null;
                    switch (world.Background[i, j])
                    {
                        case Tiles.FloorLeft:
                            shape = new Image
                            {
                                Width = cellWidth,
                                Height = cellHeight,
                                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/wall.png"))
                            };

                            gameCanvas.Children.Add(shape);
                            Canvas.SetLeft(shape, i * cellWidth);
                            Canvas.SetTop(shape, j * cellHeight);
                            break;
                        case Tiles.FloorRight:
                            shape = new Image
                            {
                                Width = cellWidth,
                                Height = cellHeight,
                                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/wall.png"))
                            };

                            gameCanvas.Children.Add(shape);
                            Canvas.SetLeft(shape, i * cellWidth);
                            Canvas.SetTop(shape, j * cellHeight);
                            break;
                        case Tiles.Ladder:
                            shape = new Image
                            {
                                Width = cellWidth,
                                Height = cellHeight,
                                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/ladder.png"))
                            };

                            gameCanvas.Children.Add(shape);
                            Canvas.SetLeft(shape, i * cellWidth);
                            Canvas.SetTop(shape, j * cellHeight);
                            break;
                        case Tiles.Fire:
                            shape = new Image
                            {
                                Width = cellWidth,
                                Height = cellHeight,
                                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/fire.png"))
                            };

                            gameCanvas.Children.Add(shape);
                            Canvas.SetLeft(shape, i * cellWidth);
                            Canvas.SetTop(shape, j * cellHeight);
                            break;
                        default:
                            break;
                    }
                }
                RenderPlayer();
            }

            var goal = new Image
            {
                Width = cellWidth,
                Height = cellHeight,
                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/key.png"))
            };
            gameCanvas.Children.Add(goal);
            Canvas.SetLeft(goal, world.Goal.X * cellWidth);
            Canvas.SetTop(goal, world.Goal.Y * cellHeight);

            var cave = new Image
            {
                Width = cellWidth,
                Height = cellHeight,
                Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/cave.png"))
            };
            gameCanvas.Children.Add(cave);
            Canvas.SetLeft(cave, world.EnnemySource.X * cellWidth);
            Canvas.SetTop(cave, world.EnnemySource.Y * cellHeight);

            gameCanvas.Children.Add(PlayerLeft);
            gameCanvas.Children.Add(PlayerRight);
            gameCanvas.Children.Add(PlayerJumpLeft);
            gameCanvas.Children.Add(PlayerJumpRight);

        }
        private void RenderWorld()
        {
            // Render ennemies
            var ennemyImages = gameCanvas.Children.OfType<Image>().Where(e => e.Tag is Ennemy).ToList();
            foreach (var ennemy in world.Ennemies)
            {
                var shape = ennemyImages.FirstOrDefault(b => b.Tag == ennemy);
                if (shape == null)
                {
                    shape = new Image
                    {
                        Width = ennemySizeX,
                        Height = ennemySizeY,
                        Source = new BitmapImage(new Uri("pack://application:,,,/Sprites/monster.png")),
                        Tag = ennemy,
                        Margin = new Thickness(0)
                    };
                    // Add the TextBlock to the Canvas
                    gameCanvas.Children.Add(shape);
                }
                Canvas.SetLeft(shape, ennemy.X * cellWidth + ennemyOffsetX);
                Canvas.SetTop(shape, ennemy.Y * cellHeight + ennemyOffsetY);
            }

            foreach (var e in ennemyImages.Where(e => (e.Tag as Ennemy).IsDead))
            {
                gameCanvas.Children.Remove(e);
            }

            // Render player
            RenderPlayer();
        }

        Image PlayerLeft;
        Image PlayerRight;
        Image PlayerJumpLeft;
        Image PlayerJumpRight;


        Image playerShape;
        List<Image> playerShapes = new();

        Key lastKey = Key.None;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            this.keyboardAgent.LastKey = e.Key;
            return;
        }

        private void RenderPlayer()
        {
            if (world.Player.IsJumping)
            {
                if (world.Player.IsMovingRight)
                {
                    playerShape = PlayerJumpRight;
                }
                else
                {
                    playerShape = PlayerJumpLeft;
                }
            }
            else
            {
                if (world.Player.IsMovingRight)
                {
                    playerShape = PlayerRight;
                }
                else
                {
                    playerShape = PlayerLeft;
                }
            }
            playerShape.Visibility = Visibility.Visible;
            Canvas.SetLeft(playerShape, world.Player.X * cellWidth);
            Canvas.SetTop(playerShape, world.Player.Y * cellHeight);

            foreach (var p in playerShapes)
            {
                if (p != playerShape)
                {
                    p.Visibility = Visibility.Collapsed;
                }
            }
            world.Player.Dump(false);

            switch (world.Status)
            {
                case LevelStatus.Running:
                    break;
                case LevelStatus.Completed:
                    OnLevelCompleted();
                    break;
                case LevelStatus.Lost:
                    OnPlayerDead();
                    break;
                default:
                    break;
            }
        }

        private void startGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.State == EngineState.Playing)
            {
                viewModel.State = EngineState.Idle;
                startGameBtn.Content = "Start";
                levelComboBox.IsEnabled = true;
            }
            else
            {
                if (viewModel.EditLevel == null)
                {
                    world.Initialize(1);
                }
                else
                {
                    world.Initialize(viewModel.EditLevel.Number);
                }

                ennemySizeX = cellWidth * .75;
                ennemySizeY = cellHeight * .75;
                ennemyOffsetX = cellWidth * 0.125;
                ennemyOffsetY = cellHeight * 0.25;

                gameCanvas.Children.Clear();

                RenderWorldBackground();
                RenderWorld();

                viewModel.State = EngineState.Playing;
                startGameBtn.Content = "Stop";

                gameCanvas.Focus();
                levelComboBox.IsEnabled = false;
            }
        }

        private void GameTick(object sender, EventArgs e)
        {
            var action = keyboardAgent.Decide();
            ProcessAction(action);

            world.Step();

            RenderWorld();

            world.EncodeState();
        }

        private void ProcessAction(AgentAction action)
        {
            if (action == AgentAction.None)
                return;

            Debug.WriteLine($"=>ProcessAction {action}");

            world.Player.Dump(true);

            if (world.Player.IsFalling || world.Player.IsJumping)
            {
                if (action == AgentAction.Left)
                    world.Player.IsMovingRight = false;
                else if (action == AgentAction.Right)
                    world.Player.IsMovingRight = true;


                Debug.WriteLine($"!=ProcessAction {action}");

                world.Player.Dump(true);
                return;
            }

            switch (action)
            {
                case AgentAction.Left:
                    if (world.Player.IsMovingRight)
                    {
                        world.Player.IsMovingRight = false;
                    }
                    else
                    {
                        if (world.Player.CanMove(world.Player.X - 1, world.Player.Y))
                        {
                            world.Player.X--;
                        }
                    }
                    break;
                case AgentAction.Right:
                    if (!world.Player.IsMovingRight)
                    {
                        world.Player.IsMovingRight = true;
                    }
                    else
                    {
                        if (world.Player.CanMove(world.Player.X + 1, world.Player.Y))
                        {
                            world.Player.X++;
                        }
                    }
                    break;
                case AgentAction.Up:
                    if (world.Player.CanMoveUp(world.Player.X, world.Player.Y - 1))
                    {
                        world.Player.Y--;
                        world.Player.IsJumping = false;
                    }
                    else if (world.Player.CanMove(world.Player.X, world.Player.Y - 1))
                    {

                        world.Player.IsJumping = world.Background[world.Player.X, world.Player.Y] != Tiles.Ladder;
                        world.Player.Y--;
                    }
                    break;
                case AgentAction.Down:
                    if (world.Player.CanMoveDown(world.Player.X, world.Player.Y + 1))
                    {
                        world.Player.Y++;
                    }
                    break;
            }

            Debug.WriteLine($"<=ProcessAction {action}");

            world.Player.Dump(true);
        }

        Tiles? editorTile;
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as RadioButton;
            if (toggleButton == null || !toggleButton.IsChecked.Value)
            {
                return;
            }
            editorTile = (Tiles)toggleButton.Tag;
        }

        private void gameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel.EditLevel == null || editorTile == null)
                return;

            var pos = new Coord((int)(e.GetPosition(sender as Canvas).X / cellWidth), (int)(e.GetPosition(sender as Canvas).Y / cellHeight));

            switch (editorTile.Value)
            {
                case Tiles.Empty:
                case Tiles.FloorLeft:
                case Tiles.FloorRight:
                case Tiles.Ladder:
                case Tiles.Fire:
                    viewModel.EditLevel.LevelArray[pos.Y][pos.X] = editorTile.Value;
                    break;
                case Tiles.Ennemy:
                    viewModel.EditLevel.EnnemySource = pos;
                    break;
                case Tiles.Player:
                    viewModel.EditLevel.PlayerStartPos = pos;
                    break;
                case Tiles.Goal:
                    viewModel.EditLevel.GoalPos = pos;
                    break;
                default:
                    break;
            }

            world.Initialize(this.viewModel.EditLevel.Number);
            RenderWorldBackground();
            RenderWorld();
        }

        private void newLevelButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.EditLevel = new Level(new Coord(0, 9), new Coord(0, 9), new Coord(1, 0), 500, 10);
        }

        private void saveLevelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.EditLevel == null)
                return;

            this.viewModel.EditLevel.Serialize();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "EditLevel":
                    world.Initialize(this.viewModel.EditLevel.Number);

                    RenderWorldBackground();
                    RenderWorld();
                    break;
            }
        }
    }
}