using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;
using Moq;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public abstract class CloneVacancyOrchestratorTestBase
    {
        internal long SourceUkprn = 10004375;
        internal TrainingProvider TrainingProvider => new TrainingProvider {Ukprn = SourceUkprn};

        internal Guid SourceVacancyId = Guid.NewGuid();
        internal DateTime SourceStartDate = DateTime.Now.AddDays(2);
        internal DateTime SourceClosingDate = DateTime.Now.AddDays(1);
        internal Vacancy SourceVacancy => new Vacancy
        {
            Id = SourceVacancyId,
            AccountLegalEntityPublicHashedId = "1",
            TrainingProvider = TrainingProvider,
            Status = VacancyStatus.Live,
            StartDate = SourceStartDate,
            ClosingDate = SourceClosingDate,
            OwnerType = OwnerType.Provider
        };
        internal VacancyRouteModel VRM => new VacancyRouteModel
        {
            VacancyId = SourceVacancyId,
            Ukprn = SourceUkprn
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
            var utility = new Utility(recruitClientMock.Object);

            return new CloneVacancyOrchestrator(recruitClientMock.Object,
                timeProviderMock.Object, loggerMock.Object, utility);
        }
    }
}