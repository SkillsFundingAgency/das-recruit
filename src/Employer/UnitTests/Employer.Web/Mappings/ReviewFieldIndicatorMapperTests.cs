using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Mappings
{
    public class ReviewFieldIndicatorMapperTests
    {
        [Fact]
        public void ShouldMapApprenticeship()
        {
            var shortDescriptionProfanityCheckRuleOutcome =
                new RuleOutcome(
                    RuleId.ProfanityChecks,
                    100,
                    "Profanity 'drat' found in 'ShortDescription'",
                    "ShortDescription",
                    new List<RuleOutcome>(), "{\"Profanity\" : \"drat\",\"Occurrences\" : 1}");

            var titleProfanityCheckRuleOutcome = new RuleOutcome(
                RuleId.ProfanityChecks,
                0,
                "No profanities found in 'Title'",
                "Title");

            var profanityChecksRuleOutcome = new RuleOutcome(
                RuleId.ProfanityChecks, 
                100,
                "No profanities found in 'Title', Profanity 'drat' found in 'ShortDescription', etc", 
                "",
                new List<RuleOutcome>
                    {
                        shortDescriptionProfanityCheckRuleOutcome,
                        titleProfanityCheckRuleOutcome
                    }
                );

            var sut = new ReviewFieldIndicatorMapper(new RuleMessageTemplateRunner());

            var review = new VacancyReview
            {
                ManualQaFieldIndicators = new List<ManualQaFieldIndicator>
                {
                    new ManualQaFieldIndicator
                    {
                        IsChangeRequested = false,
                        FieldIdentifier = FieldIdentifiers.Title
                    },
                    new ManualQaFieldIndicator
                    {
                        IsChangeRequested = true,
                        FieldIdentifier = FieldIdentifiers.ShortDescription
                    }
                },
                AutomatedQaOutcomeIndicators = new List<RuleOutcomeIndicator>
                {
                    new RuleOutcomeIndicator
                    {
                        IsReferred = false,
                        RuleOutcomeId = titleProfanityCheckRuleOutcome.Id
                    },
                    new RuleOutcomeIndicator
                    {
                        IsReferred = true,
                        RuleOutcomeId = shortDescriptionProfanityCheckRuleOutcome.Id
                    }
                },
                AutomatedQaOutcome = new RuleSetOutcome {RuleOutcomes = new List<RuleOutcome> {profanityChecksRuleOutcome}},
                VacancySnapshot = new Vacancy()
            };

            var vm = sut.MapFromFieldIndicators(ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators(), review).ToList();

            vm.Count.Should().Be(1);

            var shortDescription = vm.Single(v => v.ReviewFieldIdentifier == FieldIdentifiers.ShortDescription);

            shortDescription.ManualQaText.Should().NotBeNullOrEmpty();

            shortDescription.AutoQaTexts.Count.Should().Be(1);
            shortDescription.AutoQaTexts[0].Should().Be("Brief overview contains the phrase 'drat'");
        }
        
        [Fact]
        public void ShouldMapTraineeship()
        {
            var shortDescriptionProfanityCheckRuleOutcome =
                new RuleOutcome(
                    RuleId.ProfanityChecks,
                    100,
                    "Profanity 'drat' found in 'ShortDescription'",
                    "ShortDescription",
                    new List<RuleOutcome>(), "{\"Profanity\" : \"drat\",\"Occurrences\" : 1}");

            var titleProfanityCheckRuleOutcome = new RuleOutcome(
                RuleId.ProfanityChecks,
                0,
                "No profanities found in 'Title'",
                "Title");

            var profanityChecksRuleOutcome = new RuleOutcome(
                RuleId.ProfanityChecks, 
                100,
                "No profanities found in 'Title', Profanity 'drat' found in 'ShortDescription', etc", 
                "",
                new List<RuleOutcome>
                    {
                        shortDescriptionProfanityCheckRuleOutcome,
                        titleProfanityCheckRuleOutcome
                    }
                );

            var sut = new ReviewFieldIndicatorMapper(new RuleMessageTemplateRunner());

            var review = new VacancyReview
            {
                ManualQaFieldIndicators = new List<ManualQaFieldIndicator>
                {
                    new ManualQaFieldIndicator
                    {
                        IsChangeRequested = false,
                        FieldIdentifier = FieldIdentifiers.Title
                    },
                    new ManualQaFieldIndicator
                    {
                        IsChangeRequested = true,
                        FieldIdentifier = FieldIdentifiers.ShortDescription
                    }
                },
                AutomatedQaOutcomeIndicators = new List<RuleOutcomeIndicator>
                {
                    new RuleOutcomeIndicator
                    {
                        IsReferred = false,
                        RuleOutcomeId = titleProfanityCheckRuleOutcome.Id
                    },
                    new RuleOutcomeIndicator
                    {
                        IsReferred = true,
                        RuleOutcomeId = shortDescriptionProfanityCheckRuleOutcome.Id
                    }
                },
                AutomatedQaOutcome = new RuleSetOutcome {RuleOutcomes = new List<RuleOutcome> {profanityChecksRuleOutcome}},
                VacancySnapshot = new Vacancy{VacancyType = VacancyType.Traineeship}
            };

            var vm = sut.MapFromFieldIndicators(ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators(), review).ToList();

            vm.Count.Should().Be(1);

            var shortDescription = vm.Single(v => v.ReviewFieldIdentifier == FieldIdentifiers.ShortDescription);

            shortDescription.ManualQaText.Should().NotBeNullOrEmpty();

            shortDescription.AutoQaTexts.Count.Should().Be(1);
            shortDescription.AutoQaTexts[0].Should().Be("Brief overview contains the phrase 'drat'");
        }
    }
}
