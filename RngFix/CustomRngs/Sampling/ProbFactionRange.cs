using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RngFix.CustomRngs.Sampling
{
    public class ProbFactionRange
    {

        float[] ranges;

        List<Func<float, float>> fractionReductors = new List<Func<float, float>>() { w => w / 10f, w => w / 100f};

        public ProbFactionRange(IEnumerable<float> ranges)
        {
            this.ranges = ranges.ToArray<float>();
        }

        public List<Func<float, float>> WeightReductors { get => fractionReductors; set => fractionReductors = value; }

        public (float, string) GetFraction(float maxW, bool logWarning = true)
        {
            for(int i = 0; i < ranges.Length; i++) 
            {
                var r = ranges[i];
                if (maxW <= r)
                {
                    var warning = "";
                    if (i > 0)
                        warning = "Not using the preferred probability fraction range;";

                    float reducedR = r;
                    foreach (var fr in fractionReductors)
                    {
                        float red = fr(r);
                        if (red >= maxW && red < r)
                        {
                            reducedR = Math.Min(red, reducedR);
                        }
                    }
                    if(reducedR < r)
                    {
                        warning += $"Using fraction reduction r={reducedR};";
                        r = reducedR;
                    }

                    return (r, warning);
                }
            }
            return (maxW, "None of the probability ranges fitted");
        }


    }
}
