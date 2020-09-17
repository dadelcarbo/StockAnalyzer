using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sornette
{
    public class ViewModel : INotifyPropertyChanged
    {
        private double min;
        public double Min
        {
            get { return min; }
            set
            {
                if (value != min)
                {
                    min = value;
                    this.OnPropertyChanged("Min");
                    Calculate();
                }
            }
        }

        private double max;
        public double Max
        {
            get { return max; }
            set
            {
                if (value != max)
                {
                    max = value;
                    this.OnPropertyChanged("Max");
                    Calculate();
                }
            }
        }

        private double omega;

        public double ω
        {
            get { return omega; }
            set
            {
                if (value != omega)
                {
                    omega = value;
                    this.OnPropertyChanged("ω");
                    Calculate();
                }
            }
        }

        private double phi;

        public double φ
        {
            get { return phi; }
            set
            {
                if (value != phi)
                {
                    phi = value;
                    this.OnPropertyChanged("φ");
                    Calculate();
                }
            }
        }


        public ViewModel()
        {
            this.min = 0;
            this.max = Math.PI * 2;
            this.omega = 1;
            this.phi = 0;
            Calculate();
        }

        private void Calculate()
        {
            this.F1 = Function.CreateTrigo("Cos", min, max, 500, Math.Cos, ω, φ);
            this.F2 = Function.CreateTrigo("Sin", min, max, 500, Math.Sin, ω, φ);

            this.OnPropertyChanged("F1");
            this.OnPropertyChanged("F2");
        }

        public Function F1 { get; set; }
        public Function F2 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
