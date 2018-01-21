using StockAnalyzer.StockMath;
using System.Collections.Generic;

namespace StockAnalyzer.StockDrawing
{
    public class StockHarmonicPatternRatio
    {
        public string Name { get; set; }
        public FloatRange XD { get; set; }
        public FloatRange AB { get; set; }
        public FloatRange AC { get; set; }
        public FloatRange BD { get; set; }

        public static string MatchPattern(XABCD pattern)
        {
            if (!pattern.IsComplete) return null;
            foreach (var p in HarmonicPatterns)
            {
                if (p.Match(pattern))
                {
                    var name = pattern.IsBullish ? "Bullish " : "Bearish ";
                    return name + p.Name;
                }
            }
            return null;
        }

        bool Match(XABCD pattern)
        {
            return (
                (XD == null || XD.IsInside(pattern.XD)) &&
                (AC == null || AC.IsInside(pattern.AC)) &&
                (BD == null || BD.IsInside(pattern.BD)) &&
                (AB == null || AB.IsInside(pattern.AB))
                );
        }

        const float accuracy = 0.1f;
        static public List<StockHarmonicPatternRatio> HarmonicPatterns = new List<StockHarmonicPatternRatio> {
            new StockHarmonicPatternRatio { Name = "Shark",
                AC = new FloatRange(1.172f,1.618f),
                BD = new FloatRange(1.618f,2.24f),
                AB = new FloatRange(0.236f, 0.786f)
            },
            new StockHarmonicPatternRatio { Name = "Gartley",
                AC = new FloatRange(0.382f, 0.886f),
                BD = new FloatRange(1.272f, 1.618f),
                AB = new FloatRange(0.5f, 0.786f)
            },
            new StockHarmonicPatternRatio { Name = "Crab",
                AC = new FloatRange(0.382f, 0.886f),
                BD = new FloatRange(2.24f,3.618f),
                AB = new FloatRange(0.382f, 0.618f)
            },
            new StockHarmonicPatternRatio { Name = "Bat",
                AC = new FloatRange(0.382f, 0.886f),
                BD = new FloatRange(1.618f,2.618f),
                AB = new FloatRange(0.382f, 0.5f)
            }
            //new HarmonicPatternRatio { Name = "Gartley", AB = new FloatRange(0.5f,0.62f), BC = new FloatRange(0.62f,0.886f), CD = FloatRange.FromAccuracy(0.786f,accuracy)},
            //new HarmonicPatternRatio { Name = "Gartley", AB = new FloatRange(0.5f,0.62f), BC = new FloatRange(0.5f,0.62f), CD = new FloatRange(0.5f,0.62f)}
        };
    }
}
