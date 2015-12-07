namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System;

    public class RandomGenerator
    {
        private static readonly Random Random = new Random();

        public static int RandomInt(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        public static int RandomInt(int maxValue)
        {
            return Random.Next(maxValue);
        }
    }
}
