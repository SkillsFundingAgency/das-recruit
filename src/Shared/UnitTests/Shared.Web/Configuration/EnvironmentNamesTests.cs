using Esfa.Recruit.Shared.Configuration;
using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Configuration
{
    public class EnvironmentNamesTests
    {
        [Fact]
        public void GetNonProdEnvironmentNamesShouldNotIncludeProd()
        {
            Regex.IsMatch(EnvironmentNames.GetNonProdEnvironmentNamesCommaDelimited(), @"(?<!PRE)PROD\b").Should().BeFalse();
        }

        [Fact]
        public void GetNonProdEnvironmentNamesShouldIncludeEveryNonProdEnvironment()
        {
            EnvironmentNames.GetNonProdEnvironmentNamesCommaDelimited().Should().Be("DEVELOPMENT,AT,TEST,DEMO,PREPROD");
        }
    }
}