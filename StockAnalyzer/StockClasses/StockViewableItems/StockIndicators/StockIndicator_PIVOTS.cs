using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_PIVOTS : StockIndicatorBase
    {
        public StockIndicator_PIVOTS()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Name
        {
            get { return "PIVOTS(" + this.Parameters[0].ToString() + ")"; }
        }

        public override string Definition
        {
            get { return "PIVOTS(int Period)"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500)}; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override string[] SerieNames { get { return new string[] { "P", "S1", "S2", "S3", "R1", "R2", "R3" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get { return null; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie H = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie L = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie C = stockSerie.GetSerie(StockDataType.CLOSE);

            FloatSerie P = new FloatSerie(H.Count + 1);
            FloatSerie S1 = new FloatSerie(H.Count + 1);
            FloatSerie S2 = new FloatSerie(H.Count + 1);
            FloatSerie S3 = new FloatSerie(H.Count + 1);
            FloatSerie R1 = new FloatSerie(H.Count + 1);
            FloatSerie R2 = new FloatSerie(H.Count + 1);
            FloatSerie R3 = new FloatSerie(H.Count + 1);

            for (int i = 2; i <= H.Count; i++)
            {
                P[i] = (L[i - 2] + H[i - 2] + C[i - 1]) / 3.0f;
                S1[i] = ( P[i] * 2 ) - H[i-1];
                S2[i] = P[i] - ( H[i-1] - L[i-1] );
                S3[i] = L[i - 1] - 2 * (H[i - 1] - P[i]);

                R1[i] = ( P[i] * 2 ) - L[i - 1];
                R2[i] = P[i] + ( H[i - 1] - L[i - 1] );
                R3[i] = H[i - 1] + 2 * (P[i] - L[i - 1]);
            }

            this.series[0] = P;
            this.series[0].Name = "P";

            this.series[1] = S1;
            this.series[1].Name = "S1";
            this.series[2] = S2;
            this.series[2].Name = "S2";
            this.series[3] = S3;
            this.series[3].Name = "S3";

            this.series[4] = R1;
            this.series[4].Name = "R1";
            this.series[5] = R2;
            this.series[5].Name = "R2";
            this.series[6] = R3;
            this.series[6].Name = "R3";

        }

        static string[] eventNames = new string[] { "Bottom", "Top" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
