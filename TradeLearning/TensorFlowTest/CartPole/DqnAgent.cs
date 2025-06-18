using NumSharp;
using Tensorflow;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

public class DqnAgent
{
    private int stateSize = 4;
    private int actionSize = 2;
    private float learningRate = 0.001f;

    private Tensor xInput, yTarget;
    private Tensor prediction, loss;
    private Operation trainOp;
    private Session sess;

    public DqnAgent()
    {
        tf.compat.v1.disable_eager_execution(); // Use graph mode

        var graph = tf.Graph().as_default();

        // Placeholders
        xInput = tf.placeholder(tf.float32, shape: (-1, stateSize), name: "x");
        yTarget = tf.placeholder(tf.float32, shape: (-1, actionSize), name: "y");

        // Network: 2 hidden layers
        var w1 = tf.Variable(tf.random.normal((stateSize, 24)), name: "w1");
        var b1 = tf.Variable(tf.zeros((24)), name: "b1");
        var h1 = tf.nn.relu(tf.matmul(xInput, w1) + b1);

        var w2 = tf.Variable(tf.random.normal((24, 24)), name: "w2");
        var b2 = tf.Variable(tf.zeros((24)), name: "b2");
        var h2 = tf.nn.relu(tf.matmul(h1, w2) + b2);

        var wOut = tf.Variable(tf.random.normal((24, actionSize)), name: "wOut");
        var bOut = tf.Variable(tf.zeros((actionSize)), name: "bOut");
        prediction = tf.matmul(h2, wOut) + bOut;

        // Loss and optimizer
        loss = tf.reduce_mean(tf.square(yTarget - prediction));
        var optimizer = tf.train.AdamOptimizer(learningRate);
        trainOp = optimizer.minimize(loss);

        // Session
        sess = tf.Session();
        sess.run(tf.global_variables_initializer());
    }

    public void Predict(NDArray state)
    {
        var result = sess.run(prediction, new FeedItem(xInput, state));
    }

    public void Train(NDArray xBatch, NDArray yBatch)
    {
        sess.run(trainOp, new FeedItem(xInput, xBatch), new FeedItem(yTarget, yBatch));
    }

    public void Other()
    {
        var layers = keras.layers;
        // input layer
        var inputs = keras.Input(shape: (32, 32, 3), name: "img");
        // convolutional layer
        var x = layers.Conv2D(32, 3, activation: "relu").Apply(inputs);
        x = layers.Conv2D(64, 3, activation: "relu").Apply(x);
        var block_1_output = layers.MaxPooling2D(3).Apply(x);
        x = layers.Conv2D(64, 3, activation: "relu", padding: "same").Apply(block_1_output);
        x = layers.Conv2D(64, 3, activation: "relu", padding: "same").Apply(x);
        var block_2_output = layers.Add().Apply(new Tensors(x, block_1_output));
        x = layers.Conv2D(64, 3, activation: "relu", padding: "same").Apply(block_2_output);
        x = layers.Conv2D(64, 3, activation: "relu", padding: "same").Apply(x);
        var block_3_output = layers.Add().Apply(new Tensors(x, block_2_output));
        x = layers.Conv2D(64, 3, activation: "relu").Apply(block_3_output);
        x = layers.GlobalAveragePooling2D().Apply(x);
        x = layers.Dense(256, activation: "relu").Apply(x);
        x = layers.Dropout(0.5f).Apply(x);
        // output layer
        var outputs = layers.Dense(10).Apply(x);
        // build keras model
        var model = keras.Model(inputs, outputs, name: "toy_resnet");
        model.summary();
        // compile keras model in tensorflow static graph
        model.compile(optimizer: keras.optimizers.RMSprop(1e-3f),
            loss: keras.losses.SparseCategoricalCrossentropy(from_logits: true),
            metrics: new[] { "acc" });
        // prepare dataset
        var ((x_train, y_train), (x_test, y_test)) = keras.datasets.cifar10.load_data();
        // normalize the input
        x_train = x_train / 255.0f;
        // training
        model.fit(x_train[new Tensorflow.Slice(0, 2000)], y_train[new Tensorflow.Slice(0, 2000)],
                    batch_size: 64,
                    epochs: 10,
                    validation_split: 0.2f);
        // save the model
        model.save("./toy_resnet_model");
    }
}
