using System;
using System.Collections.Generic;
using System.Text;

namespace VariantsC
{
    public static class Helpers
    {
        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }
    }
}
