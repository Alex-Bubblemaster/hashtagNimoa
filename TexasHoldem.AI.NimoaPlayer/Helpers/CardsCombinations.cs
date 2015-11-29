namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System.Collections.Generic;

    using TexasHoldem.Logic.Cards;

    public static class CardsCombinations
    {
        public static void CombinationsNoRepetitions(int index, int start, List<Card[]> enemyCards, List<Card> remainingCards, int variantCardsCount)
        {
            if (index >= variantCardsCount)
            {
                return;
            }
            else
            {
                var arr = new Card[variantCardsCount];
                for (int i = start; i < remainingCards.Count; i++)
                {
                    arr[index] = remainingCards[i];
                    CombinationsNoRepetitions(index + 1, i + 1, enemyCards, remainingCards, variantCardsCount);
                }
            }
        }

        // Source: http://rosettacode.org/wiki/Combinations
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
                        yield return result;
                        break;
                    }
                }
            }
        }
    }
}