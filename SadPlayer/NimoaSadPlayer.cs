namespace SadPlayer
{
    using System;
    using System.Collections.Generic;
    using Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Cards;
    using TexasHoldem.Logic.Helpers;
    using TexasHoldem.Logic.Players;

    public class NimoaSadPlayer : BasePlayer // Bill Chen formula http://www.simplyholdem.com/chen.html
    {
        private static double ownCardsStrength;
        private static HandEvaluator evaluator = new HandEvaluator();

        public override string Name { get; } = "Adrian.Bozhankov_" + Guid.NewGuid();

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            if (context.RoundType == GameRoundType.PreFlop)
            {
                ownCardsStrength = CardEvaluator.CalculateStrength(this.FirstCard, this.SecondCard);
                if (ownCardsStrength >= 9)
                {
                    return PlayerAction.Raise((int)ownCardsStrength * context.SmallBlind);
                }

                if (ownCardsStrength >= 8)
                {
                   return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (context.RoundType == GameRoundType.Flop)
            {
                var cardsToevaluate = new List<Card>();

                foreach (var cc in this.CommunityCards)
                {
                    cardsToevaluate.Add(cc);
                }

                cardsToevaluate.Add(this.FirstCard);
                cardsToevaluate.Add(this.SecondCard);
                var bestCard = evaluator.GetBestHand(cardsToevaluate);
                if (bestCard.RankType == 0)
                {
                    return PlayerAction.Fold();
                }
                else
                {
                    return PlayerAction.CheckOrCall();
                }
            }

            return PlayerAction.CheckOrCall();
        }
    }
}
