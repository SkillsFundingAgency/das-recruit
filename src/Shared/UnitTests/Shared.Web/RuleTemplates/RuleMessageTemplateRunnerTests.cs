using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.RuleTemplates
{
    public class RuleMessageTemplateRunnerTests
    {
        [Fact]
        public void ShouldCreateProfanityCheckMessage()
        {
            var sut = new RuleMessageTemplateRunner();

            var actual = sut.ToText(RuleId.ProfanityChecks, "{\"Profanity\":\"drat\",\"Occurrences\":1}", "Profanity field");
            actual.Should().Be("Profanity field contains the phrase 'drat'");

            actual = sut.ToText(RuleId.ProfanityChecks, "{\"Profanity\":\"drat\",\"Occurrences\":3}", "Profanity field");
            actual.Should().Be("Profanity field contains the phrase 'drat' 3 times");
        }

        [Fact]
        public void ShouldCreateBannedPhraseCheckMessage()
        {
            var sut = new RuleMessageTemplateRunner();

            var actual = sut.ToText(RuleId.BannedPhraseChecks, "{\"BannedPhrase\":\"driving licence\",\"Occurrences\":1}", "Banned phrase field");
            actual.Should().Be("Banned phrase field contains the phrase 'driving licence'");

            actual = sut.ToText(RuleId.BannedPhraseChecks, "{\"BannedPhrase\":\"driving licence\",\"Occurrences\":3}", "Banned phrase field");
            
            actual.Should().Be("Banned phrase field contains the phrase 'driving licence' 3 times");
        }
    }
}
