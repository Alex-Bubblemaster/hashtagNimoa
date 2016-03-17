namespace TexasHoldem.AI.NimoaPlayer
{
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Players;

    public class AntiAlwaysCallStrategy
    {
        public static PlayerAction GetPlayerAction(float odds, int enemyMoney, GetTurnContext context)
        {

            var merit = odds * context.CurrentPot / context.MoneyToCall;

            if (context.RoundType != GameRoundType.River)
            {
                return PlayerAction.CheckOrCall();
            }

            if (odds > .99)
            {
                return PlayerAction.Raise(context.MoneyLeft + 1);
            }

            if (odds > .9)
            {
                return PlayerAction.Raise(context.MoneyLeft / 2 + 1);
            }

            if (odds >= .8)
            {
                return PlayerAction.Raise(context.MoneyLeft / 3 + 1);
            }

            if (odds >= .7)
            {
                return PlayerAction.Raise(context.MoneyLeft / 4 + 1);
            }

            if (odds > .6)
            {
                return PlayerAction.Raise(context.MoneyLeft / 5 + 1);
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
