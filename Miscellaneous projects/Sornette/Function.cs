using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sornette
{
    public delegate double Fnct(double x);
    public class Function
    {
        public Value[] Values { get; private set; }

        public string Name { get; private set; }

        static public Function CreateTrigo(string name, double start, double end, int nbValues, Fnct f, double ω, double φ)
        {
            double step = (end - start) / (double)(nbValues - 1);
            var function = new Function()
            {
                Values = new Value[nbValues],
                Name = name
            };

            double x = start;
            for (int i = 0; i < nbValues; i++)
            {
                function.Values[i] = new Value
                {
                    X = x,
                    Y = f(ω * x + φ)
                };
                x += step;
            }

            return function;
        }
        static public Function CreateSornette(double start, double end, int nbValues, double Tc, double ω, double φ, double A0, double gradient)
        {
            double step = (end - start) / (double)(nbValues - 1);
            var function = new Function()
            {
                Values = new Value[nbValues],
                Name = "Sornette"
            };

            double t = start;
            double val = A0;
            for (int i = 0; i < nbValues; i++)
            {
                val = A0 * Math.Pow(1 + gradient, t);
                function.Values[i] = new Value
                {
                    X = t,
                    Y = Math.Cos(ω * Math.Log(Tc - t) + φ) + val
                };
                t += step;
            }

            return function;
        }
    }
}
