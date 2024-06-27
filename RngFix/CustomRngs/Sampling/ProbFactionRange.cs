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

        public ProbFactionRange(IEnumerable<float> ranges)
        {
            this.ranges = ranges.ToArray<float>();
        }

        public (float, string) GetFraction(float maxW, bool logWarning = true)
        {
            for(int i = 0; i < ranges.Length; i++) 
            {
                var r = ranges[i];
                if (maxW <= r)
                {
                    var warning = "";
                    if (i > 0)
                        warning = "Not using the preferred probability fraction range";
                    return (r, warning);
                }
            }
            return (maxW, "None of the probability ranges fitted");
        }
    }
}
