namespace TexasHoldem.Tests.Player
{
    using System.Collections.Generic;

    using NUnit.Framework;
    using AI.NimoaPlayer.Helpers;
    using System.Linq;

    [TestFixture]
    public class CardsCombinationsTest
    {
        [Test]
        public void GenerateRandomCardsShouldGenerateUniqueCombinations()
        {
            var randomCombinations = CardsCombinations.GetRandomCardsIndexes(20, 2, 5);
            var uniqueCombinations=new HashSet<string>(randomCombinations.Select(x => string.Join(", ", x)));

            Assert.AreEqual(randomCombinations.Count, uniqueCombinations.Count);

            foreach (var randomCombination in randomCombinations)
            {
                Assert.AreNotEqual(randomCombination[0], randomCombination[1]);
            }
        }
    }
}
