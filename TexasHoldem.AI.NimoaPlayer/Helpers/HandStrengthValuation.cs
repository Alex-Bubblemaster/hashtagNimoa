namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Helpers;

    public static class HandStrengthValuation
    {
        private const int MaxCardTypeValue = 14;
        private const int PlayerCardsCount = 2;
        private static readonly IList<Card> FullDeck = Deck.AllCards;
        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();

        private static readonly float[,] StartingHandsOds =
            {
                { .85f, .76f, .66f, .65f, .65f, .63f, .62f, .61f, .60f, .60f, .59f, .58f, .57f },
                { .65f, .82f, .63f, .63f, .62f, .60f, .58f, .58f, .57f, .56f, .55f, .54f, .53f },
                { .64f, .61f, .80f, .60f, .59f, .58f, .56f, .54f, .54f, .53f, .52f, .51f, .50f },
                { .64f, .61f, .58f, .77f, .58f, .56f, .54f, .52f, .51f, .50f, .49f, .48f, .47f },
                { .63f, .59f, .57f, .55f, .75f, .54f, .52f, .51f, .49f, .47f, .47f, .46f, .45f },
                { .61f, .58f, .55f, .53f, .52f, .72f, .51f, .49f, .47f, .46f, .44f, .43f, .42f },
                { .60f, .56f, .54f, .51f, .50f, .48f, .69f, .48f, .46f, .45f, .43f, .41f, .40f },
                { .59f, .55f, .52f, .50f, .48f, .46f, .45f, .66f, .45f, .44f, .42f, .40f, .38f },
                { .58f, .54f, .51f, .48f, .46f, .44f, .43f, .42f, .63f, .43f, .41f, .40f, .38f },
                { .58f, .53f, .50f, .47f, .44f, .43f, .41f, .41f, .40f, .60f, .41f, .40f, .38f },
                { .57f, .52f, .49f, .46f, .44f, .42f, .39f, .38f, .39f, .38f, .57f, .38f, .37f },
                { .56f, .51f, .48f, .45f, .43f, .40f, .37f, .37f, .36f, .36f, .35f, .54f, .36f },
                { .55f, .51f, .47f, .44f, .42f, .39f, .37f, .35f, .34f, .34f, .33f, .32f, .50f }
            };

        public static float PreFlopOdsLookupTable(Card firstCard, Card secondCard)
        {
            float value = firstCard.Suit == secondCard.Suit
                          ? (firstCard.Type > secondCard.Type
                                 ? StartingHandsOds[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]
                                 : StartingHandsOds[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type])
                          : (firstCard.Type > secondCard.Type
                                 ? StartingHandsOds[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type]
                                 : StartingHandsOds[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]);

            return value;
        }

        // TODO: generate all board variants or use hand potential algorithm
        public static float PostFlop(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            int ahead = 0, tied = 0, behind = 0;

            // Assume boardCards>=3
            var ourHandsCards = boardCards.ToList(); // ours + comunity cards
            ourHandsCards.Add(firstCard);
            ourHandsCards.Add(secondCard);

            BestHand ourBestHand = HandEvaluator.GetBestHand(ourHandsCards);
            var remainingCards = FullDeck.ToList();

            foreach (var card in ourHandsCards)
            {
                remainingCards.Remove(card);
            }

            var oponentCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, PlayerCardsCount);
            ////var oponentCardsVariants2=new List<Card[]>();
            ////CardsCombinations.CombinationsNoRepetitions(0, 0, remainingCasrds, PlayerCardsCount, oponentCardsVariants2, new Card[2]);

            foreach (var variant in oponentCardsVariants)
            {
                // TODO: evaluate our rank only once
                BestHand oponentCurrentBestHand = HandEvaluator.GetBestHand(variant.Concat(boardCards));
                int handsComparisonResult = ourBestHand.CompareTo(oponentCurrentBestHand);
                //// int handsComparisonResult = Helpers.CompareCards(ourHandsCards, variant.Concat(boardCards));
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

            float chances = (ahead + ((float)tied / 2)) / (ahead + tied + behind);

            return chances;
        }
    }
}