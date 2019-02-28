using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Botwoon.Data
{
    public sealed class Randomizer
    {
        private static readonly Randomizer instance = new Randomizer();
        private readonly Random random = new Random();

        private Randomizer()
        {   
        }

        public static Randomizer GetRandomizer()
        {
            return instance;
        }

        public void RandomizeList(ref List<string> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                string tmp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = tmp;
            }
        }

        public int GetRandomNumber(int upperBound)
        {
            return random.Next(upperBound + 1);
        }

        public int GetRandomNumber(int lowerBound, int upperBound)
        {
            return GetRandomNumber(upperBound - lowerBound) + lowerBound;
        }

        public TimeSpan GetRandomTime(int maxSeconds)
        {
            return new TimeSpan(0, 0, GetRandomNumber(maxSeconds));
        }

    }
}
