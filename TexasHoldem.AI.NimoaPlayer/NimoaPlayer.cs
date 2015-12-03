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

                if (context.PreviousRoundActions.Count > 0)
                {
                    var enemyLastAction = context.PreviousRoundActions.Last().Action;
                    if (enemyLastAction.Type == PlayerActionType.Raise)
                    {
                        if (enemyLastAction.Money > context.SmallBlind * 10)
                        {
                            playHand--;
                        }
                    }
                }

                //check total bid ammount
                if (playHand == CardValuationType.Risky && context.MoneyToCall < context.SmallBlind * 20)
                {
                    var smallBlindsTimes = RandomProvider.Next(1, 8);
                    return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
                }

                if (playHand == CardValuationType.Risky)
                {
                    return PlayerAction.CheckOrCall();
                }

                if (playHand == CardValuationType.Recommended && context.MoneyToCall < context.SmallBlind * 50)
                {
                    var smallBlindsTimes = RandomProvider.Next(6, 14);
                    return PlayerAction.Raise(context.SmallBlind * smallBlindsTimes);
                }

                if (playHand == CardValuationType.Recommended)
                {
                    return PlayerAction.CheckOrCall();
                }

                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            float ods = 0;
            // Fast
            if (context.RoundType == GameRoundType.Flop || context.RoundType == GameRoundType.River)
            {
                ods = HandStrengthValuation.PostFlop(this.FirstCard, this.SecondCard, this.CommunityCards);
            }
            else
            {
                // Smart, really really slow
                ods = HandPotentialValuation.GetHandPotential2(this.FirstCard, this.SecondCard, this.CommunityCards);
            }

            //var ods = HandPotentialValuation.GetHandStrength(this.FirstCard, this.SecondCard, this.CommunityCards);

            var merit = ods * context.CurrentPot / context.MoneyToCall;
            if (merit < 1)
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
