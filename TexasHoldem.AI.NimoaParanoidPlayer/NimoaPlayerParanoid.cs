namespace TexasHoldem.AI.NimoaParanoidPlayer
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using TexasHoldem.AI.NimoaParanoidPlayer.Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using TexasHoldem.Logic.Players;

    public class NimoaPlayerParanoid : BasePlayer
    {
        public override string Name { get; } = "ScardyCat_" + Guid.NewGuid();

        private static int startMoney;

        private static float roundOdds;

        private static bool enemyAlwaysRise = true;

        private static bool enemyAlwaysAllIn = true;

        public override void StartRound(StartRoundContext context)
        {
            if (context.RoundType == GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PreFlopOdsLookupTable(this.FirstCard, this.SecondCard);
            }
            /*else if (context.RoundType == GameRoundType.Flop)
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
                // var ods = HandPotentialValuation.GetHandPotential2(this.FirstCard, this.SecondCard, this.CommunityCards);
            }*/

            if (context.RoundType != GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PostFlop(this.FirstCard, this.SecondCard, this.CommunityCards);
            }

            base.StartRound(context);
        }



        public override PlayerAction GetTurn(GetTurnContext context)
        {
            float ods = roundOdds;

            // TODO: raise ods if last action is call and money <= smallBlind

            // last enemy action based paranoia. Only weors if the other AI is good.
            if (context.PreviousRoundActions.Count > 0)
            {
                var enemyLastAction = context.PreviousRoundActions.Last().Action;
                if (enemyAlwaysRise && enemyLastAction.Type != PlayerActionType.Raise)
                {
                    enemyAlwaysRise = false;
                }

                if (!enemyAlwaysRise && enemyLastAction.Type == PlayerActionType.Raise)
                {
                    if (enemyLastAction.Money > context.SmallBlind * 50)
                    {
                        ods -= .15f;
                    }
                    else if (enemyLastAction.Money > context.SmallBlind * 20)
                    {
                        ods -= .1f;
                    }
                    else if (enemyLastAction.Money > context.SmallBlind * 10)
                    {
                        ods -= .05f;
                    }
                }
            }

            var merit = ods * context.CurrentPot / context.MoneyToCall;
            if (merit < 1 && context.CurrentPot > 0)
            {
                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (ods > .9 && context.MoneyLeft > 0)
            {
                return PlayerAction.Raise(context.MoneyLeft);
            }

            if (ods >= .8)
            {
                var smallBlindsTimes = RandomProvider.Next(24, 50);
                return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
            }

            if (ods >= .7) // Recommended
            {
                if (context.MyMoneyInTheRound > startMoney / 2)
                {
                    return PlayerAction.CheckOrCall();
                }

                var smallBlindsTimes = RandomProvider.Next(12, 26);
                return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
            }

            if (ods >= .6)
            {
                if (context.MyMoneyInTheRound > startMoney / 4)
                {
                    return PlayerAction.CheckOrCall();
                }

                var smallBlindsTimes = RandomProvider.Next(6, 14);
                return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
            }

            if (ods > .5) // Risky
            {
                if (context.MyMoneyInTheRound > startMoney / 6)
                {
                    return PlayerAction.CheckOrCall();
                }

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
