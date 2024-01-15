using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public static class StaticRandom
    {
        private static Random _random = new Random();

        public static double NextDouble()
        {
            return _random.NextDouble();
        }
    }

}
