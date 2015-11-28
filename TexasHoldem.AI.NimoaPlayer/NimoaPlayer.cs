namespace TexasHoldem.AI.NimoaPlayer
{
    using System;

    using TexasHoldem.AI.NimoaPlayer.Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using TexasHoldem.Logic.Players;

    public class NimoaPlayer : BasePlayer
    {
        public override string Name { get; } = "DaDummestPlayerEver_" + Guid.NewGuid();

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            if (context.RoundType == GameRoundType.PreFlop)
            {
                var playHand = HandStrengthValuation.PreFlopLookupTable(this.FirstCard, this.SecondCard);
                //||notrecomended
                if (playHand == CardValuationType.Unplayable)
                {
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return PlayerAction.Fold();
                    }
                }

                //check total bid ammount
                if (playHand == CardValuationType.Risky)
                {
                    var smallBlindsTimes = RandomProvider.Next(1, 8);
                    return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
                }

                if (playHand == CardValuationType.Recommended)
                {
                    var smallBlindsTimes = RandomProvider.Next(6, 14);
                    return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
                }

                //check total bid ammount
                return PlayerAction.CheckOrCall();
            }


            var ods = HandStrengthValuation.PostFlop(this.FirstCard, this.SecondCard, this.CommunityCards);
            var merit = ods * context.CurrentPot / context.MoneyToCall;
            if (merit < 1)
            {
                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (ods >= .6) // Recommended
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
