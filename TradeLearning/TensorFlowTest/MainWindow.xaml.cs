using NumSharp;
using System;
using System.Diagnostics;
using System.Windows;
using static Tensorflow.Binding;



namespace TensorFlowTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var hello = tf.constant("Hello, TensorFlow!");
            Debug.WriteLine(hello);

            // Initialize TensorFlow
            tf.enable_eager_execution();

            // Create two tensors
            var a = tf.constant(3.0f);
            var b = tf.constant(4.0f);

            // Add them
            var c = tf.add(a, b);

            // Print the result
            Console.WriteLine($"Result of 3 + 4 = {c.numpy()}");

            var dqn = new DqnAgent();

            var state = np.arange(0, 10);

            dqn.Other();

            dqn.Predict(state);

        }
    }
}
