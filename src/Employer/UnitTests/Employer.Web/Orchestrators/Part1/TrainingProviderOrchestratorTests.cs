using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
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
        public async Task PostSelectTrainingProviderAsync_WhenNotChoosingThenRemoveExistingTrainingProvider()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation",
                ProgrammeId = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                IsTrainingProviderSelected = false,
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            vacancy.TrainingProvider.Should().BeNull();
            result.Data.FoundTrainingProviderUkprn.Should().BeNull();
        }

        [Theory]
        [InlineData("This search won't match a single provider")]
        [InlineData("88888")] //will match multiple providers
        public async Task PostSelectTrainingProviderAsync_TrainingProviderSearch_WhenNoSingleProviderFoundShouldNotReturnFoundProvider(string trainingProviderSearch)
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation",
                ProgrammeId = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                IsTrainingProviderSelected = true,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = trainingProviderSearch,
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };
            
            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Data.FoundTrainingProviderUkprn.Should().BeNull();
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_TrainingProviderSearch_WhenSingleProviderFoundShouldReturnFoundProvider()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation",
                ProgrammeId = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                IsTrainingProviderSelected = true,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = "MR EGG 88888888",
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Data.FoundTrainingProviderUkprn.Should().Be(88888888);
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_UKPRN_WhenSingleProviderFoundShouldReturnFoundProvider()
        {
            var vacancy = new Vacancy
            {
                Id = VacancyId,
                EmployerAccountId = EmployerAccountId,
                TrainingProvider = new TrainingProvider(),
                Title = "specified for route validation",
                ProgrammeId = "specified for route validation"
            };

            var orch = GetTrainingProviderOrchestrator(vacancy);

            var m = new SelectTrainingProviderEditModel
            {
                IsTrainingProviderSelected = true,
                SelectionType = TrainingProviderSelectionType.Ukprn,
                Ukprn = "88888888",
                EmployerAccountId = EmployerAccountId,
                VacancyId = VacancyId
            };

            var result = await orch.PostSelectTrainingProviderAsync(m, new VacancyUser());

            result.Data.FoundTrainingProviderUkprn.Should().Be(88888888);
        }

        private TrainingProviderOrchestrator GetTrainingProviderOrchestrator(Vacancy vacancy)
        {
            var mockClient = new Mock<IEmployerVacancyClient>();

            var mockTrainingProviderService = new Mock<ITrainingProviderService>();
            mockTrainingProviderService.Setup(t => t.GetProviderAsync(88888888))
                .ReturnsAsync(new TrainingProvider {Ukprn = 88888888});

            var mockRecruitClient = new Mock<IRecruitVacancyClient>();
            mockRecruitClient.Setup(c => c.GetVacancyAsync(VacancyId)).ReturnsAsync(vacancy);

            var mockTrainingProviderSummaryProvider = new Mock<ITrainingProviderSummaryProvider>();

            var mrEggTrainingProvider = new TrainingProviderSummary { ProviderName = "MR EGG", Ukprn = 88888888 };
            var mrsEggTrainingProvider = new TrainingProviderSummary { ProviderName = "MRS EGG", Ukprn = 88888889 };

            mockTrainingProviderSummaryProvider.Setup(p => p.FindAllAsync()).ReturnsAsync(new List<TrainingProviderSummary>
            {
                mrEggTrainingProvider,
                mrsEggTrainingProvider
            });

            mockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(88888888))
                .ReturnsAsync(mrEggTrainingProvider);

            mockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(88888889))
                .ReturnsAsync(mrsEggTrainingProvider);

            mockRecruitClient.Setup(c => c.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.TrainingProvider))
                .Returns(new EntityValidationResult());

            var mockLog = new Mock<ILogger<TrainingProviderOrchestrator>>();
            var mockReview = new Mock<IReviewSummaryService>();

            return new TrainingProviderOrchestrator(mockClient.Object, mockRecruitClient.Object, mockLog.Object, mockReview.Object, mockTrainingProviderSummaryProvider.Object, mockTrainingProviderService.Object);
        }
    }
}
