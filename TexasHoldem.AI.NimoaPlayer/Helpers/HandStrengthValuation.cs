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
        private static readonly IList<Card> fullDeck = Deck.AllCards;

        private static readonly int[,] StartingHandRecommendations =
            {
                { 3, 3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1 }, // AA AKs AQs AJs ATs A9s A8s A7s A6s A5s A4s A3s A2s
                { 3, 3, 3, 3, 3, 2, 1, 1, 1, 1, 1, 1, 1 }, // AKo KK KQs KJs KTs K9s K8s K7s K6s K5s K4s K3s K2s
                { 3, 3, 3, 3, 3, 2, 2, 0, 0, 0, 0, 0, 0 }, // AQo KQo QQ QJs QTs Q9s Q8s Q7s Q6s Q5s Q4s Q3s Q2s
                { 3, 3, 2, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0 }, // AJo KJo QJo JJ JTs J9s J8s J7s J6s J5s J4s J3s J2s
                { 3, 2, 2, 2, 3, 3, 2, 1, 0, 0, 0, 0, 0 }, // ATo KTo QTo JTo TT T9s T8s T7s T6s T5s T4s T3s T2s
                { 1, 1, 1, 1, 1, 3, 2, 1, 1, 0, 0, 0, 0 }, // A9o K9o Q9o J9o T9o 99 98s 97s 96s 95s 94s 93s 92s
                { 1, 0, 0, 1, 1, 1, 3, 1, 1, 0, 0, 0, 0 }, // A8o K8o Q8o J8o T8o 98o 88 87s 86s 85s 84s 83s 82s
                { 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 0, 0, 0 }, // A7o K7o Q7o J7o T7o 97o 87o 77 76s 75s 74s 73s 72s
                { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0 }, // A6o K6o Q6o J6o T6o 96o 86o 76o 66 65s 64s 63s 62s
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0 }, // A5o K5o Q5o J5o T5o 95o 85o 75o 65o 55 54s 53s 52s
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }, // A4o K4o Q4o J4o T4o 94o 84o 74o 64o 54o 44 43s 42s
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }, // A3o K3o Q3o J3o T3o 93o 83o 73o 63o 53o 43o 33 32s
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 } // A2o K2o Q2o J2o T2o 92o 82o 72o 62o 52o 42o 32o 22
            };

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


        // http://www.rakebackpros.net/texas-holdem-starting-hands/
        public static CardValuationType PreFlopLookupTable(Card firstCard, Card secondCard)
        {
            var value = firstCard.Suit == secondCard.Suit
                          ? (firstCard.Type > secondCard.Type
                                 ? StartingHandRecommendations[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]
                                 : StartingHandRecommendations[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type])
                          : (firstCard.Type > secondCard.Type
                                 ? StartingHandRecommendations[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type]
                                 : StartingHandRecommendations[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]);

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

        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();

        // TODO: generate all board variants or use hand potential algorithm
        public static float PostFlop(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            int ahead = 0, tied = 0, behind = 0;

            // Assume boardCards>=3
            var ourHandsCards = boardCards.ToList(); // ours + comunity cards
            ourHandsCards.Add(firstCard);
            ourHandsCards.Add(secondCard);

            BestHand ourBestHand = HandEvaluator.GetBestHand(ourHandsCards);
            var remainingCasrds = fullDeck.ToList();

            foreach (var card in ourHandsCards)
            {
                remainingCasrds.Remove(card);
            }

            var oponentCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCasrds, PlayerCardsCount);
            //var oponentCardsVariants2=new List<Card[]>();
            //CardsCombinations.CombinationsNoRepetitions(0, 0, remainingCasrds, PlayerCardsCount, oponentCardsVariants2, new Card[2]);

            foreach (var variant in oponentCardsVariants)
            {
                // TODO: evaluate our rank only once
                BestHand oponentCurrentBestHand = HandEvaluator.GetBestHand(variant.Concat(boardCards));
                int handsComparisonResult = ourBestHand.CompareTo(oponentCurrentBestHand);
                // int handsComparisonResult = Helpers.CompareCards(ourHandsCards, variant.Concat(boardCards));
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
