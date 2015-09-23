/*
 * NEURAL NETWORK Library
 * Version 0.1 (april 2002)
 * By Fleurey Franck (franck.fleurey@ifrance.com)
 * Distributed under GPL licence (see www.fsf.org)
 */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace StockNeuralNetwork
{
	/// <summary>
	/// Implementation of artificial neural network
	/// </summary>  
	/// <remarks>
	/// <code>
	/// 
	/// 
	///                        o
	///                        o  o  o  
	///    INPUT VECTOR =====> o  o  o =====> OUTPUT VECTOR
	///                        o  o  o  
	///                        o
	///                      NERON LAYERS
	/// 
	/// </code> 
	/// Each neuron of the layer N-1 is conected to 
	/// every neuron of the layer N.
	/// At the begining the neural network needs to
	/// learn using couples (INPUT, EXPECTED OUTPUT)
	/// and a learnig algorithm.
	/// </remarks>
	[Serializable]
	public class NeuralNetwork
	{

		#region PROTECTED FIELDS (STATE OF THE NETWORK)
		
		/// <summary>
		/// Layers of neuron in the network
		/// </summary>
		protected Layer[] layers;
		/// <summary>
		/// Number of inputs of the network
		/// (number of inputs of the first layer)
		/// </summary>
		protected int ni;
		/// <summary>
		/// Learning algorithm used by the network
		/// </summary>
		protected LearningAlgorithm la;

		#endregion

		#region PUBLIC ACCESS TO NETWORK STATE

		/// <summary>
		/// Get number of inputs of the network
		/// (network input vector size)
		/// </summary>
		public int N_Inputs 
		{
			get { return ni; }
		}
		/// <summary>
		/// Get number of output of the network
		/// (network output vector size)
		/// </summary>
		public int N_Outputs
		{
			get { return layers[N_Layers-1].N_Neurons; }
		}
		/// <summary>
		/// Get number of inputs of the network
		/// (network input vector size)
		/// </summary>
		public int N_Layers 
		{
			get { return layers.Length; }
		}
		/// <summary>
		/// Get or set network learning algorithm
		/// </summary>
		public LearningAlgorithm LearningAlg 
		{
			get { return la; }
			set { la = (value!=null)?value:la; }
		}
		/// <summary>
		/// Get the n th Layer of the network 
		/// </summary>
		public Layer this[int n] 
		{
			get { return layers[n]; }
		}

		#endregion

		#region NEURAL NETWORK CONSTRUCTOR

		/// <summary>
		/// Create a new neural network
		/// with "inputs" inputs and size of "layers"
		/// layers of neurones.
		/// The layer i is made with layers_desc[i] neurones.
		/// The activation function of each neuron is set to n_act.
		/// The lerning algorithm is set to learn.
		/// </summary>
		/// <param name="inputs">Number of inputs of the network</param>
		/// <param name="layers_desc">Number of neurons for each layer of the network</param>
		/// <param name="n_act">Activation function for each neuron in the network</param>
		/// <param name="learn">Learning algorithm to be used by the neural network</param>
		public NeuralNetwork(int inputs, int[] layers_desc, ActivationFunction n_act, LearningAlgorithm learn)
		{
			if (layers_desc.Length<1)
				throw new Exception("PERCEPTRON : cannot build perceptron, it must have at least 1 layer of neurone");
			if (inputs<1)
				throw new Exception("PERCEPTRON : cannot build perceptron, it must have at least 1 input");
			la = learn;
			ni = inputs;
			layers = new Layer[layers_desc.Length];
			layers[0] = new Layer(layers_desc[0], ni);
			for(int i=1; i<layers_desc.Length; i++) 
				layers[i] = new Layer(layers_desc[i],layers_desc[i-1],n_act);
		}
		/// <summary>
		/// Create a new neural network
		/// with "inputs" inputs and size of "layers"
		/// layers of neurones.
		/// The layer i is made with layers_desc[i] neurones.
		/// The activation function of each neuron is set to n_act.
		/// The lerning algorithm is set to default (Back Propagation).
		/// </summary>
		/// <param name="inputs">Number of inputs of the network</param>
		/// <param name="layers_desc">Number of neurons for each layer of the network</param>
		/// <param name="n_act">Activation function for each neuron in the network</param>
		public NeuralNetwork(int inputs, int[] layers_desc, ActivationFunction n_act)
		{
			if (layers_desc.Length<1)
				throw new Exception("PERCEPTRON : cannot build perceptron, it must have at least 1 layer of neurone");
			if (inputs<1)
				throw new Exception("PERCEPTRON : cannot build perceptron, it must have at least 1 input");
			la = new BackPropagationLearningAlgorithm(this);
			ni = inputs;
			layers = new Layer[layers_desc.Length];
			layers[0] = new Layer(layers_desc[0], ni);
			for(int i=1; i<layers_desc.Length; i++) 
				layers[i] = new Layer(layers_desc[i],layers_desc[i-1],n_act);
		}
		/// <summary>
		/// Create a new neural network
		/// with "inputs" inputs and size of "layers"
		/// layers of neurones.
		/// The layer i is made with layers_desc[i] neurones.
		/// The activation function of each neuron is set to default (Sigmoid with beta = 1).
		/// The lerning algorithm is set to default (Back Propagation).
		/// </summary>
		/// <param name="inputs">Number of inputs of the network</param>
		/// <param name="layers_desc">Number of neurons for each layer of the network</param>
		public NeuralNetwork(int inputs, int[] layers_desc)
		{
			if (layers_desc.Length<1)
				throw new Exception("PERCEPTRON : cannot build perceptron, it must have at least 1 layer of neurone");
			if (inputs<1)
				throw new Exception("PERCEPTRON : cannot build perceptron, it must have at least 1 input");
			la = new BackPropagationLearningAlgorithm(this);
			ni = inputs;
			ActivationFunction n_act = new SigmoidActivationFunction();
			layers = new Layer[layers_desc.Length];
			layers[0] = new Layer(layers_desc[0], ni);
			for(int i=1; i<layers_desc.Length; i++) 
				layers[i] = new Layer(layers_desc[i],layers_desc[i-1],n_act);
		}

		#endregion

		#region INITIALIZATION FUNCTIONS

		/// <summary>
		/// Randomize all neurones weights between -0.5 and 0.5
		/// </summary>
		public void randomizeWeight() 
		{
			foreach (Layer l in layers)
				l.randomizeWeight();
		}
		/// <summary>
		/// Randomize all neurones threholds between 0 and 1
		/// </summary>
		public void randomizeThreshold() 
		{
			foreach (Layer l in layers)
				l.randomizeThreshold();
		}
		/// <summary>
		/// Randomize all neurones threholds between 0 and 1
		/// and weights between -0.5 and 0.5
		/// </summary>
		public void randomizeAll() 
		{
			foreach (Layer l in layers)
				l.randomizeAll();
		}
		/// <summary>
		/// Set an activation function to all neurons of the network
		/// </summary>
		/// <param name="f">An activation function</param>
		public void setActivationFunction(ActivationFunction f) 
		{
			foreach(Layer l in layers)
				l.setActivationFunction(f);
		}
		/// <summary>
		/// Set the interval in which weights and threshold will be randomized
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public void setRandomizationInterval(float min, float max) 
		{
			foreach (Layer l in layers)
				l.setRandomizationInterval(min, max);
		}

		#endregion

		#region OUPUT METHODS

		/// <summary>
		/// Compute the value for the specified input
		/// </summary>
		/// <param name="input">the input vector</param>
		/// <returns>the output vector of the neuronal network</returns>
		public float[] Output(float[] input) 
		{
			if (input.Length != ni)
				throw new Exception("PERCEPTRON : Wrong input vector size, unable to compute output value");
			float[] result;
			result = layers[0].Output(input);
			for(int i=1; i<N_Layers; i++)
				result = layers[i].Output(result);
			return result;
		}

		

		#endregion

		#region PERSISTANCE IMPLEMENTATION
		/// <summary>
		/// Save the Neural Network in a binary formated file
		/// </summary>
		/// <param name="file">the target file path</param>
		public void save(string file) 
		{
			IFormatter binFmt = new BinaryFormatter();
			Stream s = File.Open(file, FileMode.Create);     
			binFmt.Serialize(s, this);
			s.Close();
		}
		/// <summary>
		/// Load a neural network from a binary formated file
		/// </summary>
		/// <param name="file">the neural network file file</param>
		/// <returns></returns>
		public static NeuralNetwork load(string file) 
		{
			NeuralNetwork result;
			try 
			{
				IFormatter binFmt = new BinaryFormatter();
				Stream s = File.Open(file, FileMode.Open); 
				result = (NeuralNetwork)binFmt.Deserialize(s);
				s.Close();
			}
			catch(Exception e)
			{
				throw new Exception("NeuralNetwork : Unable to load file "+file +" : "+e);
			}
			return result;
		}
		#endregion
	}
}
