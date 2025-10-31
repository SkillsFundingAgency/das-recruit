using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest;

public abstract class CloneVacancyOrchestratorTestBase
{
    internal string EmployerAccountId = "AB1234";

    internal Guid SourceVacancyId = Guid.NewGuid();
    internal DateTime SourceStartDate = DateTime.Now.AddDays(14);
    internal DateTime SourceClosingDate = DateTime.Now.AddDays(7);
    internal Vacancy SourceVacancy => new Vacancy
    {
        Id = SourceVacancyId,
        AccountLegalEntityPublicHashedId = "1",
        EmployerAccountId = EmployerAccountId,
        Status = VacancyStatus.Live,
        StartDate = SourceStartDate,
        ClosingDate = SourceClosingDate
    };
    internal VacancyRouteModel VRM => new VacancyRouteModel
    {
        VacancyId = SourceVacancyId,
        EmployerAccountId = EmployerAccountId
    };

    internal CloneVacancyOrchestrator GetSut(Vacancy vacancy)
    {
        var timeProviderMock = new Mock<ITimeProvider>();
        timeProviderMock.Setup(t => t.Now).Returns(DateTime.UtcNow);
            
        var loggerMock = new Mock<ILogger<CloneVacancyOrchestrator>>();

        var recruitClientMock = new Mock<IRecruitVacancyClient>();
        recruitClientMock
            .Setup(c => c.GetVacancyAsync(It.IsAny<Guid>()))
            .ReturnsAsync(vacancy);

        var utility = new Utility(recruitClientMock.Object, Mock.Of<ITaskListValidator>());

        return new CloneVacancyOrchestrator(recruitClientMock.Object,
            timeProviderMock.Object, loggerMock.Object, utility);
    }
}