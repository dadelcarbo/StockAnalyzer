using System;
using System.Linq;

namespace TradeLearning.Model
{
    public class DataSerie
    {
        public double[] Data { get; set; }
        public DataPoint[] DataPoints { get; set; }
        public string Name { get; set; }

        public static double[] GenerateSin(int nbValues, double period, double amplitude, double level, double drift = 0)
        {
            double[] data = new double[nbValues];
            double step = (2 * Math.PI) / period;

            for (int i = 0; i < nbValues; i++)
            {
                data[i] = level + amplitude * Math.Sin(i * step);
                level += drift;
            }

            return data;
        }

        public static DataSerie FromArray(double[] data, string name)
        {
            int index = 0;
            return new DataSerie { Name = name, Data = data, DataPoints = data.Select(d => new DataPoint { X = index++, Y = d }).ToArray() };
        }
    }
    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

}
