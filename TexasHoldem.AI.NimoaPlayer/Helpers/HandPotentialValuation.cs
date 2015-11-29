namespace TexasHoldem.AI.NimoaPlayer.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using TexasHoldem.Logic.Cards;

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
    }
}
