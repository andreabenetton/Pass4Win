namespace Pass4Win.Tests
{
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// The pwgen tests.
    /// </summary>
    [TestFixture()]
    public class PwgenTests
    {
      
        [Test()]
        public void GeneratePasswordWithTenCharactersTest()
        {
            string result = Pwgen.Generate(10);
            result.Length.ShouldBe(10);
        }

        [Test()]
        public void GeneratePasswordWithEightyCharactersTest()
        {
            string result2 = Pwgen.Generate(80);
            result2.Length.ShouldBe(80);
        }
    }
}