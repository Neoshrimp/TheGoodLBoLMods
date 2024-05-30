using LBoL.Base;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace RngFix.CustomRngs
{
    public abstract class MasterRng
    {
        public RandomGen rng;

        public MasterRng() {}

        public MasterRng(ulong seed) : this()
        {
            this.rng = new RandomGen(seed);
        }

        public abstract void Advance(GameRunController gr);

        public void AdvanceSteps(GameRunController gr, int steps)
        {
            for (int i = 0; i < steps; i++)
                Advance(gr);
        }
    }
  
}
