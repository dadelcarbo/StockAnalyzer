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
   /// A layer of neurone in a neuronal network
   /// </summary>
   /// <remarks>
   /// <code>
   ///             / N1 ----->        OUTPUTS
   /// INPUTS ===> - N2 ----->  (1 output for each 
   ///             \ Ni ----->  neuron of the layer)
   /// </code>       
   /// Each neuron of the layer has the same number of
   /// inputs, this is the number of inputs of the layer
   /// itself.
   /// </remarks>
   /// 
   [Serializable]
   public class Layer
   {

      #region PROTECTED FIELDS (UpDownState of the layer)

      /// <summary>
      /// Number of neurons in the layer
      /// </summary>
      protected int nn;
      /// <summary>
      /// Number of inputs of the layer
      /// </summary>
      protected int ni;
      /// <summary>
      /// Neurons of the layer
      /// </summary>
      protected Neuron[] neurons;
      /// <summary>
      /// Last output of the layer
      /// </summary>
      protected float[] output;

      #endregion

      #region PUBLIC ACCES TO LAYER STATE

      /// <summary>
      /// Number of neurons in this layer
      /// </summary>
      public int N_Neurons
      {
         get { return nn; }
      }
      /// <summary>
      /// Number of inputs of this layer
      /// </summary>
      public int N_Inputs
      {
         get { return ni; }
      }
      /// <summary>
      /// Indexer of layer's neurons
      /// </summary>
      public Neuron this[int neurone]
      {
         get { return neurons[neurone]; }
      }
      /// <summary>
      /// Return last output vector of the layer
      /// </summary>
      public float[] Last_Output
      {
         get { return output; }
      }

      #endregion

      #region LAYER CONSTRUCTORS

      /// <summary>
      /// Build a new Layer with neurons neurones. Every neuron 
      /// has "inputs" inputs and the activation function f.
      /// </summary>
      /// <param name="inputs">Number of inputs</param>
      /// <param name="neurons">Number of neurons</param>
      /// <param name="f">Activation function of each neuron</param>
      public Layer(int neurons, int inputs, ActivationFunction f)
      {
         nn = neurons;
         ni = inputs;
         this.neurons = new Neuron[nn];
         output = new float[nn];
         for (int i = 0; i < neurons; i++)
            this.neurons[i] = new Neuron(inputs, f);
      }

      /// <summary>
      /// Build a new Layer with neurons neurones. Every neuron 
      /// has "inputs" inputs and the sigmoid activation function.
      /// </summary>
      /// <param name="inputs">Number of inputs</param>
      /// <param name="neurons">Number of neurons</param>
      public Layer(int neurons, int inputs)
      {
         nn = neurons;
         ni = inputs;
         this.neurons = new Neuron[nn];
         output = new float[nn];
         for (int i = 0; i < neurons; i++)
            this.neurons[i] = new Neuron(inputs);
      }

      /// <summary>
      /// Set the activation function f to all neurons of the layer
      /// </summary>
      /// <param name="f">An activation function</param>
      public void setActivationFunction(ActivationFunction f)
      {
         foreach (Neuron n in neurons)
            n.F = f;
      }

      #endregion

      #region INITIALIZATION FUNCTIONS

      /// <summary>
      /// Randomize all neurons weights
      /// </summary>
      public void randomizeWeight()
      {
         foreach (Neuron n in neurons)
            n.randomizeWeight();
      }
      /// <summary>
      /// Randomize all neurons thresholds
      /// </summary>
      public void randomizeThreshold()
      {
         foreach (Neuron n in neurons)
            n.randomizeThreshold();
      }
      /// <summary>
      /// Randomize all neurons threshold and weights
      /// </summary>
      public void randomizeAll()
      {
         randomizeWeight();
         randomizeThreshold();
      }
      /// <summary>
      /// Set the randomization interval for all neurons
      /// </summary>
      /// <param name="min">the minimum value</param>
      /// <param name="max">the maximum value</param>
      public void setRandomizationInterval(float min, float max)
      {
         foreach (Neuron n in neurons)
         {
            n.Randomization_Max = max;
            n.Randomization_Min = min;
         }
      }

      #endregion

      #region OUTPUT VALUE ACCES

      /// <summary>
      /// Compute output of the layer.
      /// The output vector contains the output of each 
      /// neuron of the layer.
      /// </summary>
      /// <param name="input">input of the layer (size must be N_inputs)</param>
      /// <returns>the output vector (size = N_neurons)</returns>
      public float[] Output(float[] input)
      {
         if (input.Length != ni)
            throw new Exception("LAYER : Wrong input vector size, unable to compute output value");
         for (int i = 0; i < nn; i++)
            output[i] = neurons[i].ComputeOutput(input);
         return output;
      }

      #endregion
   }
}
