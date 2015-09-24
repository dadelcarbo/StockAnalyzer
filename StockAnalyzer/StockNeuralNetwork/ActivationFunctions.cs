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
   /// Interface of the activation function of a neuron.
   /// </summary>
   public interface ActivationFunction
   {
      /// <summary>
      /// Compute function value
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f(x)</returns>
      float Output(float x);
      /// <summary>
      /// Compute the diff of the function
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f'(x)</returns>
      float OutputPrime(float x);
   }

   #region SIGMOID ACTIVATION FUNCTION
   /// <summary>
   /// The sigmoid activation function
   /// </summary>
   /// <remarks>
   /// Here is the definition of the sigmoid activation function
   /// <code>
   ///                1
   /// f(x) = -----------------   beta > 0
   ///         1 + e^(-beta*x)
   /// 
   /// f'(x) = beta * f(x) * ( 1 - f(x) )   
   /// </code>     
   /// </remarks>
   [Serializable]
   public class SigmoidActivationFunction : ActivationFunction
   {
      /// <summary>
      /// The beta parameter of the sigmoid
      /// </summary>
      protected float beta = 1.0f;

      /// <summary>
      /// Get or set the beta parameter of the function
      /// ( beta must be positive )
      /// </summary>
      public float Beta
      {
         get { return beta; }
         set { beta = (value > 0) ? value : 1.0f; }
      }
      /// <summary>
      /// Get the name of the activation function
      /// </summary>
      public string Name
      {
         get { return "Sigmoid"; }
      }
      /// <summary>
      /// <code>
      ///                 1
      /// f(x) = -----------------   beta > 0
      ///         1 + e^(-beta*x)
      /// </code>
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f(x)</returns>
      public virtual float Output(float x)
      {
         return (float)(1 / (1 + Math.Exp(-beta * x)));
      }
      /// <summary>
      /// <code>
      /// f'(x) = beta * f(x) * ( 1 - f(x) )
      /// </code>
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f'(x)</returns>
      public virtual float OutputPrime(float x)
      {
         float y = Output(x);
         return (beta * y * (1 - y));
      }
   }
   #endregion

   #region LINEAR ACTIVATION FUNCTION
   /// <summary>
   /// The linear activation function
   /// </summary>
   /// <remarks>
   /// <code>
   ///        |1            if x > 0.5/A
   /// f(x) = |A * x + 0.5  if 0.5/A > x > -0.5/A
   ///        |0            if -0.5/A > x
   /// 
   ///             A > 0      
   /// </code>
   /// </remarks>
   [Serializable]
   public class LinearActivationFunction : ActivationFunction
   {
      /// <summary>
      /// The A parameter of the linear function
      /// </summary>
      protected float a = 1.0f;
      /// <summary>
      /// Usefull to compute function value
      /// </summary>
      protected float threshold = 0.5f;

      /// <summary>
      /// Get or set the A parameter of the function
      /// ( A must be positive )
      /// </summary>
      public float A
      {
         get { return a; }
         set
         {
            a = (value > 0) ? value : 1.0f;
            threshold = 0.5f / a;
         }
      }
      /// <summary>
      /// Get the name of the activation function
      /// </summary>
      public string Name
      {
         get { return "Linear"; }
      }
      /// <summary>
      ///  Get the activation function value
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f(x)</returns>
      public virtual float Output(float x)
      {
         if (x > threshold) return 1;
         else if (x < -threshold) return 0;
         else return a * x + 0.5f;
      }
      /// <summary>
      /// Get the diff function value
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f'(x)</returns>
      public virtual float OutputPrime(float x)
      {
         if (x > threshold) return 0;
         else if (x < -threshold) return 0;
         else return a;
      }
   }
   #endregion

   #region HEAVISIDE ACTIVATION FUNCTION
   /// <summary>
   /// The heaviside activation function
   /// </summary>
   /// <remarks>
   /// <code>
   /// f(x) = 0 if 0>x
   /// f(x) = 1 if x>0
   /// </code>   
   /// </remarks>
   [Serializable]
   public class HeavisideActivationFunction : ActivationFunction
   {
      /// <summary>
      /// Get the name of the activation function
      /// </summary>
      public string Name
      {
         get { return "Heaviside"; }
      }
      /// <summary>
      ///  Get the heaviside function value
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f(x)</returns>
      public virtual float Output(float x)
      {
         if (x > 0) return 1;
         else return 0;
      }
      /// <summary>
      /// Get the derivative function value
      /// Simulate an impulse at origin...
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f'(x)</returns>
      public virtual float OutputPrime(float x)
      {
         if (Math.Abs(x) < 0.0001) return float.MaxValue;
         else return 0;
      }
   }
   #endregion

   #region GAUSSIAN ACTIVATION FUNCTION
   /// <summary>
   /// The gaussian activation function
   /// </summary>
   /// <remarks>
   /// <code>
   /// 
   ///                  1                -(x-mu)^2 / (2 * sigma^2)
   /// f(x)  =  -------------------- *  e
   ///          sqrt(2 * pi * sigma)
   /// 
   /// f'(x)  =  y(x) * -2*K*(x - mu) 
   /// </code>
   /// To implement a more efficient computation :
   /// <code>
   /// C = 1/sqrt(2 * pi * sigma)
   /// K = 1/(2 * sigma^2)
   /// </code>
   /// </remarks>
   [Serializable]
   public class GaussianActivationFunction : ActivationFunction
   {
      /// <summary>
      /// The sigma parameter of the gaussian
      /// </summary>
      protected float sigma = 0.159155f;
      /// <summary>
      /// The mu parameter of the gaussian
      /// </summary>
      protected float mu = 0f;
      /// <summary>
      /// C parameter (usfull for computing function value)
      /// </summary>
      protected float C;
      /// <summary>
      /// C parameter (usfull for computing function value)
      /// </summary>
      protected float K;

      /// <summary>
      /// Get or set the sigma parameter of the function
      /// (sigma must be positive)
      /// </summary>
      public float Sigma
      {
         get { return sigma; }
         set
         {
            sigma = (value > 0) ? value : sigma;
            computeCK();
         }
      }
      /// <summary>
      /// Get or set the mu parameter of the function
      /// </summary>
      public float Mu
      {
         get { return mu; }
         set { mu = value; }
      }
      /// <summary>
      /// Compute C and K parameters from sigma
      /// </summary>
      protected void computeCK()
      {
         C = 1f / (float)Math.Sqrt(2 * Math.PI * sigma);
         K = 1 / (2 * sigma * sigma);
      }
      /// <summary>
      /// GaussianActivationFunction constructor
      /// </summary>
      public GaussianActivationFunction()
      {
         computeCK();
      }
      /// <summary>
      /// Get the name of the activation function
      /// </summary>
      public string Name
      {
         get { return "Gaussian"; }
      }
      /// <summary>
      /// Compute the value of the gaussian function
      /// <param name="x">x</param>
      /// <returns>f(x)</returns>
      /// </summary>
      public virtual float Output(float x)
      {
         return C * (float)Math.Exp(-(x - mu) * (x - mu) * K);
      }
      /// <summary>
      /// compute the derivative value of function
      /// </summary>
      /// <param name="x">x</param>
      /// <returns>f'(x)</returns>
      public virtual float OutputPrime(float x)
      {
         float y = Output(x);
         return -2 * y * K * (x - mu);
      }
   }
   #endregion

}