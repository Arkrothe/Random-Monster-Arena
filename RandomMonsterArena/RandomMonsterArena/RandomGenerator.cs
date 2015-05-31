using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RandomMonsterArena
{
    public static class RandomGenerator
    {        
        /// <summary>
        /// random class object used to generate all the random numbers required for the game.
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Returns the random class object.
        /// </summary>
        public static Random Random
        {
            get { return rand; }
        }

        public static int signRandom()
        {
            int x = Random.Next() & 1;
            return x==1 ? 1 : -1;
        }

        public static float RandomValue(float min, float max)
        {
            max = max - min;
            return min + max * (float)Random.NextDouble();
        }

        /*
        public static float LineRandomValue(float range)
        {
            float area = range * range / 2;
            float p = area * (float)Random.NextDouble();
            return range - (float)Math.Sqrt(range * range - 2 * p);
        }

        public static float HatRandomValue(float range)
        {
            float area = 4 * (float)Math.Atan(6.0);
            float p = area * (float)Random.NextDouble();
            return (float)Math.Tan(p / 4) * range / 6;
        }
        /*
        public static string RandomName(List<string> prefixList, int nameId)
        {
            string newString = String.Concat(prefixList[Random.Next(prefixList.Count)], nameId.ToString());
            return newString;
        }
        */    
    }
}
