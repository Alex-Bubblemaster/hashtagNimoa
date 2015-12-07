namespace TexasHoldem.AI.NimoaPlayer
{
    using System;
    using System.Linq;

    using TexasHoldem.AI.NimoaPlayer.Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using TexasHoldem.Logic.Players;

    public class NimoaPlayer : BasePlayer
    {
        private static int gamesCount = 0;

        private static int startMoney;

        private static float roundOdds;

        private static bool enemyAlwaysAllIn = true;

        private static bool enemyAlwaysRise = true;

        public override string Name { get; } = "DaDummestPlayerEver_" + Guid.NewGuid();

        public override void StartRound(StartRoundContext context)
        {
            if (context.RoundType == GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PreFlopOdsLookupTable(this.FirstCard, this.SecondCard);
            }
            else if (context.RoundType == GameRoundType.Flop || context.RoundType == GameRoundType.Turn)
            {
                // Approximation
                roundOdds = HandPotentialValuation.GetHandPotentialMonteCarloApproximation2(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    250);
            }
            else if (context.RoundType == GameRoundType.River)
            {
                roundOdds = HandStrengthValuation.HandStrengthMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    500);
            }

            base.StartRound(context);
        }

        public override void StartGame(StartGameContext context)
        {
            base.StartGame(context);
            gamesCount++;
            startMoney = context.StartMoney;
        }

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            var merit = roundOdds * context.CurrentPot / context.MoneyToCall;

            var enemyMoney = (startMoney * 2) - context.MoneyLeft - context.CurrentPot;
            if (context.PreviousRoundActions.Count > 0)
            {
                //var enemyLastAction = context.PreviousRoundActions.Where(x => x.PlayerName != this.Name).LastOrDefault().Action;
                var enemyLastAction = context.PreviousRoundActions.Last().Action;

                if (enemyAlwaysRise && enemyLastAction.Type != PlayerActionType.Raise && enemyMoney > 0)
                {
                    enemyAlwaysRise = false;
                    enemyAlwaysAllIn = false;
                }

                if (enemyAlwaysAllIn && enemyLastAction.Money < enemyMoney && enemyLastAction.Money > context.SmallBlind)
                {
                    enemyAlwaysAllIn = false;
                }
            }

            if (!enemyAlwaysAllIn && merit < 1 && context.CurrentPot > 0)
            {
                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (roundOdds > .9 && context.MoneyLeft > 0)
            {
                var moneyToRaise = Math.Min(enemyMoney, context.MoneyLeft) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (enemyAlwaysAllIn && roundOdds >= .8)
            {
                PlayerAction.Raise(context.SmallBlind);
            }

            if (roundOdds >= .8) // Recommended
            {
                var maxBet = context.MoneyLeft / RandomProvider.Next(2, 4);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (roundOdds >= .7) // Recommended
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(3, 8);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (roundOdds >= .6)
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft / 2)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(6, 14);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (roundOdds > .5) //// Risky
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft / 4)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(12, 24);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (merit > 1)
            {
                return PlayerAction.CheckOrCall();
            }

            // fcsk it
            if (context.CanCheck || context.MoneyToCall <= context.SmallBlind)
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
