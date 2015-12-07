namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Helpers;

    public class HandPotentialValuation
    {
        private const int AllPublicCardsCount = 5;
        private static readonly IList<Card> FullDeck = Deck.AllCards;
        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();

        public static float HandPotentialMonteCarloApproximation(
            Card firstCard,
            Card secondCard,
            IEnumerable<Card> boardCards,
            int randomCasesCount)
        {
            var remainingCards = FullDeck.ToList();
            remainingCards.Remove(firstCard);
            remainingCards.Remove(secondCard);

            foreach (var card in boardCards)
            {
                remainingCards.Remove(card);
            }

            var ourKnownCards = boardCards.ToList();
            ourKnownCards.Add(firstCard);
            ourKnownCards.Add(secondCard);

            var cardVariations = CardsCombinations.GetRandomCardsIndexes(
                randomCasesCount,
                2 + 5 - boardCards.Count(),
                remainingCards.Count - 1);

            int ahead = 0;
            int tied = 0;
            int behind = 0;
            foreach (var cardVariation in cardVariations)
            {
                var publicCardsVariant = boardCards.ToList();

                if (boardCards.Count() < 5)
                {
                    publicCardsVariant.Add(remainingCards[cardVariation[2]]);
                }
                else if (boardCards.Count() == 3)
                {
                    publicCardsVariant.Add(remainingCards[cardVariation[3]]);
                }

                var ourCards = publicCardsVariant.ToList();
                ourCards.Add(firstCard);
                ourCards.Add(secondCard);

                // enemy cards
                publicCardsVariant.Add(remainingCards[cardVariation[0]]);
                publicCardsVariant.Add(remainingCards[cardVariation[1]]);

                int comparisonResult = Helpers.CompareCards(ourCards, publicCardsVariant);
                if (comparisonResult > 0)
                {
                    ahead++;
                }
                else if (comparisonResult == 0)
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

        public static float GetHandPotential(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            var cardsToAdd = AllPublicCardsCount - boardCards.Count();

            if (cardsToAdd == 0)
            {
                return HandStrengthValuation.PostFlop(firstCard, secondCard, boardCards);
            }

            var remainingCards = FullDeck.ToList();
            remainingCards.Remove(firstCard);
            remainingCards.Remove(secondCard);

            foreach (var card in boardCards)
            {
                remainingCards.Remove(card);
            }

            float odsSum = 0;
            if (cardsToAdd == 1)
            {
                foreach (var card in remainingCards)
                {
                    List<Card> newBoard = boardCards.ToList();
                    newBoard.Add(card);
                    odsSum += HandStrengthValuation.PostFlop(firstCard, secondCard, newBoard);
                }

                return odsSum / remainingCards.Count;
            }

            // A-A-a-a-ahhhh thhheeeeee sloooowwwwnesssss
            IEnumerable<Card[]> potentialComunityCards = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, 2);

            foreach (var set in potentialComunityCards)
            {
                odsSum += HandStrengthValuation.PostFlop(firstCard, secondCard, boardCards.Concat(set));
            }

            return odsSum / potentialComunityCards.Count();
        }

        // for small subsets
        public static float GetHandPotentialMonteCarloApproximation2(Card firstCard, Card secondCard, IEnumerable<Card> boardCards, int variants)
        {
            if (boardCards.Count() == AllPublicCardsCount)
            {
                return HandStrengthValuation.PostFlop(firstCard, secondCard, boardCards);
            }

            var remainingCards = FullDeck.ToList();
            remainingCards.Remove(firstCard);
            remainingCards.Remove(secondCard);

            foreach (var card in boardCards)
            {
                remainingCards.Remove(card);
            }

            var enemydCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, 2).ToList();
            //var boardVariants = new List<IEnumerable<Card>>();

            // Fast delete
            var boardCardsVariants = new List<IEnumerable<Card>>();
            if (boardCards.Count() == 3)
            {
                for (int i = 0; i < enemydCardsVariants.Count; i++)
                {
                    boardCardsVariants.Add(boardCards.Concat(enemydCardsVariants[i]));
                }
            }
            else if (boardCards.Count() == 4)
            {
                for (int i = 0; i < remainingCards.Count; i++)
                {
                    List<Card> cards = boardCards.ToList();
                    cards.Add(remainingCards[i]);
                    boardCardsVariants.Add(cards);
                }
            }

            int ahead = 0;
            int tied = 0;
            int behind = 0;

            var ourHand = new List<Card>() { firstCard, secondCard };
            var usedOpponentCarsdIndex=new HashSet<int>();
            var usedBoardsVariantsIndex = new HashSet<int>();
            for (int i = 0; i < variants; i++)
            {
                int randomBoardIndex = RandomGenerator.RandomInt(0, boardCardsVariants.Count);
                int randomOpponentCardsIndex = RandomGenerator.RandomInt(0, enemydCardsVariants.Count);

                if (usedOpponentCarsdIndex.Contains(randomOpponentCardsIndex) || usedBoardsVariantsIndex.Contains(randomBoardIndex))
                {
                    continue;
                }

                var opponentCards = enemydCardsVariants[randomOpponentCardsIndex];
                var newPublicCards = boardCardsVariants[randomBoardIndex];
                while (newPublicCards.Contains(opponentCards[0]) || newPublicCards.Contains(opponentCards[1]))
                {
                    randomBoardIndex = RandomGenerator.RandomInt(0, boardCardsVariants.Count);
                    randomOpponentCardsIndex = RandomGenerator.RandomInt(0, enemydCardsVariants.Count);

                    opponentCards = enemydCardsVariants[randomOpponentCardsIndex];
                    newPublicCards = boardCardsVariants[randomBoardIndex];
                }

                usedOpponentCarsdIndex.Add(randomOpponentCardsIndex);
                if (boardCardsVariants.Count > variants)
                {
                    usedBoardsVariantsIndex.Add(randomBoardIndex);
                }

                var handsComparisonResult = Helpers.CompareCards(
                    ourHand.Concat(newPublicCards),
                    opponentCards.Concat(newPublicCards));

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
