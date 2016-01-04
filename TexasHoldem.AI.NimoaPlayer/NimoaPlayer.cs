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
    using Logic.Helpers;

    public class NimoaPlayer : BasePlayer
    {
        private static int gamesCount = 0;

        private static int startMoney;

        private static float roundOdds;

        private static bool enemyAlwaysAllIn;

        private static bool enemyAlwaysRise;

        private static bool enemyAlwaysCall;

        private static Dictionary<int, List<int>> betsForBlinds;

        private static int handNumber;

        private int aintiAIReverseLogickCoeficient;
        private int victories;
        private int defeats;

        private static int realBetThreshhold;

        private static List<float> oddsForThisRound;

        private string enemyName;

        private GameRoundType currentRoundType;

        private Dictionary<GameRoundType, bool> thisHandEnemyRising;

        private Dictionary<GameRoundType, List<EndGameState>> enemyGuessesPerRound;

        private Dictionary<GameRoundType, float> enemyAccuracyPerRound;

        private IList<Card[]> enemyCardsPrediction;

        private int enemyCardsPredictionHits;

        private int enemyCardsPredictionMisses;

        public override string Name { get; } = "NimoaPlayer" + Guid.NewGuid();

        public override void StartGame(StartGameContext context)
        {
            base.StartGame(context);
            gamesCount++;
            startMoney = context.StartMoney;
            var newEnemyName = context.PlayerNames.FirstOrDefault(n => n != this.Name);
            if (newEnemyName != this.enemyName)
            {
                this.enemyName = newEnemyName;
                enemyAlwaysAllIn = true;
                enemyAlwaysRise = true;
                enemyAlwaysCall = true;
                gamesCount = 1;
                this.enemyGuessesPerRound = new Dictionary<GameRoundType, List<EndGameState>>();
                this.enemyAccuracyPerRound = new Dictionary<GameRoundType, float>
                {
                    [GameRoundType.PreFlop] = 0,
                    [GameRoundType.Flop] = 0,
                    [GameRoundType.Turn] = 0,
                    [GameRoundType.River] = 0
                };
                this.enemyCardsPredictionHits = 0;
                this.enemyCardsPredictionMisses = 0;
                this.victories = 0;
                this.defeats = 0;
                betsForBlinds = new Dictionary<int, List<int>>();
            }
        }

        public override void StartHand(StartHandContext context)
        {
            base.StartHand(context);
            handNumber = context.HandNumber;
            realBetThreshhold = context.SmallBlind * 2;
            this.thisHandEnemyRising = new Dictionary<GameRoundType, bool>();
            this.enemyCardsPrediction = null;
        }

        public override void StartRound(StartRoundContext context)
        {
            this.aintiAIReverseLogickCoeficient = RandomProvider.Next(0, 100);
            this.currentRoundType = context.RoundType;
            oddsForThisRound = new List<float>();

            this.currentRoundType = context.RoundType;
            oddsForThisRound = new List<float>();

            if (context.RoundType == GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PreFlopOdsLookupTable(this.FirstCard, this.SecondCard);

                // assume worst case
                this.enemyCardsPrediction = EnemyPredictions.PredictEnemyCardsPreFlop(
                        this.FirstCard,
                        this.SecondCard,
                        PlayerAction.Raise(1));
            }
            else
            {
                // 2 much overhead
                /*if (enemyCardsPrediction != null)
                {
                    enemyCardsPrediction = EnemyPredictions.UpdateEnemyCardsGuess(enemyCardsPrediction, context.CommunityCards);
                }*/

                this.GetAverageOdds(context.RoundType);
            }

            base.StartRound(context);
        }

        // LassVegas method (slow, accurate). Calculate on every turn and get average of all the aproximate ods for the round to reducethe error.
        public float GetAverageOdds(GameRoundType roundType)
        {
            float ods;
            if (!(enemyAlwaysAllIn || enemyAlwaysRise) && this.enemyCardsPrediction != null)
            {
                if (roundType == GameRoundType.Flop || roundType == GameRoundType.Turn)
                {
                    // Approximation
                    ods = HandPotentialValuation.GetHandPotentialMonteCarloApproximation3(
                        this.FirstCard,
                        this.SecondCard,
                        this.CommunityCards,
                        this.enemyCardsPrediction.ToList(),
                        250);
                }
                else
                {
                    ods = HandStrengthValuation.HandStrengthMonteCarloApproximation2(
                        this.FirstCard,
                        this.SecondCard,
                        this.CommunityCards,
                        this.enemyCardsPrediction.ToList(),
                        500); // 500 passing
                }
            }
            else
            {
                if (roundType == GameRoundType.Flop || roundType == GameRoundType.Turn)
                {
                    // Approximation
                    ods = HandPotentialValuation.GetHandPotentialMonteCarloApproximation2(
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
                        500); // 500 passing
                }
            }

            oddsForThisRound.Add(ods);
            roundOdds = oddsForThisRound.Average();
            return roundOdds;
        }

        public override void EndGame(EndGameContext context)
        {
            base.EndGame(context);
        }

        public override void EndHand(EndHandContext context)
        {
            if (context.ShowdownCards.Count > 0)
            {
                var enemyCards = context.ShowdownCards[this.enemyName].ToList();
                // var myHand = context.ShowdownCards[this.Name].Concat(this.CommunityCards);
                // var enemyHand = enemyCards.Concat(this.CommunityCards);
                if (this.enemyCardsPrediction != null)
                {
                    this.enemyCardsPredictionMisses++;
                    foreach (var cards in this.enemyCardsPrediction)
                    {
                        if (cards.Contains(enemyCards[0]) && cards.Contains(enemyCards[1]))
                        {
                            this.enemyCardsPredictionHits++;
                            this.enemyCardsPredictionMisses--;
                            break;
                        }
                    }
                }

                //var gameResult = (EndGameState)Logic.Helpers.Helpers.CompareCards(enemyHand, myHand);
                // per round enemy accuracy statistics (too many games required! Potentially slow!)
                /*foreach (var round in this.thisHandEnemyRising)
                {
                    if (!this.enemyGuessesPerRound.ContainsKey(round.Key))
                    {
                        this.enemyGuessesPerRound.Add(round.Key, new List<EndGameState>());
                    }

                    if (round.Value)
                    {
                        this.enemyGuessesPerRound[round.Key].Add(gameResult);

                        if (this.enemyGuessesPerRound[round.Key].Count >= 300)
                        {
                            var enemyVictoriesCount = this.enemyGuessesPerRound[round.Key].Count(h => h == EndGameState.Win);
                            var drawsCount = this.enemyGuessesPerRound[round.Key].Count(h => h == EndGameState.Draw);
                            var enemyLosesCount = this.enemyGuessesPerRound[round.Key].Count(h => h == EndGameState.Loose);

                            if (!this.enemyAccuracyPerRound.ContainsKey(round.Key))
                            {
                                this.enemyAccuracyPerRound.Add(round.Key, 0);
                            }

                            this.enemyAccuracyPerRound[round.Key] = (enemyVictoriesCount + ((float)drawsCount / 2)) / (enemyVictoriesCount + drawsCount + enemyLosesCount);
                        }
                    }
                }*/
            }

            base.EndHand(context);
        }

        public override void EndRound(EndRoundContext context)
        {
            base.EndRound(context);
            // the enemy expects to win
            var enemyBets = context.RoundActions
                .Where(
                    a =>
                        a.PlayerName == this.enemyName && a.Action.Money > realBetThreshhold);

            if (enemyBets.Count() > 1)
            {
                this.thisHandEnemyRising.Add(this.currentRoundType, true);
            }
            else
            {
                this.thisHandEnemyRising.Add(this.currentRoundType, false);
            }
        }

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            if (context.MoneyLeft == 0)
            {
                return PlayerAction.CheckOrCall();
            }

            if (context.RoundType == GameRoundType.PreFlop && context.PreviousRoundActions.Count >= 3 && context.PreviousRoundActions.Count < 5)
            {
                var enemyActions = context.PreviousRoundActions.Where(a => a.PlayerName == this.enemyName).ToList();
                // remove entry fee
                enemyActions.RemoveAt(0);
                var enemyFirstPreFlopAction = enemyActions.FirstOrDefault().Action; // && a.Action.Money > context.SmallBlind

                // assume worst case
                /*this.enemyCardsPrediction = EnemyPredictions.PredictEnemyCardsPreFlop(
                        this.FirstCard,
                        this.SecondCard,
                        PlayerAction.Raise(1));*/

                if (enemyFirstPreFlopAction != null)
                {
                    /*this.enemyCardsPrediction = EnemyPredictions.PredictEnemyCardsPreFlop(
                        this.FirstCard,
                        this.SecondCard,
                        enemyFirstPreFlopAction);*/

                    /*roundOdds = HandStrengthValuation.HandStrengthPreFlopGuessedEnemyCards(
                        this.FirstCard,
                        this.SecondCard,
                        this.enemyCardsPrediction.ToList());*/
                }
                else
                {
                    // need the info
                    // return PlayerAction.Raise(1);
                }
            }

            if (context.RoundType != GameRoundType.PreFlop)
            {
                this.GetAverageOdds(context.RoundType);
                /*if (context.RoundType == GameRoundType.Flop && oddsForThisRound.Count > 2)
                {
                    var old = HandPotentialValuation.HandPotentialMonteCarloApproximation(
                    this.FirstCard,
                    this.SecondCard,
                    this.CommunityCards,
                    250);
                    var old2 = HandPotentialValuation.GetHandPotentialMonteCarloApproximation2(
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
                PlayerAction enemyLastAction = context.PreviousRoundActions.Last().Action;

                if (enemyAlwaysRise && enemyLastAction.Type != PlayerActionType.Raise && enemyMoney > 0)
                {
                    enemyAlwaysRise = false;
                    enemyAlwaysAllIn = false;
                }

                if (enemyAlwaysCall && enemyLastAction.Type != PlayerActionType.CheckCall && enemyMoney > 0)
                {
                    enemyAlwaysCall = false;
                }

                if (enemyAlwaysAllIn && enemyLastAction.Money < enemyMoney && enemyLastAction.Money > context.SmallBlind)
                {
                    enemyAlwaysAllIn = false;
                }

                // increase on check or call because that works best somehow
                if (!enemyAlwaysRise && !enemyAlwaysAllIn)
                {
                    if (enemyLastAction.Type == PlayerActionType.Raise)
                    {
                        if (!betsForBlinds.ContainsKey(context.SmallBlind))
                        {
                            betsForBlinds[context.SmallBlind] = new List<int>();
                        }

                        var recentBets = betsForBlinds[context.SmallBlind];

                        if (recentBets.Count >= 30)
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

                        if (enemyLastAction.Money > realBetThreshhold)
                        {
                            recentBets.Add(enemyLastAction.Money);
                        }
                    }
                    else if (enemyAlwaysCall && enemyLastAction.Type == PlayerActionType.CheckCall)
                    {
                        if (enemyLastAction.Money == 0)
                        {
                            ods += .1f;
                        }

                        // enemy action for call always has 0 money
                        /*else if (enemyLastAction.Money < realBetThreshhold)
                        {
                            ods += .05f;
                        }*/
                    }
                }
            }

            // AAI: Artificial AntiInteligence (may defeat some statistics)
            if (this.aintiAIReverseLogickCoeficient == 40 && this.victories < this.defeats)
            {
                ods = 1 - ods;
                if (ods < .5)
                {
                    return PlayerAction.CheckOrCall();
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

            /*if (enemyAlwaysAllIn && ods >= .8)
            {
                PlayerAction.Raise(context.SmallBlind);
            }*/

            if (ods >= .8) //// Recommended
            {
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