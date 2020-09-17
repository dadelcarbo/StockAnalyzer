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

        static public Function CreateTrigo(string name, double start, double end, int nbValues, Fnct f, double omega, double phi)
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
                    Y = f(omega * x + phi)
                };
                x += step;
            }

            return function;
        }
        static public Function CreateSornette(double start, double end, int nbValues, double Tc, double omega, double phi)
        {
            double step = (end - start) / (double)(nbValues - 1);
            var function = new Function()
            {
                Values = new Value[nbValues],
                Name = "Sornette"
            };

            double x = start;
            for (int i = 0; i < nbValues; i++)
            {
                function.Values[i] = new Value
                {
                    X = x,
                    Y = Math.Cos(omega * x + phi)
                };
                x += step;
            }

            return function;
        }
    }
}
