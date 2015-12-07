namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System;

    public class RandomGenerator
    {
        private static readonly Random random = new Random();

        public static int RandomInt(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        public static int RandomInt(int maxValue)
        {
            return random.Next(maxValue);
        }
    }
}
