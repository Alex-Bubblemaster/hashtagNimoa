# hashtagNimoa - Poker AI

<img src="http://i.ebayimg.com/11/!BwOYqQwCGk~$(KGrHqUOKjcEve5QYT4eBMH9WHWGTQ~~_35.JPG" align=left width="120px"/>
<hr/>
We used a Monte Carlo algorithm for this project. A Monte Carlo simulation uses essentially random inputs (within realistic limits) to model the game and produces probable outcomes.
Depending on the time that we are given for each turn, we can adjust the amount of random card combinations that are calculated and take it's representation for plausible probability.

## Preflop

The HandStrengthValuation class takes care of the preflop round by using StartingHandsOdds matrix represented by floats and the behaviour of the player is dependent on the outcome.

## Flop

The HandPotentialValuation class has a method HandPotentialMonteCarloApproximation that currently works with 250(roughly a quarter of the actual possibilities) and performs very well for time fitting each turn in 0.1s. It takes the NimoaPlayer cards as well as the community cards and returns the chances of winning during flop. The result is returned as a float. 

## Future development

The Monte Carlo algorithm can easily be converted to a more reliable LasVegas algorithm. We would need to use more resources in order to store the outcome of the Monte Carlo method.
