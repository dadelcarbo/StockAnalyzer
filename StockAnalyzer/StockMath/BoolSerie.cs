using System.Linq;

namespace StockAnalyzer.StockMath
{
    public class BoolSerie
    {
        public string Name { get; set; }
        public bool[] Values { get; set; }

        public BoolSerie(int size, string name)
        {
            this.Values = new bool[size];
            this.Name = name;
        }

        public bool this[int index]
        {
            get { return this.Values[index]; }
            set { this.Values[index] = value; }
        }
        public int Count
        {
            get { return this.Values.Count(); }
        }
    }
}
