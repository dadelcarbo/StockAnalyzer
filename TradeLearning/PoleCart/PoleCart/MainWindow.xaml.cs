using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PoleCart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        Cart cart = new Cart();
        public MainWindow()
        {
            InitializeComponent();

            // timer.Tick += Timer_Tick;

            startButton_Click(null, null);

            CompositionTarget.Rendering += CompositionTarget_Rendering; ;
        }


        double canvasCenter;
        private TranslateTransform cartTransform = new TranslateTransform();
        private void mainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus(); // Ensure the window has keyboard focus

            double canvasWidth = mainCanvas.ActualWidth;
            double cartWidth = 40;

            // Center horizontally and position vertically above the ground
            canvasCenter = (canvasWidth - cartWidth) / 2;
            cartTransform.X = canvasCenter;
            cartTransform.Y = 280; // Adjust based on your layout

            cartPoleCanvas.RenderTransform = cartTransform;

            cart.XMin = -canvasCenter;
            cart.XMax = canvasCenter;

        }

        double dt = 0.01; // in seconds

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                startButton.Content = "Start";
            }
            else
            {
                cart.Reset();

                timer.Interval = TimeSpan.FromSeconds(dt);
                timer.Start();
                startButton.Content = "Stop";

                lastUpdate = DateTime.Now;
            }
        }

        private DateTime lastUpdate;
        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (!timer.IsEnabled)
                return;
            var now = DateTime.Now;
            double dt = (now - lastUpdate).TotalSeconds;

            double force = 0;
            if (Keyboard.IsKeyDown(Key.Left)) force = -100;
            if (Keyboard.IsKeyDown(Key.Right)) force = 100;

            cart.Step(force, dt);

            cartTransform.X = canvasCenter + cart.X;
            PoleRotate.Angle = cart.θ * 180 / Math.PI;

            //Debug.WriteLine($"X:{cart.X.ToString("F3")} dX:{cart.dx.ToString("F3")} Theta:{cart.θ.ToString("F3")} Angle:{(int)PoleRotate.Angle} Theta:{cart.θ.ToString("F3")}");

            lastUpdate = now;
        }
    }
}