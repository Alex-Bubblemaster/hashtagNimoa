using TexasHoldem.Logic.Cards;

namespace TexasHoldem.AI.NimoaPlayer
{
    using System;
    using System.Collections.Generic;
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

        private static Dictionary<int, List<int>> betsForBlinds = new Dictionary<int, List<int>>();

        private static int handNumber;

        private static List<float> oddsForThisRound;

        public override string Name { get; } = "NimoaPlayer" + Guid.NewGuid();

        public override void StartHand(StartHandContext context)
        {
            base.StartHand(context);
            handNumber = context.HandNumber;
        }

        public override void StartRound(StartRoundContext context)
        {
            if (context.RoundType == GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PreFlopOdsLookupTable(this.FirstCard, this.SecondCard);
            }
            else
            {
                oddsForThisRound = new List<float>();
                GetAverageOdds(context.RoundType);
            }

            base.StartRound(context);
        }

        // LassVegas method (slow, accurate). Calculate on every turn and get average of all the aproximate ods for the round to reducethe error.
        public float GetAverageOdds(GameRoundType roundType)
        {
            float ods;
            if (roundType == GameRoundType.Flop || roundType == GameRoundType.Turn)
            {
                // Approximation
                ods = HandPotentialValuation.HandPotentialMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    250);
            }
            else
            {
                ods = HandStrengthValuation.HandStrengthMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    500);
            }

            oddsForThisRound.Add(ods);
            roundOdds = oddsForThisRound.Average();
            return roundOdds;
        }

        public override void StartGame(StartGameContext context)
        {
            base.StartGame(context);
            gamesCount++;
            startMoney = context.StartMoney;
        }

        public override void EndGame(EndGameContext context)
        {
            base.EndGame(context);
        }

        public override void EndHand(EndHandContext context)
        {
            base.EndHand(context);
        }

        public override void EndRound(EndRoundContext context)
        {
            base.EndRound(context);
        }

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            if (context.MoneyLeft == 0)
            {
                return PlayerAction.CheckOrCall();
            }

            if (context.RoundType != GameRoundType.PreFlop)
            {
                GetAverageOdds(context.RoundType);
                /*if (context.RoundType == GameRoundType.Turn)
                {
                    var old = HandPotentialValuation.HandPotentialMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    250);
                    var accurate = HandPotentialValuation.GetHandPotential(
                        this.FirstCard,
                        this.SecondCard,
                        this.CommunityCards);
                    var br = 0;
                }*/
            }

            var ods = roundOdds;

            var enemyMoney = startMoney * 2 - context.MoneyLeft - context.CurrentPot;
            if (context.PreviousRoundActions.Count > 0)
            {
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

                // increase on check or call
                if (!enemyAlwaysRise && !enemyAlwaysAllIn)
                {
                    if (enemyLastAction.Type == PlayerActionType.Raise)
                    {
                        if (!betsForBlinds.ContainsKey(context.SmallBlind))
                        {
                            betsForBlinds[context.SmallBlind] = new List<int>();
                        }

                        var recentBets = betsForBlinds[context.SmallBlind];

                        if (recentBets.Count > 30)
                        {
                            var averageBet = recentBets.Average();

                            var maxBet = recentBets.Max();

                            if (enemyLastAction.Money >= maxBet * .7)
                            {
                                ods -= .15f;
                            }
                            else if (enemyLastAction.Money > averageBet)
                            {
                                ods -= .1f;
                            }
                            else if (enemyLastAction.Money > averageBet / 2)
                            {
                                ods -= .05f;
                            }
                        }

                        if (enemyLastAction.Money > context.SmallBlind * 2)
                        {
                            recentBets.Add(enemyLastAction.Money);
                        }
                    }
                    else if (enemyLastAction.Type == PlayerActionType.CheckCall)
                    {
                        if (enemyLastAction.Money == 0)
                        {
                            ods += .1f;
                        }
                        else if (enemyLastAction.Money < context.SmallBlind*2)
                        {
                            ods += .05f;
                        }
                    }
                }
            }


            var merit = ods * context.CurrentPot / context.MoneyToCall;

            if (!enemyAlwaysAllIn && merit < 1 && context.CurrentPot > 0)
            {
                if (context.CanCheck)
                {
                    return PlayerAction.CheckOrCall();
                }

                return PlayerAction.Fold();
            }

            if (ods > .9 && context.MoneyLeft > 0)
            {
                var moneyToRaise = Math.Min(enemyMoney, context.MoneyLeft) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (enemyAlwaysAllIn && ods >= .8)
            {
                PlayerAction.Raise(context.SmallBlind);
            }

            if (ods >= .8) //// Recommended
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft / 2)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(2, 4);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (ods >= .7) //// Recommended
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft / 3)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(3, 8);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (ods >= .6)
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft / 4)
                {
                    return PlayerAction.CheckOrCall();
                }

                var maxBet = context.MoneyLeft / RandomProvider.Next(6, 14);
                var moneyToRaise = Math.Min(maxBet, enemyMoney) + 1;
                return PlayerAction.Raise(moneyToRaise);
            }

            if (ods > .5) //// Risky
            {
                if (context.MyMoneyInTheRound > context.MoneyLeft / 5)
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
            if (context.CanCheck) // || (context.MoneyToCall <= context.SmallBlind && context.SmallBlind < context.MoneyLeft / 15)
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