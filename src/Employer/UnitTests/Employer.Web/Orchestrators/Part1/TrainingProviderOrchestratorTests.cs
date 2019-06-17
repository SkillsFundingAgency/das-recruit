using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class TrainingProviderOrchestratorTests
    {
        private const string EmployerAccountId = "EMPLOYERACCOUNTID";
        private readonly Guid VacancyId = Guid.Parse("0a8e54c9-e284-4023-b688-14b9d841fcd7");
        
        [Fact]
        public async Task PostSelectTrainingProviderAsync_WhenNotChoosingThenRemoveExistingTrainingProviderAndContinue()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                SelectTrainingProvider = false,
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Action.Should().Be(PostSelectTrainingProviderResultAction.TrainingProviderContinue);
            vacancy.TrainingProvider.Should().BeNull();
        }

        [Theory]
        [InlineData("This search won't match a single provider")]
        [InlineData("88888")] //will match multiple providers
        public async Task PostSelectTrainingProviderAsync_TrainingProviderSearch_WhenNoSingleProviderFoundShouldReturnNotFoundResult(string trainingProviderSearch)
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                SelectTrainingProvider = true,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = trainingProviderSearch,
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };
            
            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Action.Should().Be(PostSelectTrainingProviderResultAction.TrainingProviderNotFound);
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_UKPRN_WhenNoProviderFoundShouldReturnNotFoundResult()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                SelectTrainingProvider = true,
                SelectionType = TrainingProviderSelectionType.Ukprn,
                Ukprn = "12345678",
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Action.Should().Be(PostSelectTrainingProviderResultAction.TrainingProviderNotFound);
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_TrainingProviderSearch_WhenSingleProviderFoundShouldReturnConfirmResult()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                SelectTrainingProvider = true,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = "MR EGG 88888888",
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Action.Should().Be(PostSelectTrainingProviderResultAction.TrainingProviderConfirm);
            result.FoundProviderUkprn.Should().Be(88888888);
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_UKPRN_WhenSingleProviderFoundShouldReturnConfirmResult()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                SelectTrainingProvider = true,
                SelectionType = TrainingProviderSelectionType.Ukprn,
                Ukprn = "88888888",
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Action.Should().Be(PostSelectTrainingProviderResultAction.TrainingProviderConfirm);
            result.FoundProviderUkprn.Should().Be(88888888);
        }

        private TrainingProviderOrchestrator GetTrainingProviderOrchestrator(Vacancy vacancy)
        {
            var mockClient = new Mock<IEmployerVacancyClient>();

            var mockRecruitClient = new Mock<IRecruitVacancyClient>();
            mockRecruitClient.Setup(c => c.GetVacancyAsync(VacancyId)).ReturnsAsync(vacancy);

            var mockLog = new Mock<ILogger<TrainingProviderOrchestrator>>();
            var mockReview = new Mock<IReviewSummaryService>();

            var mockCache = new Mock<ICache>();
            Func<Task<IEnumerable<TrainingProviderSuggestion>>> cacheFunc = () =>
            {
                var providers = new List<TrainingProviderSuggestion>
                {
                    new TrainingProviderSuggestion{ProviderName = "MR EGG",Ukprn = 88888888},
                    new TrainingProviderSuggestion{ProviderName = "MRS EGG",Ukprn = 88888889}
                };
                return Task.FromResult(providers.AsEnumerable());
            };
            mockCache.Setup(c => c.CacheAsideAsync(CacheKeys.TrainingProviders, It.IsAny<DateTime>(), It.IsAny<Func<Task<IEnumerable<TrainingProviderSuggestion>>>>()))
                .Returns(Task.FromResult(new List<TrainingProviderSuggestion>
                {
                    new TrainingProviderSuggestion{ProviderName = "MR EGG",Ukprn = 88888888},
                    new TrainingProviderSuggestion{ProviderName = "MRS EGG",Ukprn = 88888889}
                }.AsEnumerable()));
            var timeProvider = new CurrentUtcTimeProvider();

            return new TrainingProviderOrchestrator(mockClient.Object, mockRecruitClient.Object, mockLog.Object, mockReview.Object, mockCache.Object, timeProvider);
        }
    }
}
