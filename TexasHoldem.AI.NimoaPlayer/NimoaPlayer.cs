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

            float ods = 0;
            // Fast
            //if (context.RoundType == GameRoundType.River)
            //{
                ods = HandStrengthValuation.PostFlop(this.FirstCard, this.SecondCard, this.CommunityCards);
            //}
            /*else
            {
                // Smart, really really slow
                ods = HandPotentialValuation.GetHandPotential2(this.FirstCard, this.SecondCard, this.CommunityCards);
            }*/

            //var ods = HandPotentialValuation.GetHandStrength(this.FirstCard, this.SecondCard, this.CommunityCards);

            // last enemy action based paranoia. Only weors if the other AI is good.
            /*var enemyLastAction = context.PreviousRoundActions.Last().Action;
            if (enemyLastAction.Type == PlayerActionType.Raise)
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
            }*/

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
