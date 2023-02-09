using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Rules.VacancyRules
{
    public class VacancyProfanityChecksRuleTests
    {
        [Fact]
        public void WhenCreated_ItShouldReturnBasicInformationAboutTheRule()
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());

            Assert.Equal(RuleId.ProfanityChecks, rule.RuleId);
        }

        [Theory]
        [InlineData("nothing", 0)]
        [InlineData("bother-bother, bother!", 3)]
        [InlineData("bother dang", 2)]
        [InlineData("drat", 1)]
        [InlineData("hydrate", 0)]
        [InlineData("dangleberry", 0)]
        [InlineData("bother", 100, 100)]
        [InlineData("bother dang", 100, 100)]
        public async Task WhenInvoked_ItShouldReturnTheExpectedScore(string phrase, int expectedScore, decimal weighting = 1.0m)
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider(), ConsolidationOption.ConsolidateByField, weighting);
            var entity = TestVacancyBuilder.Create().SetTitle(phrase);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.Equal(expectedScore, outcome.Score);
        }

        [Fact]
        public async Task WhenInvoked_ItShouldReturnAnOverallUnconsolidatedNarrative()
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
            var skills = new[] { "Juggling", "Running", "dang" };
            var qualifications = new List<Qualification> { new Qualification{Grade = "dang grade", Subject = "subject"} };
            var entity = TestVacancyBuilder.Create()
                .SetTitle("bother-bother, bother!")
                .SetDescription("dang it and balderdash!!")
                .SetSkills(skills)
                .SetQualifications(qualifications);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.Contains("Profanity 'bother' found 3 times in 'Title'", outcome.Narrative);
            Assert.Contains("Profanity 'dang' found in 'Description'", outcome.Narrative);
            Assert.Contains("Profanity 'balderdash' found in 'Description'", outcome.Narrative);
            Assert.Contains("Profanity 'dang' found in 'Skills'", outcome.Narrative);
            Assert.Contains("Profanity 'dang' found in 'Qualifications'", outcome.Narrative);
        }

        [Fact]
        public async Task WhenInvoked_ItShouldReturnAnOverallConsolidatedNarrative()
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider(), ConsolidationOption.ConsolidateByField);
            var skills = new[] { "Juggling", "Running", "dang" };
            var entity = TestVacancyBuilder.Create()
                .SetTitle("bother-bother, bother!")
                .SetDescription("dang it and balderdash!!")
                .SetSkills(skills);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.Contains("3 profanities 'bother' found in 'Title'", outcome.Narrative);
            Assert.Contains("2 profanities 'dang,balderdash' found in 'Description'", outcome.Narrative);
            Assert.Contains("1 profanities 'dang' found in 'Skills'", outcome.Narrative);
        }

        [Fact]
        public async Task WhenInvoked_ItShouldIncludeDetailsOfEachFieldOutcome()
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
            var skills = new[] { "Juggling", "Running", "dang" };
            var entity = TestVacancyBuilder.Create()
                .SetTitle("bother-bother, bother!")
                .SetDescription("dang it and balderdash!!")
                .SetSkills(skills);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.True(outcome.HasDetails);
            Assert.Equal(19, outcome.Details.Count());

            Assert.All(outcome.Details, a =>
            {
                Assert.NotEmpty(a.Target);
                Assert.NotEmpty(a.Narrative);
                Assert.Equal(RuleId.ProfanityChecks, a.RuleId);
            });
        }
    }

    public class TestProfanityListProvider : IProfanityListProvider
    {
        public Task<IEnumerable<string>> GetProfanityListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "bother", "dang", "balderdash", "drat" }) ;
        }
    }
}
