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

            data[0] = level;
            for (int i = 1; i < nbValues; i++)
            {
                level += drift;
                data[i] = level + amplitude * Math.Sin(i * step);
            }

            return data;
        }
        public static double[] GeneratePeriodic(int nbValues, double period1, double amplitude1, double period2, double amplitude2, double level, double drift = 0)
        {
            double[] data = new double[nbValues];
            double step1 = (2 * Math.PI) / period1;
            double step2 = (2 * Math.PI) / period2;

            data[0] = level;
            for (int i = 1; i < nbValues; i++)
            {
                level += drift;
                data[i] = level + amplitude1 * Math.Sin(i * step1) + amplitude2 * Math.Sin(i * step2);
            }

            return data;
        }
        public static double[] GenerateRandom(int nbValues, double period, double amplitude, double level, double drift = 0)
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
