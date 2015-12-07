# hashtagNimoa - Poker AI

<img src="http://i.ebayimg.com/11/!BwOYqQwCGk~$(KGrHqUOKjcEve5QYT4eBMH9WHWGTQ~~_35.JPG" align=left width="120px"/>
<hr/>
# Algorithms
## Monte Carlo

Monte Carlo simulation uses essentially random inputs (within realistic limits) to model the game and produces probable outcomes.
Depending on the time that we are given for each turn, we can adjust the amount of random card combinations that are calculated and take it's representation for plausible probability. For this competition we generated 250 possibilities for each round. During *River* we could increase this number to 400 and keep the same performance.

## EHS

We used Effective Hand Strength (EHS) and Hand Strength(HS) to a certain extent. The EHS algorithm is developed by computer scientists Darse Billings, Denis Papp, Jonathan Schaeffer and Duane Szafron that has been published for the first time in a research paper (1998). "Opponent Modeling in Poker". The algorithm is a numerical approach to quantify the strength of a poker hand where its result expresses the strength of a particular hand in percentile (i.e. ranging from 0 to 1), compared to all other possible hands. The underlying assumption is that an Effective Hand Strength (EHS) is composed of the current Hand Strength (HS) and its potential to improve or deteriorate. To aid this algorithm we have used a float matrix accessed by the index.

## Generate combinations without repetitions

Borrowed and adapted the approach from http://rosettacode.org/wiki/Combinations

# Strategies/Behaviour

* All our strategies are implemented in GetTurn() method. 
* We calculate the enemy's money to detect any AllInPlayer.
* We compare our money with theirs to produce a better betting behaviour i.e we take the opponent's money as our ceiling bet even when we are playing AllIn.
* Money matters: 

  * The less money we have, the less we bet.
  * We choose a random divisor that varies between 24 and 80 depending on our chances to win the hand.
  * Merit - this is a coefficient we calculate at each turn. Using our odds in the round multiplied by the current pot and divided by the money to CheckOrCall(). The benefits of using this proves risky at time, however it helps return former investments.

## Preflop

The HandStrengthValuation class takes care of the preflop round by using StartingHandsOdds matrix represented by floats and the behaviour of the player is dependent on the outcome.

## Flop

The HandPotentialValuation class has a method HandPotentialMonteCarloApproximation that currently works with 250(roughly a quarter of the actual possibilities) and performs very well for time fitting each turn in 0.1s. It takes the NimoaPlayer cards as well as the community cards and returns the chances of winning during flop. The result is returned as a float. 

## Future development

The Monte Carlo algorithm can easily be converted to a more reliable LasVegas algorithm. We would need to use more resources in order to store the outcome of the Monte Carlo method.
