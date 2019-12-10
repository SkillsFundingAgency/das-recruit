using System;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Moq;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public abstract class CloneVacancyOrchestratorTestBase
    {
        internal string EmployerAccountId = "AB1234";

        internal Guid SourceVacancyId = Guid.NewGuid();
        internal DateTime SourceStartDate = DateTime.Now.AddDays(2);
        internal DateTime SourceClosingDate = DateTime.Now.AddDays(1);
        internal Vacancy SourceVacancy => new Vacancy
        {
            Id = SourceVacancyId,
            LegalEntityId = 1,
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

            return new CloneVacancyOrchestrator(recruitClientMock.Object,
                timeProviderMock.Object, loggerMock.Object);
        }
    }
}