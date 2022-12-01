using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Rules.VacancyRules
{
    public class VacancyBannedPhraseChecksRuleTest
    {
        private readonly IBannedPhrasesProvider _bannedPhrasesProvider;
        private readonly IEnumerable<string> _bannedPhrases = new[] { "school", "schools", "driving license", "over 18" };
        public VacancyBannedPhraseChecksRuleTest()
        {
            var mockBannedPhraseProvider = new Mock<IBannedPhrasesProvider>();
            mockBannedPhraseProvider.Setup(b => b.GetBannedPhrasesAsync()).Returns(Task.FromResult(_bannedPhrases));
            _bannedPhrasesProvider = mockBannedPhraseProvider.Object;
        }

        [Fact]
        public void ShouldIgnoreNonAlphaNumericCharacters()
        {
             var sut = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
            var vacancy = new Fixture()
                .Build<Vacancy>()
                .With(v => v.Title,
                    @"Dolore school leavers �school"" leaver 5pm, shifts, may work evenings and weekends school") 
                .Create();

            var result = sut.EvaluateAsync(vacancy).Result;

            result.Details.First(d => d.Target == "Title");
            result.Score.Should().Be(100);
        }

        [Theory]
        [InlineData("a driving license is required", 1)]
        [InlineData("driving - license required", 1)]
        [InlineData("*driving*license* is required", 1)]
        [InlineData("driving license", 100, 100)]
        [InlineData("driving license, driving license, driving license", 100, 100)]
        [InlineData("driving license over 18", 100, 100)]
        [InlineData("*driving \t \n license* is required", 1)]
        [InlineData("occasionally there may be some driving. licensees are required to like beer", 0)]
        [InlineData("a great mover, 18 moves at least!", 0)]
        [InlineData("school", 1)]
        public void WhenInvoked_ItShouldReturnTheExpectedScore(string phrase, int expectedScore,
            decimal weighting = 1.0m)
        {
            var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider, ConsolidationOption.ConsolidateByField, weighting);
            var entity = TestVacancyBuilder.Create().SetDetails(phrase, string.Empty);

            var outcome = rule.EvaluateAsync(entity).Result;

            outcome.Score.Should().Be(expectedScore);
        }

        [Fact]
        public void ShouldReturnAnOverallUnconsolidatedNarrative()
        {
            var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
            var skills = new[] { "Juggling", "Running", "driving license" };
            var qualifications = new List<Qualification> { new Qualification{Grade = "over 18 grade", Subject = "subject"} };
            var entity = TestVacancyBuilder.Create()
                .SetDetails("driving - license required", "must be over 18 years of age and hold a clean driving license, yes a driving license!")
                .SetSkills(skills)
                .SetQualifications(qualifications);

            var outcome = rule.EvaluateAsync(entity).Result;

            outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'Title'");
            outcome.Narrative.Should().Contain("Banned phrase 'driving license' found 2 times in 'Description'");
            outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'Skills'");
            outcome.Narrative.Should().Contain("Banned phrase 'over 18' found in 'Description'");
            outcome.Narrative.Should().Contain("Banned phrase 'over 18' found in 'Qualifications'");
        }

        [Fact]
        public void ShouldReturnAnOverallConsolidatedNarrative()
        {
            var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider, ConsolidationOption.ConsolidateByField);
            var skills = new[] { "Juggling", "Running", "driving license" };
            var entity = TestVacancyBuilder.Create()
                .SetDetails("driving - license required", "must be over 18 years of age and hold a clean driving license, yes a driving license!")
                .SetSkills(skills);

            var outcome = rule.EvaluateAsync(entity).Result;

            outcome.Narrative.Should().Contain("1 banned phrases 'driving license' found in 'Title'");
            outcome.Narrative.Should().Contain("3 banned phrases 'driving license,over 18' found in 'Description'");
            outcome.Narrative.Should().Contain("1 banned phrases 'driving license' found in 'Skills'");
        }

        [Fact]
        public void WhenInvoked_ItShouldIncludeDetailsOfEachFieldOutcome()
        {
            var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
            var skills = new[] { "Juggling", "Running", "driving license" };
            var entity = TestVacancyBuilder.Create()
                .SetDetails("an apprenticeship", "must be over 18 years of age and hold a clean driving license")
                .SetSkills(skills);

            var outcome = rule.EvaluateAsync(entity).Result;

            outcome.HasDetails.Should().BeTrue();

            Assert.All(outcome.Details, a =>
            {
                a.Target.Should().NotBeEmpty();
                a.Narrative.Should().NotBeEmpty();
            });
        }
    }
}