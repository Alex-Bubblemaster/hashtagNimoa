using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Helpers;

    public static class HandStrengthValuation
    {
        private const int MaxCardTypeValue = 14;

        private static readonly int[,] StartingHandRecommendationsSuited =
            {
                { 3, 3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1 }, // A
                { 0, 3, 3, 3, 3, 2, 1, 1, 1, 1, 1, 1, 1 }, // K
                { 0, 0, 3, 3, 3, 2, 2, 0, 0, 0, 0, 0, 0 }, // Q
                { 0, 0, 0, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0 }, // J
                { 0, 0, 0, 0, 3, 3, 2, 1, 0, 0, 0, 0, 0 }, // 10
                { 0, 0, 0, 0, 0, 3, 2, 1, 1, 0, 0, 0, 0 }, // 9
                { 0, 0, 0, 0, 0, 0, 3, 1, 1, 0, 0, 0, 0 }, // 8
                { 0, 0, 0, 0, 0, 0, 0, 3, 1, 1, 0, 0, 0 }, // 7
                { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0 }, // 6
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0 }, // 5
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }, // 4
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }, // 3
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 } // 2
            };
        //TODO: check pairs in suited only, shrink matrices
        private static readonly int[,] StartingHandRecommendationsUnsuited =
            {
                { 3, 3, 3, 3, 3, 1, 1, 1, 0, 0, 0, 0, 0 }, // A
                { 0, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0 }, // K
                { 0, 0, 3, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0 }, // Q
                { 0, 0, 0, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0 }, // J
                { 0, 0, 0, 0, 3, 1, 1, 0, 0, 0, 0, 0, 0 }, // 10
                { 0, 0, 0, 0, 0, 3, 1, 1, 0, 0, 0, 0, 0 }, // 9
                { 0, 0, 0, 0, 0, 0, 3, 1, 0, 0, 0, 0, 0 }, // 8
                { 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0 }, // 7
                { 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0 }, // 6
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0 }, // 5
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }, // 4
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }, // 3
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 } // 2
            };

        // http://www.rakebackpros.net/texas-holdem-starting-hands/
        public static CardValuationType PreFlopLookupTable(Card firstCard, Card secondCard)
        {
            int value;
            // big vs small blind
            if (firstCard.Suit == secondCard.Suit)
            {
                value = firstCard.Type > secondCard.Type
                            ? StartingHandRecommendationsSuited[
                                MaxCardTypeValue - (int)firstCard.Type,
                                  MaxCardTypeValue - (int)secondCard.Type]
                            : StartingHandRecommendationsSuited[
                                MaxCardTypeValue - (int)secondCard.Type,
                                  MaxCardTypeValue - (int)firstCard.Type];
            }
            else
            {
                if (firstCard.Type > secondCard.Type)
                {
                    value =
                        StartingHandRecommendationsUnsuited[
                            MaxCardTypeValue - (int)firstCard.Type,
                            MaxCardTypeValue - (int)secondCard.Type];
                }
                else
                {
                    value =
                        StartingHandRecommendationsUnsuited[
                            MaxCardTypeValue - (int)secondCard.Type,
                            MaxCardTypeValue - (int)firstCard.Type];
                }
            }

            switch (value)
            {
                case 0:
                    return CardValuationType.Unplayable;
                case 1:
                    return CardValuationType.NotRecommended;
                case 2:
                    return CardValuationType.Risky;
                case 3:
                    return CardValuationType.Recommended;
                default:
                    return CardValuationType.Unplayable;
            }
        }

        // TODO: generate all board variants or use hand potential algorithm
        public static float PostFlop(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            int ahead = 0, tied = 0, behind = 0;

            // Assume boardCards>=3
            var ourHandsCards = boardCards.ToList();//ours + comunity cards
            ourHandsCards.Add(firstCard);
            ourHandsCards.Add(secondCard);

            //Logic.HandRankType ourRank = Helpers.GetHandRank(handsCards);
            IList<Card> fullDeck = Deck.AllCards;
            var remainingCasrds = fullDeck.ToList();

            foreach (var card in ourHandsCards)
            {
                remainingCasrds.Remove(card);
            }

            var oponentCardsVariants = CombinationsNoRepetitionsIterative(remainingCasrds);

            foreach (var variant in oponentCardsVariants)
            {
                int handsComparisonResult = Helpers.CompareCards(ourHandsCards, variant.Concat(boardCards));
                if (handsComparisonResult > 0)
                {
                    ahead++;
                }
                else if (handsComparisonResult == 0)
                {
                    tied++;
                }
                else
                {
                    behind++;
                }
            }

            float chances = (ahead + (float)tied / 2) / (ahead + tied + behind);

            return chances;
        }

        private static int playerCardsCount=2;

        static void CombinationsNoRepetitions(int index, int start, List<Card[]> enemyCards, List<Card> remainingCards)
        {
            if (index >= playerCardsCount)
            {
                return;
            }
            else
            {
                var arr = new Card[playerCardsCount];
                for (int i = start; i < remainingCards.Count; i++)
                {
                    arr[index] = remainingCards[i];
                    CombinationsNoRepetitions(index + 1, i + 1, enemyCards, remainingCards);
                }
            }
        }

        private static IEnumerable<Card[]> CombinationsNoRepetitionsIterative(List<Card> remainingCards)
        {
            Card[] result = new Card[playerCardsCount];
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
                    if (index == playerCardsCount)
                    {
                        yield return result;
                        break;
                    }
                }
            }
        }
    }
}
