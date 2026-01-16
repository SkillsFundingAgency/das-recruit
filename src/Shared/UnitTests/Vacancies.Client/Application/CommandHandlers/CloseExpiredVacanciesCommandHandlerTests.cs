using System.Collections.Generic;
using System.Threading;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers;

public class CloseExpiredVacanciesCommandHandlerTests
{
    [Test]
    public async Task ThenWithNoVacanciesDoesNotProcess()
    {
        var mockLogger = new Mock<ILogger<CloseExpiredVacanciesCommandHandler>>();
        var mockQuery = new Mock<IVacancyQuery>();
        mockQuery
            .Setup(x => x.GetVacanciesToCloseAsync(DateTime.Parse("2019-03-24")))
            .ReturnsAsync([]);
            
        var mockTimeProvider = new Mock<ITimeProvider>();
        mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));
            
        var mockVacancyService = new Mock<IVacancyService>();
        var messagingService = new Mock<IMessaging>();
        var queryStoreReader = new Mock<IQueryStoreReader>();
            
        var handler = new CloseExpiredVacanciesCommandHandler(mockLogger.Object, mockQuery.Object, mockTimeProvider.Object, mockVacancyService.Object, queryStoreReader.Object, messagingService.Object);
            
        var command = new CloseExpiredVacanciesCommand();
            
        await handler.Handle(command, CancellationToken.None);
            
        mockVacancyService.Verify(s => s.CloseExpiredVacancy(It.IsAny<Guid>()), Times.Never());
        messagingService.Verify(x=>x.PublishEvent(It.IsAny<IEvent>()), Times.Never);
    }
    [Test]
    public async Task ShouldCloseAllVacanciesWithClosingDateBeforeToday_And_Orphaned_Vacancies()
    {
        var vacancies = new List<VacancyIdentifier> 
        {
            new() { ClosingDate = DateTime.Parse("2019-03-22"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1651") },
            new() { ClosingDate = DateTime.Parse("2019-03-03"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1651") },
            new() { ClosingDate = DateTime.Parse("2019-03-24"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1652") },
            new() { ClosingDate = DateTime.Parse("2019-03-25"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1653") }
        };
            
        var mockLogger = new Mock<ILogger<CloseExpiredVacanciesCommandHandler>>();
        var mockQuery = new Mock<IVacancyQuery>();
        mockQuery
            .Setup(x => x.GetVacanciesToCloseAsync(DateTime.Parse("2019-03-24")))
            .ReturnsAsync(vacancies);

        var liveVacancies = new List<LiveVacancy>
        {
            new LiveVacancy
            {
                VacancyId= Guid.NewGuid(),
                VacancyReference = 1
            },
            new LiveVacancy
            {
                VacancyId= Guid.NewGuid(),
                VacancyReference = 2
            }
        };
        var queryStoreReader = new Mock<IQueryStoreReader>();
        queryStoreReader.Setup(x => x.GetLiveExpiredVacancies(DateTime.Parse("2019-03-24")))
            .ReturnsAsync(liveVacancies);
            
        var mockTimeProvider = new Mock<ITimeProvider>();
        mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));
            
        var mockVacancyService = new Mock<IVacancyService>();
        var messagingService = new Mock<IMessaging>();
            
        var handler = new CloseExpiredVacanciesCommandHandler(mockLogger.Object, mockQuery.Object, mockTimeProvider.Object, mockVacancyService.Object, queryStoreReader.Object, messagingService.Object);
            
        var command = new CloseExpiredVacanciesCommand();
            
        await handler.Handle(command, new CancellationToken());
            
        mockVacancyService.Verify(s => s.CloseExpiredVacancy(It.IsAny<Guid>()), Times.Exactly(4));
        messagingService.Verify(x=>x.PublishEvent(It.Is<VacancyClosedEvent>(c=>c.VacancyReference.Equals(1))), Times.Once());
        messagingService.Verify(x=>x.PublishEvent(It.Is<VacancyClosedEvent>(c=>c.VacancyReference.Equals(2))), Times.Once());
    }
}