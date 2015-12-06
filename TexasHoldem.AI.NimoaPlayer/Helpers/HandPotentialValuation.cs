namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Helpers;

    public class HandPotentialValuation
    {
        private const int AllPublicCardsCount = 5;
        private static readonly IList<Card> fullDeck = Deck.AllCards;

        public static float GetHandStrength(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            var cardsToAdd = AllPublicCardsCount - boardCards.Count();

            if (cardsToAdd == 0)
            {
                return HandStrengthValuation.PostFlop(firstCard, secondCard, boardCards);
            }

            var remainingCards = fullDeck.ToList();
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

        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();

        public static float GetHandPotential2(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            if (boardCards.Count() == AllPublicCardsCount)
            {
                return HandStrengthValuation.PostFlop(firstCard, secondCard, boardCards);
            }

            var remainingCards = fullDeck.ToList();
            remainingCards.Remove(firstCard);
            remainingCards.Remove(secondCard);

            foreach (var card in boardCards)
            {
                remainingCards.Remove(card);
            }

            var enemydCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, 2);
            var boardVariants = new List<List<Card>>();
            if (boardCards.Count() == 3)
            {
                foreach (var boardCardsVariant in enemydCardsVariants)
                {
                    boardVariants.Add(boardCards.Concat(boardCardsVariant).ToList());
                }
            }
            else if (boardCards.Count() == 4)
            {
                foreach (var card in remainingCards)
                {
                    List<Card> cards = boardCards.ToList();
                    cards.Add(card);
                    boardVariants.Add(cards);
                }
            }

            int ahead = 0;
            int tied = 0;
            int behind = 0;

            //var ourCurrentHand = boardCards.ToList();
            //ourCurrentHand.Add(firstCard);
            //ourCurrentHand.Add(secondCard);

            foreach (var boardVariant in boardVariants)
            {
                // var ourHandVariant = ourCurrentHand.Concat(boardCardsVariant);
                var ourHand = boardVariant.ToList();
                ourHand.Add(firstCard);
                ourHand.Add(secondCard);
                var ourBestHandVariant = HandEvaluator.GetBestHand(ourHand);
                foreach (var enemydCardsVariant in enemydCardsVariants)
                {
                    if (boardVariant.Contains(enemydCardsVariant[0]) || boardVariant.Contains(enemydCardsVariant[1]))
                    {
                        continue;
                    }

                    var enemyHands = enemydCardsVariant.Concat(boardVariant);
                    var enemyBestHandVariant = HandEvaluator.GetBestHand(enemyHands);
                    var handsComparisonResult = ourBestHandVariant.CompareTo(enemyBestHandVariant);

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
            }

            float chances = (ahead + ((float)tied / 2)) / (ahead + tied + behind);

            return chances;
        }

        // chances of improoving your hand, list of improoved hands
        public void GetHandPotential3(Card firstCard, Card secondCard, IEnumerable<Card> boardCards)
        {
            if (boardCards.Count() == AllPublicCardsCount)
            {
                //return HandStrengthValuation.PostFlop(firstCard, secondCard, boardCards);
            }

            var remainingCards = fullDeck.ToList();
            remainingCards.Remove(firstCard);
            remainingCards.Remove(secondCard);

            foreach (var card in boardCards)
            {
                remainingCards.Remove(card);
            }

            var ourKnownCards = boardCards.ToList();
            ourKnownCards.Add(firstCard);
            ourKnownCards.Add(secondCard);

            BestHand ourWeakestPossibleHand = HandEvaluator.GetBestHand(ourKnownCards);
            var ourImprovedHands = new List<BestHand>();
            var enemydCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, 2);
            var boardVariants = new List<List<Card>>();
            if (boardCards.Count() == 3)
            {
                foreach (var boardCardsVariant in enemydCardsVariants)
                {
                    //var boarVariant = ;
                    boardVariants.Add(boardCards.Concat(boardCardsVariant).ToList());

                    var newBestHand = HandEvaluator.GetBestHand(ourKnownCards.Concat(boardCardsVariant).ToList());

                    if (newBestHand.CompareTo(ourWeakestPossibleHand) > 0)
                    {
                        ourImprovedHands.Add(newBestHand);
                    }
                }
            }
            else if (boardCards.Count() == 4)
            {
                foreach (var card in remainingCards)
                {
                    List<Card> cards = boardCards.ToList();
                    cards.Add(card);
                    boardVariants.Add(cards);

                    var ourCards = ourKnownCards.ToList();
                    ourCards.Add(card);
                    var newBestHand = HandEvaluator.GetBestHand(ourCards);

                    if (newBestHand.CompareTo(ourWeakestPossibleHand) > 0)
                    {
                        ourImprovedHands.Add(newBestHand);
                    }
                }
            }

            var chancesOfImproovingHand = ourImprovedHands.Count / 1081;
        }

        private static List<List<Card>> GetBoardVariants(List<Card> board, List<Card> remainingCards)
        {
            var boardCardsVariants = new List<Card[]>();
            var boardVariants = new List<List<Card>>();
            if (board.Count == 3)
            {
                boardCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, 2).ToList();
                foreach (var boardCardsVariant in boardCardsVariants)
                {
                    boardVariants.Add(board.Concat(boardCardsVariant).ToList());
                }

                return boardVariants;
            }

            foreach (var card in remainingCards)
            {
                List<Card> cards = board.ToList();
                cards.Add(card);
                boardVariants.Add(cards);
            }

            return boardVariants;
        }

        public static float HandPotentialMonteCarloApproximation(
            Card firstCard,
            Card secondCard,
            IEnumerable<Card> boardCards,
            int randomCasesCount)
        {
            var remainingCards = fullDeck.ToList();
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
    }
}
