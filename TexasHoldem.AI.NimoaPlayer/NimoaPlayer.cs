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

        private IList<Card[]> enemyCardsPredictionWorstCase;

        private int enemyCardsPredictionHits;

        private int enemyCardsPredictionMisses;
        private bool enemyRising;

        private string CurrentName { get; set; } = "NimoaPlayer" + Guid.NewGuid();

        public override string Name
        {
            get
            {
                return this.CurrentName;
            }
        }

        public override void StartGame(StartGameContext context)
        {
            var newEnemyName = context.PlayerNames.FirstOrDefault(n => n != this.Name);
            //this.CurrentName = "NimoaPlayer_" + Guid.NewGuid();
            base.StartGame(context);
            gamesCount++;
            startMoney = context.StartMoney;
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
                EnemyPredictions.MaxCallThreshold = .4f;
                EnemyPredictions.MinBetThreshold = .7f;
            }
        }

        public override void StartHand(StartHandContext context)
        {
            base.StartHand(context);
            handNumber = context.HandNumber;
            realBetThreshhold = context.SmallBlind * 2;
            this.thisHandEnemyRising = new Dictionary<GameRoundType, bool>();
            this.enemyCardsPrediction = null;
            this.enemyRising = false;
        }

        public override void StartRound(StartRoundContext context)
        {
            this.aintiAIReverseLogickCoeficient = RandomProvider.Next(0, 100);
            this.currentRoundType = context.RoundType;

            if (context.MoneyLeft == 0) //  || enemyMoney == 0
            {
                return;
            }

            oddsForThisRound = new List<float>();

            this.currentRoundType = context.RoundType;
            oddsForThisRound = new List<float>();

            if (context.RoundType == GameRoundType.PreFlop)
            {
                roundOdds = HandStrengthValuation.PreFlopOdsLookupTable(this.FirstCard, this.SecondCard);
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
            var predictions = this.enemyCardsPrediction;
            if (predictions == null)
            {
                predictions = this.enemyCardsPredictionWorstCase;
            }

            float ods;
            if (!(enemyAlwaysAllIn || enemyAlwaysRise) && predictions != null)
            {
                if (roundType == GameRoundType.Flop || roundType == GameRoundType.Turn)
                {
                    // Approximation
                    ods = HandPotentialValuation.GetHandPotentialMonteCarloApproximation3(
                        this.FirstCard,
                        this.SecondCard,
                        this.CommunityCards,
                        predictions,
                        250);
                }
                else
                {
                    ods = HandStrengthValuation.HandStrengthMonteCarloApproximation2(
                        this.FirstCard,
                        this.SecondCard,
                        this.CommunityCards,
                        predictions,
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
            this.CurrentName = "NimoaPlayer_" + Guid.NewGuid();
            base.EndGame(context);

            if (context.WinnerName == this.Name)
            {
                this.victories++;
            }
            else
            {
                this.defeats++;
            }
        }

        public override void EndHand(EndHandContext context)
        {
            if (context.ShowdownCards.Count > 0)
            {
                //var enemyCards = context.ShowdownCards.FirstOrDefault(n => !n.Key.Contains("NimoaPlayer")).Value.ToList();
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

                    var enemyOdds = HandStrengthValuation.PreFlopOdsLookupTable(enemyCards[0], enemyCards[1]);

                    if (this.enemyRising)
                    {
                        EnemyPredictions.MinBetThreshold = Math.Min(EnemyPredictions.MinBetThreshold, enemyOdds);
                    }
                    else
                    {
                        EnemyPredictions.MaxCallThreshold = Math.Max(EnemyPredictions.MaxCallThreshold, enemyOdds);
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

        private PlayerAction previousRoundLastEnemyAction;
        public override void EndRound(EndRoundContext context)
        {
            var enemyActions = context.RoundActions.Where(a => a.PlayerName == enemyName).ToList();
            if (enemyActions.Count > 1)
            {
                var enemyLastAction = enemyActions[enemyActions.Count - 1].Action;

                if (enemyLastAction == PlayerAction.Fold())
                {
                    enemyAlwaysRise = false;
                    enemyAlwaysAllIn = false;
                    enemyAlwaysCall = false;

                    return;
                }

                previousRoundLastEnemyAction = enemyLastAction;

                if (currentRoundType == GameRoundType.PreFlop
                && !(enemyAlwaysAllIn || enemyAlwaysCall || enemyAlwaysRise))
                {
                    var enemyFirstPreFlopAction = enemyActions[1].Action;

                    if (enemyFirstPreFlopAction.Type == PlayerActionType.Raise)
                    {
                        enemyRising = true;
                    }
                    else
                    {
                        enemyRising = false;
                    }

                    enemyCardsPrediction = EnemyPredictions.PredictEnemyCardsPreFlop(
                        this.FirstCard,
                        this.SecondCard,
                        enemyFirstPreFlopAction);
                }
            }
        }

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            var enemyMoney = startMoney * 2 - context.MoneyLeft - context.CurrentPot;
            if ((enemyAlwaysAllIn || enemyAlwaysCall || enemyAlwaysRise) && context.PreviousRoundActions.Count > 2)
            {
                PlayerAction enemyLastAction = context.PreviousRoundActions.Last().Action;

                if ((enemyAlwaysRise || enemyAlwaysAllIn) && enemyLastAction.Type != PlayerActionType.Raise
                    && enemyMoney > 0)
                {
                    enemyAlwaysRise = false;
                    enemyAlwaysAllIn = false;
                }

                if (enemyAlwaysRise && enemyLastAction.Money != context.SmallBlind && enemyMoney > 0)
                {
                    enemyAlwaysRise = false;
                }

                if (enemyAlwaysCall && enemyLastAction.Type != PlayerActionType.CheckCall)
                {
                    enemyAlwaysCall = false;
                }

                // TODO: if ! moneyleft - false
                if (enemyAlwaysAllIn && enemyLastAction.Type == PlayerActionType.Raise && enemyMoney > 0)
                {
                    enemyAlwaysAllIn = false;
                }

                //TODO: detect variable raise strategy
            }
            else if (context.RoundType != GameRoundType.PreFlop && this.previousRoundLastEnemyAction != null && this.previousRoundLastEnemyAction.Type == PlayerActionType.CheckCall && enemyMoney > 0)
            {
                enemyAlwaysAllIn = false;
                enemyAlwaysRise = false;
            }

            if (context.MoneyLeft == 0)
            {
                return PlayerAction.CheckOrCall();
            }

            /*if (context.RoundType == GameRoundType.PreFlop && context.PreviousRoundActions.Count >= 3 && context.PreviousRoundActions.Count < 5)
            {
                var enemyActions = context.PreviousRoundActions.Where(a => a.PlayerName == this.enemyName).ToList();
                // remove entry fee
                enemyActions.RemoveAt(0);
                var enemyFirstPreFlopAction = enemyActions.FirstOrDefault().Action; // && a.Action.Money > context.SmallBlind

                // assume worst case
                /*this.enemyCardsPrediction = EnemyPredictions.PredictEnemyCardsPreFlop(
                        this.FirstCard,
                        this.SecondCard,
                        PlayerAction.Raise(1));#1#

                if (enemyFirstPreFlopAction != null)
                {
                    /*this.enemyCardsPrediction = EnemyPredictions.PredictEnemyCardsPreFlop(
                        this.FirstCard,
                        this.SecondCard,
                        enemyFirstPreFlopAction);#1#

                    /*roundOdds = HandStrengthValuation.HandStrengthPreFlopGuessedEnemyCards(
                        this.FirstCard,
                        this.SecondCard,
                        this.enemyCardsPrediction.ToList());#1#
                }
                /*else
                {
                    // need the info
                    return PlayerAction.Raise(1);
                }#1#
            }*/

            if (context.RoundType != GameRoundType.PreFlop)
            {
                this.GetAverageOdds(context.RoundType);
            }

            var ods = roundOdds;

            if (context.PreviousRoundActions.Count > 0)
            {
                // TODO: enemy first action this round
                PlayerAction enemyLastAction = context.PreviousRoundActions.Last().Action;

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
                            //TODO: distinguish check and call
                        }

                        if (enemyLastAction.Money > realBetThreshhold)
                        {
                            recentBets.Add(enemyLastAction.Money);
                        }
                    }
                    else if (enemyAlwaysCall && enemyLastAction.Type == PlayerActionType.CheckCall)// && enemyMoney>0
                    {
                        if (enemyLastAction.Money == 0)
                        {
                            ods += .1f;
                        }
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

            if (enemyAlwaysAllIn)
            {
                return AntiAllInStrategy.GetPlayerAction(ods, enemyMoney, context);
            }

            if (enemyAlwaysCall || enemyAlwaysRise)
            {
                return AntiDumPlayerStrategy.GetPlayerAction(ods, enemyMoney, context);
            }

            return CautiousStrategy.GetPlayerAction(ods, enemyMoney, context);
        }
    }
}