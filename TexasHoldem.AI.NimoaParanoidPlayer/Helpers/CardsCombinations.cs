namespace TexasHoldem.AI.NimoaParanoidPlayer.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;

    public static class CardsCombinations
    {
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
    }
}