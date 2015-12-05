namespace TexasHoldem.AI.NimoaPlayer
{
    using System;
    using System.Linq;
    using System.Security.Authentication;

    using TexasHoldem.AI.NimoaPlayer.Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using TexasHoldem.Logic.Players;

    public class NimoaPlayer : BasePlayer
    {
        private static float roundOds;

        private static int gamesCount = 0;

        private static int startMoney;

        private static float roundOdds;

        public override string Name { get; } = "DaDummestPlayerEver_" + Guid.NewGuid();

        public override void StartRound(StartRoundContext context)
        {
            if (context.RoundType == GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PreFlopOdsLookupTable(this.FirstCard, this.SecondCard);
            }
            else if (context.RoundType == GameRoundType.Flop)
            {
                // Approximation
                roundOdds = HandPotentialValuation.HandPotentialMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    5000);
            }
            else if (context.RoundType == GameRoundType.River)
            {
                // Fast
                roundOdds = HandStrengthValuation.PostFlop(this.FirstCard, this.SecondCard, this.CommunityCards);
            }
            else
            {
                // >1% inaccuracy
                roundOdds = HandPotentialValuation.HandPotentialMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    2000);
                // Accurate, really really slow
                //var ods = HandPotentialValuation.GetHandPotential2(this.FirstCard, this.SecondCard, this.CommunityCards);
            }

            base.StartRound(context);
        }

        public override void StartGame(StartGameContext context)
        {
            base.StartGame(context);
            gamesCount++;
            startMoney = context.StartMoney;
        }

        /*public override void StartHand(StartHandContext context)
        {
            base.StartHand(context);
        }

        public override void EndRound(EndRoundContext context)
        {
            base.EndRound(context);
        }

        public override void EndHand(EndHandContext context)
        {
            base.EndHand(context);
        }

        public override void EndGame(EndGameContext context)
        {
            base.EndGame(context);
        }*/

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            //var comparison = HandPotentialValuation.GetHandPotential2(this.FirstCard, this.SecondCard, this.CommunityCards);
            float ods = roundOdds;

            var merit = ods * context.CurrentPot / context.MoneyToCall;
            if (merit < 1 && context.CurrentPot > 0)
            {
                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (context.MyMoneyInTheRound > startMoney / 5)
            {
                return PlayerAction.CheckOrCall();
            }

            if (ods > .9 && context.MoneyLeft > 0)
            {
                return PlayerAction.Raise(context.MoneyLeft);
            }

            if (ods >= .6)
            {
                var smallBlindsTimes = RandomProvider.Next(6, 14);
                return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
            }

            if (ods > .5) // Risky
            {
                var smallBlindsTimes = RandomProvider.Next(1, 8);
                return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
            }

            if (merit > 1)
            {
                return PlayerAction.CheckOrCall();
            }

            // fcsk it
            if (context.CanCheck || context.MoneyToCall <= 2)
            {
                return PlayerAction.CheckOrCall();
            }
            else
            {
                return PlayerAction.Fold();
            }
        }
    }
}
