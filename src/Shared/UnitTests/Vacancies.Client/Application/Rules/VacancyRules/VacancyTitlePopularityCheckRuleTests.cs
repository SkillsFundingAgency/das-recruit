using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Rules.VacancyRules
{
    public class VacancyTitlePopularityCheckRuleTests
    {
        private const string TrainingLarsCode = "490";
        private const string ApprenticeTitle1 = "Administration Apprentice";
        private const string ApprenticeTitle2 = "Business Apprentice";
        private const string ApprenticeTitle3 = "Business Administration Apprentice";
        private QaRulesConfiguration _qaRulesConfig = new QaRulesConfiguration
        {
            TitlePopularityPercentageThreshold = 10
        };
        private readonly Mock<IApprenticeshipProgrammeProvider> _mockApprenticeshipProgrammesProvider;
        private readonly Mock<IGetTitlePopularity> _mockGetTitlePopularityService;

        public VacancyTitlePopularityCheckRuleTests()
        {
            _mockApprenticeshipProgrammesProvider = new Mock<IApprenticeshipProgrammeProvider>();
            _mockGetTitlePopularityService = new Mock<IGetTitlePopularity>();

            _mockApprenticeshipProgrammesProvider
            .Setup(x => x.GetApprenticeshipProgrammeAsync(TrainingLarsCode))
            .ReturnsAsync(new ApprenticeshipProgramme
            {
                Id = TrainingLarsCode,
                ApprenticeshipType = TrainingType.Framework,
                Title = "Business and Administration: Business and Administration"
            });

            _mockGetTitlePopularityService
            .Setup(x => x.GetTitlePopularityAsync(TrainingLarsCode, ApprenticeTitle1))
            .ReturnsAsync(0);
            _mockGetTitlePopularityService
            .Setup(x => x.GetTitlePopularityAsync(TrainingLarsCode, ApprenticeTitle2))
            .ReturnsAsync(9);
            _mockGetTitlePopularityService
            .Setup(x => x.GetTitlePopularityAsync(TrainingLarsCode, ApprenticeTitle3))
            .ReturnsAsync(15);
        }

        [Fact]
        public void WhenCreated_ItShouldReturnBasicInformationAboutTheRule()
        {
            var rule = new VacancyTitlePopularityCheckRule(_mockApprenticeshipProgrammesProvider.Object, _mockGetTitlePopularityService.Object, _qaRulesConfig);
            rule.RuleId.Should().Be(RuleId.TitlePopularity);
        }

        [Fact]
        public async Task WhenInvoked_ItShouldSetTheCorrectDetailsDataType()
        {
            var rule = new VacancyTitlePopularityCheckRule(_mockApprenticeshipProgrammesProvider.Object, _mockGetTitlePopularityService.Object, _qaRulesConfig);

            var entity = TestVacancyBuilder
                        .Create()
                        .SetTitle(ApprenticeTitle1)
                        .SetTrainingProgrammeId(TrainingLarsCode);

            var outcome = await rule.EvaluateAsync(entity);

            outcome.Details.First().Data.Should().BeOfType(typeof(string));
        }

        [Theory]
        [InlineData(ApprenticeTitle1, 100)]
        [InlineData(ApprenticeTitle2, 100)]
        [InlineData(ApprenticeTitle3, 0)]
        public async Task WhenInvoked_ItShouldReturnTheExpectedScore(string apprenticeshipTitle, int expectedScore)
        {
            var rule = new VacancyTitlePopularityCheckRule(_mockApprenticeshipProgrammesProvider.Object, _mockGetTitlePopularityService.Object, _qaRulesConfig);

            var entity = TestVacancyBuilder
                        .Create()
                        .SetTitle(apprenticeshipTitle)
                        .SetTrainingProgrammeId(TrainingLarsCode);

            var outcome = await rule.EvaluateAsync(entity);

            outcome.Score.Should().Be(expectedScore);
        }
    }
}