using System.Collections.Generic;
using System.Linq;
using TexasHoldem.Logic.Cards;
using TexasHoldem.Logic.Helpers;
using TexasHoldem.Logic.Players;

namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    public class EnemyPredictions
    {
        private const int MaxCardTypeValue = 14;
        private const int PlayerCardsCount = 2;
        private static readonly IList<Card> FullDeck = Deck.AllCards;
        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();

        public static IList<Card[]> PredictEnemyCardsPreFlop(Card firstCard, Card secondCard, PlayerAction firstEnemyPreFlopAction)
        {
            var ourHandsCards = new Card[] { firstCard, secondCard }; // ours + comunity cards

            var remainingCards = FullDeck.ToList();

            foreach (var card in ourHandsCards)
            {
                remainingCards.Remove(card);
            }

            IEnumerable<Card[]> oponentCardsVariants = CardsCombinations.CombinationsNoRepetitionsIterative(remainingCards, PlayerCardsCount);

            List<Card[]> opponentLikelyCardsPredictions = new List<Card[]>();

            foreach (var oponentCardsVariant in oponentCardsVariants)
            {
                var enemyOdds = HandStrengthValuation.PreFlopOdsLookupTable(oponentCardsVariant[0],
                    oponentCardsVariant[1]);

                if (firstEnemyPreFlopAction.Money > 0 && enemyOdds > .5)
                {
                    opponentLikelyCardsPredictions.Add(oponentCardsVariant);
                }
                else if (firstEnemyPreFlopAction.Money == 0 && enemyOdds <= .65)
                {
                    opponentLikelyCardsPredictions.Add(oponentCardsVariant);
                }
            }

            return opponentLikelyCardsPredictions;
        }

        public static IList<Card[]> UpdateEnemyCardsGuess(IList<Card[]> enemyCardsGuesses, IEnumerable<Card> boardCards)
        {
            var enemyCardsFastDelete = enemyCardsGuesses.Select((s, i) => new { s, i }).ToDictionary(x => x.i, x => x.s);
            for (int i = 0; i < enemyCardsFastDelete.Count; i++)
            {
                foreach (var card in boardCards)
                {
                    if (enemyCardsFastDelete[i].Contains(card))
                    {
                        enemyCardsFastDelete.Remove(i);
                        break;
                    }
                }
            }

            return enemyCardsFastDelete.Select(cards => cards.Value).ToList();
        }
    }
}
