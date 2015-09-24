/*
 * NEURAL NETWORK Library
 * Version 0.1 (april 2002)
 * By Fleurey Franck (franck.fleurey@ifrance.com)
 * Distributed under GPL licence (see www.fsf.org)
 */

using System;

namespace StockNeuralNetwork
{
   /// <summary>
   /// Class representing an artificial neuron
   /// </summary>
   /// <remarks>
   /// <code>
   ///  
   ///  --------------> * W[0] \                              -----  
   ///  --------------> * W[1] - + -------> -threshold -------| f | ---------> O
   ///  --------------> * W[i] /                              -----
   ///     SYNAPSES      WEIGHT             THRESHOLD       ACTIVATION       OUTPUT
   ///
   /// </code>
   ///</remarks>
   [Serializable]
   public class Neuron
   {


      #region PROTECTED FIELDS (UpDownState variables)

      /// <summary>
      /// Pseudo random number generator to initialize neuron weight
      /// </summary>
      protected static Random rand = new Random();
      /// <summary>
      /// Minimum value for randomisation of weights and threshold
      /// </summary>
      protected float R_MIN = -1;
      /// <summary>
      /// Maximum value for randomization of weights and threshold
      /// </summary>
      protected float R_MAX = 1;
      /// <summary>
      /// Weight of every synapse
      /// </summary>
      protected float[] w;
      /// <summary>
      /// Last weight of every synapse
      /// </summary>
      protected float[] last_w;
      /// <summary>
      /// Threshold of the neuron
      /// </summary>
      protected float threshold = 0f;
      /// <summary>
      /// Last threshold of the neuron
      /// </summary>
      protected float last_threshold = 0f;
      /// <summary>
      /// Activation function of the neuron
      /// </summary>
      protected ActivationFunction f = null;
      /// <summary>
      /// Value of the last neuron ouput
      /// </summary>
      protected float o = 0f;
      /// <summary>
      /// Last value of synapse sum minus threshold
      /// </summary>
      protected float ws = 0f;
      /// <summary>
      /// Usefull for backpropagation algorithm
      /// </summary>
      protected float a;

      #endregion

      #region PUBLIC ACCES TO STATE OF THE NEURON

      /// <summary>
      ///  Number of neuron inputs (synapses)
      /// </summary>
      public int N_Inputs
      {
         get { return w.Length; }
      }
      /// <summary>
      /// Indexer of the neuron to get or set weight of synapses
      /// </summary>
      public float this[int synapse]
      {
         get { return w[synapse]; }
         set { last_w[synapse] = w[synapse]; w[synapse] = value; }
      }
      /// <summary>
      /// To get or set the threshold value of the neuron
      /// </summary>
      public float Threshold
      {
         get { return threshold; }
         set { last_threshold = threshold; threshold = value; }
      }
      /// <summary>
      /// Get the last output of the neuron
      /// </summary>
      public float Output
      {
         get { return o; }
      }
      /// <summary>
      /// Get the last output prime of the neuron (f'(ws))
      /// </summary>
      public float OutputPrime
      {
         get { return f.OutputPrime(ws); }
      }
      /// <summary>
      /// Get the last sum of inputs
      /// </summary>
      public float WS
      {
         get { return ws; }
      }
      /// <summary>
      /// Get or set the neuron activation function
      /// </summary>
      public ActivationFunction F
      {
         get { return f; }
         set { f = value; }
      }
      /// <summary>
      /// Get or set a value of the neuron
      /// (usefull for backpropagation learning algorithm)
      /// </summary>
      public float A
      {
         get { return a; }
         set { a = value; }
      }
      /// <summary>
      /// Get the last threshold value of the neuron
      /// </summary>
      public float Last_Threshold
      {
         get { return last_threshold; }
      }
      /// <summary>
      /// Get the last weights of the neuron
      /// </summary>
      public float[] Last_W
      {
         get { return last_w; }
      }
      /// <summary>
      /// Get or set the minimum value for randomisation of weights and threshold
      /// </summary>
      public float Randomization_Min
      {
         get { return R_MIN; }
         set { R_MIN = value; }
      }
      /// <summary>
      /// Get or set the maximum value for randomization of weights and threshold
      /// </summary>
      public float Randomization_Max
      {
         get { return R_MAX; }
         set { R_MAX = value; }
      }

      #endregion

      #region NEURON CONSTRUCTOR

      /// <summary>
      /// Build a neurone with Ni inputs
      /// </summary>
      /// <param name="Ni">number of inputs</param>
      /// <param name="af">The activation function of the neuron</param>
      public Neuron(int Ni, ActivationFunction af)
      {
         w = new float[Ni];
         last_w = new float[Ni];
         f = af;
      }
      /// <summary>
      /// Build a neurone with Ni inputs whith a default 
      /// activation function (SIGMOID)
      /// </summary>
      /// <param name="Ni">number of inputs</param>
      public Neuron(int Ni)
      {
         w = new float[Ni];
         last_w = new float[Ni];
         f = new SigmoidActivationFunction();
      }

      #endregion

      #region PUBLIC METHODS (INITIALIZATION FUNCTIONS)

      /// <summary>
      /// Randomize Weight for each input between R_MIN and R_MAX
      /// </summary>
      public void randomizeWeight()
      {
         for (int i = 0; i < N_Inputs; i++)
         {
            w[i] = R_MIN + (((float)(rand.Next(1000))) / 1000f) * (R_MAX - R_MIN);
            last_w[i] = 0f;
         }
      }
      /// <summary>
      /// Randomize the threshold (between R_MIN and R_MAX)
      /// </summary>
      public void randomizeThreshold()
      {
         threshold = R_MIN + (((float)(rand.Next(1000))) / 1000f) * (R_MAX - R_MIN);
      }
      /// <summary>
      /// Randomize the threshold and the weights
      /// </summary>
      public void randomizeAll()
      {
         randomizeWeight();
         randomizeThreshold();
      }

      #endregion

      #region PUBLIC METHODS (COMPUTE THE OUTPUT VALUE)

      /// <summary>
      /// Compute the output of the neurone
      /// </summary>
      /// <param name="input">The input vector</param>
      /// <returns>The output value of the neuron ( =f(ws) )</returns>
      public float ComputeOutput(float[] input)
      {
         if (input.Length != N_Inputs)
            throw new Exception("NEURONE : Wrong input vector size, unable to compute output value");
         ws = 0;
         for (int i = 0; i < N_Inputs; i++)
            ws += w[i] * input[i];
         ws -= threshold;
         if (f != null)
            o = f.Output(ws);
         else
            o = ws;
         return o;
      }
      #endregion
   }
}
