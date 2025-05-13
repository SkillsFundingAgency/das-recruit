using System.Linq;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1;

public class TitleOrchestratorTests
{
    [TestCase("this is a value", false)]
    [TestCase("this is a new value", true)]
    public async Task WhenUpdated_ShouldFlagFieldIndicators(string title, bool fieldIndicatorSet)
    {
        TitleOrchestratorTestsFixture fixture = new();
        fixture
            .WithTitle("this is a value")
            .Setup();

        var titleEditModel = new TitleEditModel
        {
            EmployerAccountId = fixture.Vacancy.EmployerAccountId,
            VacancyId = fixture.Vacancy.Id,
            Title = title
        };

        await fixture.PostTitleEditModelAsync(titleEditModel);

        fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Title, fieldIndicatorSet);
    }

    public class TitleOrchestratorTestsFixture
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Title;
        public VacancyUser User { get; }
        public Vacancy Vacancy { get; }
        public TitleOrchestrator Sut {get; private set;}

        public TitleOrchestratorTestsFixture()
        {
            MockClient = new Mock<IProviderVacancyClient>();
            MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

            User = VacancyOrchestratorTestData.GetVacancyUser();
            Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
        }

        public TitleOrchestratorTestsFixture WithTitle(string title)
        {
            Vacancy.Title = title;
            return this;
        }

        public void Setup()
        {
            MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
            MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
            MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
            MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

            Sut = new TitleOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<TitleOrchestrator>>(), 
                Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object, Mock.Of<ITaskListValidator>()), new ServiceParameters());
        }

        public async Task PostTitleEditModelAsync(TitleEditModel model)
        {
            await Sut.PostTitleEditModelAsync(new VacancyRouteModel 
            { 
                Ukprn = Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = Vacancy.Id,
            }, model, User, Vacancy.TrainingProvider.Ukprn.Value);
        }

        public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
        {
            Vacancy.ProviderReviewFieldIndicators.Single(p => p.FieldIdentifier == fieldIdentifier)
                .Should().NotBeNull().And
                .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
        }

        public Mock<IProviderVacancyClient> MockClient { get; set; }
        public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
    }
}