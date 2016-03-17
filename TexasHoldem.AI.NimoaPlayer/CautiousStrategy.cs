namespace TexasHoldem.AI.NimoaPlayer
{
    using System;

    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using TexasHoldem.Logic.Players;

    class CautiousStrategy
    {
        public static PlayerAction GetPlayerAction(float odds, int enemyMoney, GetTurnContext context)
        {
            if (odds > .95 && context.MoneyLeft > 0)
            {
                var moneyToRaise = Math.Min(enemyMoney, context.MoneyLeft) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (odds >= .8) //// Recommended
            {
                int moneyToRaise = context.MoneyLeft / 3 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 2 || moneyToRaise <= 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(2, 4);
                moneyToRaise = Math.Min(maxBet, moneyToRaise) + 1;
                //var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            var merit = odds * context.CurrentPot / context.MoneyToCall;

            if (merit < 1 && context.CurrentPot > 0)
            {
                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (odds >= .7) //// Recommended
            {
                int moneyToRaise = context.MoneyLeft / 4 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 3 || moneyToRaise <= 0)
                //if (context.MyMoneyInTheRound > context.MoneyLeft / 3)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(3, 8);
                moneyToRaise = Math.Min(maxBet, moneyToRaise) + 1;
                //var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (odds >= .6)
            {
                int moneyToRaise = context.MoneyLeft / 5 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 4 || moneyToRaise <= 0)
                //if (context.MyMoneyInTheRound > context.MoneyLeft / 4)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(6, 14);
                moneyToRaise = Math.Min(maxBet, moneyToRaise) + 1;
                //var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (odds > .5) //// Risky
            {
                int moneyToRaise = context.MoneyLeft / 6 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 5 || moneyToRaise <= 0)
                //if (context.MyMoneyInTheRound > context.MoneyLeft / 5)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(12, 24);
                moneyToRaise = Math.Min(maxBet, moneyToRaise) + 1;
                //var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (merit > 1)
            {
                if (context.RoundType == GameRoundType.PreFlop && context.MyMoneyInTheRound <= context.SmallBlind && context.CurrentPot <= context.SmallBlind * 3)
                {
                    return PlayerAction.Raise(1);
                }

                return PlayerAction.CheckOrCall();
            }

            // fcsk it
            if (context.CanCheck) // || (context.MoneyToCall <= context.SmallBlind && context.SmallBlind < context.MoneyLeft / 20)
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
