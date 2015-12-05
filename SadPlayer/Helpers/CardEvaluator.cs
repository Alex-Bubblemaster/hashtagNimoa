namespace SadPlayer.Helpers
{
    using System;
    using TexasHoldem.Logic.Cards;

    public class CardEvaluator
    {
        public static double CalculateStrength(Card one, Card two)
        {
            double pp = 0;

            switch (one.Type)
            {
                case CardType.Jack:
                    pp = 6;
                    break;
                case CardType.Queen:
                    pp = 7;
                    break;
                case CardType.King:
                    pp = 8;
                    break;
                case CardType.Ace:
                    pp = 10;
                    break;
                default:
                    pp = (int)one.Type / (double)2;
                    break;
            }

            double pp2 = 0;
            switch (two.Type)
            {
                case CardType.Jack:
                    pp2 = 6;
                    break;
                case CardType.Queen:
                    pp2 = 7;
                    break;
                case CardType.King:
                    pp2 = 8;
                    break;
                case CardType.Ace:
                    pp2 = 10;
                    break;
                default:
                    pp2 = (int)one.Type / (double)2;
                    break;
            }

            var points = Math.Max(pp, pp2);

            if (one.Type == two.Type)
            {
                points *= 2;
                if (points < 5)
                {
                    points = 5;
                }
            }

            if (one.Suit == two.Suit)
            {
                points += 2;
            }

            if (Math.Abs(one.Type - two.Type) == 1)
            {
                points++;
            }

            if (Math.Abs(one.Type - two.Type) == 2)
            {
                points--;
            }

            if (Math.Abs(one.Type - two.Type) == 3)
            {
                points -= 2;
            }

            if (Math.Abs(one.Type - two.Type) == 4)
            {
                points -= 4;
            }

            if (Math.Abs(one.Type - two.Type) >= 5)
            {
                points -= 5;
            }

            return points;
        }
    }
}
