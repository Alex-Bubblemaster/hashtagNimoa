namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;

    public static class CardsCombinations
    {
         // Source: http://rosettacode.org/wiki/Combinations
        // TODO: Just generate card indexes
        public static IEnumerable<Card[]> CombinationsNoRepetitionsIterative(List<Card> remainingCards, int variantCardsCount)
        {
            Card[] result = new Card[variantCardsCount];
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();

                while (value < remainingCards.Count)
                {
                    result[index++] = remainingCards[value++];
                    stack.Push(value);
                    if (index == variantCardsCount)
                    {
                        yield return result.ToArray();
                        break;
                    }
                }
            }
        }

        public static List<int[]> GetRandomCardsIndexes(int variants, int cardsCount, int maxIndex)
        {
            var combinations = new List<int[]>(variants);
            var uniqueCheck = new HashSet<string>();

            while (combinations.Count < variants)
            {
                var variant = new HashSet<int>();
                while (variant.Count < cardsCount)
                {
                    variant.Add(RandomGenerator.RandomInt(0, maxIndex + 1));
                }

                var countBeforeAdd = uniqueCheck.Count;
                string val = string.Join(", ", variant);
                uniqueCheck.Add(val);
                if (uniqueCheck.Count != countBeforeAdd)
                {
                    combinations.Add(variant.ToArray());
                }
            }

            return combinations;
        }
    }
}