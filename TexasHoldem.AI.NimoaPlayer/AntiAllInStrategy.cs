namespace TexasHoldem.AI.NimoaPlayer
{
    using TexasHoldem.Logic.Players;

    public class AntiAllInStrategy
    {
        public static PlayerAction GetPlayerAction(float odds, int enemyMoney, GetTurnContext context)
        {
            var merit = odds * context.CurrentPot / context.MoneyToCall;

            if (odds > .63)
            {
                return PlayerAction.Raise(context.MoneyLeft + 1);
            }

            if (odds >= .6 && enemyMoney + context.MoneyToCall < context.MoneyLeft)
            {
                return PlayerAction.Raise(context.MoneyLeft + 1);
            }

            if (odds >= .5 && enemyMoney + context.MoneyToCall < context.MoneyLeft / 4)
            {
                return PlayerAction.Raise(context.MoneyLeft + 1);
            }

            // PlayerAction.Raise(context.MoneyLeft + 1);
            if (merit > 6) //moneytocall
            {
                return PlayerAction.CheckOrCall();
            }

            // fcsk it
            if (context.CanCheck)
            {
                return PlayerAction.CheckOrCall();
            }

            return PlayerAction.Fold();
        }
    }
}
