﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Rules.VacancyRules
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

            var entity = TestVacancyBuilder.Create().SetDetails(phrase, string.Empty);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.Equal(expectedScore, outcome.Score);
        }

        [Fact]
        public async Task WhenInvoked_ItShouldReturnAnOverallUnconsolidatedNarrative()
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());

            var skills = new[] { "Juggling", "Running", "dang" };

            var entity = TestVacancyBuilder.Create()
                .SetDetails("bother-bother, bother!", "dang it and balderdash!!")
                .SetSkills(skills);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.Contains("Profanity 'bother' found 3 times in 'Title'", outcome.Narrative);
            Assert.Contains("Profanity 'dang' found in 'Description'", outcome.Narrative);
            Assert.Contains("Profanity 'balderdash' found in 'Description'", outcome.Narrative);
            Assert.Contains("Profanity 'dang' found in 'Skills'", outcome.Narrative);
        }

        [Fact]
        public async Task WhenInvoked_ItShouldReturnAnOverallConsolidatedNarrative()
        {
            var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider(), ConsolidationOption.ConsolidateByField);

            var skills = new[] { "Juggling", "Running", "dang" };

            var entity = TestVacancyBuilder.Create()
                .SetDetails("bother-bother, bother!", "dang it and balderdash!!")
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
                .SetDetails("bother-bother, bother!", "dang it and balderdash!!")
                .SetSkills(skills);

            var outcome = await rule.EvaluateAsync(entity);

            Assert.True(outcome.HasDetails);
            Assert.Equal(18, outcome.Details.Count());

            Assert.All(outcome.Details, a =>
            {
                Assert.NotEmpty(a.Target);
                Assert.NotEmpty(a.Narrative);
                Assert.Equal(RuleId.ProfanityChecks, a.RuleId);
            });
        }
    }

    internal static class TestVacancyBuilder
    {
        internal static Vacancy Create()
        {
            return new Vacancy()
            {
                EmployerLocation = new Address(),
                Skills = new List<string>(),
                Qualifications = new List<Qualification>(),
                Wage = new Wage()
            };
        }

        internal static Vacancy SetVacancyReference(this Vacancy entity, long vacancyReference)
        {
            entity.VacancyReference = vacancyReference;

            return entity;
        }

        internal static Vacancy SetDetails(this Vacancy entity, string title, string description)
        {
            entity.Title = title;
            entity.Description = description;

            return entity;
        }

        internal static Vacancy SetSkills(this Vacancy entity, IEnumerable<string> skills)
        {
            entity.Skills = skills.ToList();

            return entity;
        }
    }

    internal class TestProfanityListProvider : IProfanityListProvider
    {
        public Task<IEnumerable<string>> GetProfanityListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "bother", "dang", "balderdash", "drat" }) ;
        }
    }
}
