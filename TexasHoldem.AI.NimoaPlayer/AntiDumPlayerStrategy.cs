namespace TexasHoldem.AI.NimoaPlayer
{
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Players;

    class AntiDumPlayerStrategy
    {
        public static PlayerAction GetPlayerAction(float odds, int enemyMoney, GetTurnContext context)
        {
            var merit = odds * context.CurrentPot / context.MoneyToCall;

            if (context.RoundType != GameRoundType.River)
            {
                return PlayerAction.CheckOrCall();
                /*if (odds > .5)
                {
                    return PlayerAction.CheckOrCall();
                }

                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();*/
            }

            if (odds >= .995)
            {
                return PlayerAction.Raise(context.MoneyLeft + 1);
            }

            if (odds >= .95)
            {
                int moneyToRaise = context.MoneyLeft / 2 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft || moneyToRaise <= 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Raise(moneyToRaise + 1);
            }

            if (odds >= .85)
            {
                int moneyToRaise = context.MoneyLeft/3 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 2 || moneyToRaise <= 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Raise(moneyToRaise + 1);
            }

            if (odds >= .75)
            {
                int moneyToRaise = context.MoneyLeft / 4 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 3 || moneyToRaise <= 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Raise(moneyToRaise + 1);
            }

            if (odds >= .65)
            {
                int moneyToRaise = context.MoneyLeft / 5 - context.MoneyToCall * 2;
                if (context.MyMoneyInTheRound >= context.MoneyLeft / 4 || moneyToRaise <= 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Raise(moneyToRaise + 1);
            }

            //redundant
            /*if (odds >= .55)
            {
                return PlayerAction.Raise(context.MoneyLeft / 6 + 1);
            }

            if (odds > .5)
            {
                return PlayerAction.Raise(context.MoneyLeft / 7 + 1);
            }*/

            /*if (merit > 1) //moneytocall
            {
                return PlayerAction.CheckOrCall();
            }*/

            // fcsk it
            if (context.CanCheck)
            {
                return PlayerAction.CheckOrCall();
            }

            return PlayerAction.Fold();
        }
    }
}
