namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;

    public static class CardsCombinations
    {
        private static Random random = new Random();

        public static void CombinationsNoRepetitions(int index, int start, List<Card> remainingCards, int variantCardsCount, List<Card[]> result, Card[] currentCombination)
        {
            //var arr = new Card[variantCardsCount];
            if (index >= variantCardsCount)
            {
                result.Add(currentCombination);
                //currentCombination = new Card[variantCardsCount];
                return;
            }
            else
            {
                for (int i = start; i < remainingCards.Count; i++)
                {
                    currentCombination[index] = remainingCards[i];
                    CombinationsNoRepetitions(index + 1, i + 1, remainingCards, variantCardsCount, result, currentCombination.ToArray());
                }
            }
        }

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

        public static ICollection<HashSet<int>> GetRandomCardsIndexes(int variants, int cardsCount, int maxIndex)
        {
            var combinations = new HashSet<HashSet<int>>();

            while (combinations.Count < variants)
            {
                var variant = new HashSet<int>();
                while (variant.Count < cardsCount)
                {
                    variant.Add(random.Next(0, maxIndex + 1));
                }

                combinations.Add(variant);
            }

            return combinations;
        }
    }
}