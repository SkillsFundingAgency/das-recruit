using FluentAssertions;
using System.Text.RegularExpressions;
using Esfa.Recruit.Shared.Web.Configuration;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Configuration;

public class EnvironmentNamesTests
{
    [Test]
    public void GetNonProdEnvironmentNamesShouldNotIncludeProd()
    {
        Regex.IsMatch(EnvironmentNames.GetNonProdEnvironmentNamesCommaDelimited(), @"(?<!PRE)PROD\b").Should().BeFalse();
    }

    [Test]
    public void GetNonProdEnvironmentNamesShouldIncludeEveryNonProdEnvironment()
    {
        EnvironmentNames.GetNonProdEnvironmentNamesCommaDelimited().Should().Be("DEVELOPMENT,AT,TEST,TEST2,DEMO,PREPROD");
    }
}
