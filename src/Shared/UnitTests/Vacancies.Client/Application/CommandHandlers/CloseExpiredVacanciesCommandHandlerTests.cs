using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class CloseExpiredVacanciesCommandHandlerTests
    {
        [Fact]
        public async Task ThenWithNoVacanciesDoesNotProcess()
        {
            var mockLogger = new Mock<ILogger<CloseExpiredVacanciesCommandHandler>>();

            var mockCursor = new Mock<IAsyncCursor<VacancyIdentifier>>();
            mockCursor.Setup(x => x.Current)
                .Returns(new List<VacancyIdentifier>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            
            var mockQuery = new Mock<IVacancyQuery>();
            mockQuery.Setup(q => q.GetVacanciesByStatusAndClosingDateAsync(It.Is<VacancyStatus>(s => s == VacancyStatus.Live), DateTime.Parse("2019-03-24")))
                .ReturnsAsync(mockCursor.Object);
            
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));
            
            var mockVacancyService = new Mock<IVacancyService>();
            
            var handler = new CloseExpiredVacanciesCommandHandler(mockLogger.Object, mockQuery.Object, mockTimeProvider.Object, mockVacancyService.Object);
            
            var command = new CloseExpiredVacanciesCommand();
            
            await handler.Handle(command, new CancellationToken());
            
            mockVacancyService.Verify(s => s.CloseExpiredVacancy(It.IsAny<Guid>()), Times.Never());
        }
        [Fact]
        public async Task ShouldCloseAllVacanciesWithClosingDateBeforeToday()
        {
            var vacancies = new List<VacancyIdentifier> 
            {
                new VacancyIdentifier{ClosingDate = DateTime.Parse("2019-03-22"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1651")},
                new VacancyIdentifier{ClosingDate = DateTime.Parse("2019-03-03"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1651")},
                new VacancyIdentifier{ClosingDate = DateTime.Parse("2019-03-24"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1652")},
                new VacancyIdentifier{ClosingDate = DateTime.Parse("2019-03-25"), Id = Guid.Parse("4913c8fb-4f5b-4069-9301-858e405c1653")}
            };
            
            var mockLogger = new Mock<ILogger<CloseExpiredVacanciesCommandHandler>>();

            var mockCursor = new Mock<IAsyncCursor<VacancyIdentifier>>();
            mockCursor.Setup(x => x.Current)
                .Returns(vacancies);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            
            var mockQuery = new Mock<IVacancyQuery>();
            mockQuery.Setup(q => q.GetVacanciesByStatusAndClosingDateAsync(It.Is<VacancyStatus>(s => s == VacancyStatus.Live), DateTime.Parse("2019-03-24")))
                .ReturnsAsync(mockCursor.Object);
            
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));
            
            var mockVacancyService = new Mock<IVacancyService>();
            
            var handler = new CloseExpiredVacanciesCommandHandler(mockLogger.Object, mockQuery.Object, mockTimeProvider.Object, mockVacancyService.Object);
            
            var command = new CloseExpiredVacanciesCommand();
            
            await handler.Handle(command, new CancellationToken());
            
            mockVacancyService.Verify(s => s.CloseExpiredVacancy(It.IsAny<Guid>()), Times.Exactly(4));
        }
    }
}
