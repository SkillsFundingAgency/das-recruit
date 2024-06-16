using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Recruit.Vacancies.Client.Infrastructure.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase("", 0)]
        [TestCase(" ", 0)]
        [TestCase("this", 1)]
        [TestCase("this is a test", 4)]
        [TestCase(" this  is, a test ", 4)]
        public void GetWordCount_Returns_Expected_Value(string input, int expected)
        {
            input.GetWordCount().Should().Be(expected);
        }
    }
}
