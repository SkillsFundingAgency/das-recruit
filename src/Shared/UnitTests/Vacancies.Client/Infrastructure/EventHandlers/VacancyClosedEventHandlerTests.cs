using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Projections = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.EventHandlers
{
    public class VacancyClosedEventHandlerTests
    {
        private VacancyClosedEventHandler _handler;
        private Mock<ILogger<VacancyClosedEventHandler>> _mockLogger;
        private Mock<IQueryStoreWriter> _mockQueryStore;
        private Mock<IVacancyRepository> _mockVacancyRepository;
        private Mock<IApprenticeshipProgrammeProvider> _mockReferenceDataReader;
        private Mock<ITimeProvider> _mockTimeProvider;
        private Mock<ICommunicationQueueService> _mockCommunicationQueueService;
        private DateTime _currentTime;
        private VacancyClosedEvent _event;
        private Vacancy _vacancy;
        private ApprenticeshipProgramme _apprenticeshipProgramme;
        private readonly Mock<IQueryStoreReader> _mockQueryReader;

        [Fact]
        public async Task Handle_ShouldDeleteLiveVacancyFromQueryStoreThenRecreate()
        {
            int sequence = 1;

            _mockQueryStore
                .Setup(x => x.DeleteLiveVacancyAsync(_vacancy.VacancyReference.Value))
                .Callback(() => Assert.Equal(1, sequence++))
                .Returns(Task.CompletedTask);

            _mockQueryStore
                .Setup(x => x.UpdateClosedVacancyAsync(It.IsAny<Projections.ClosedVacancy>()))
                .Callback(() => Assert.Equal(2, sequence++))
                .Returns(Task.CompletedTask);

            await _handler.Handle(_event, CancellationToken.None);

            Assert.Equal(3, sequence);
        }

        [Fact]
        public async Task Handle_ShouldDeleteLiveVacancyFromQueryStoreThenRecreate_If_Apprenticeship_No_Longer_Exists()
        {
            _vacancy.ProgrammeId = "-1";
            int sequence = 1;

            _mockQueryStore
                .Setup(x => x.DeleteLiveVacancyAsync(_vacancy.VacancyReference.Value))
                .Callback(() => Assert.Equal(1, sequence++))
                .Returns(Task.CompletedTask);

            _mockQueryStore
                .Setup(x => x.UpdateClosedVacancyAsync(It.IsAny<Projections.ClosedVacancy>()))
                .Callback(() => Assert.Equal(2, sequence++))
                .Returns(Task.CompletedTask);

            await _handler.Handle(_event, CancellationToken.None);
            Assert.Equal(3, sequence);
        }
        
        [Fact]
        public async Task Handle_ShouldNotSendEmail_IfAlreadyClosed()
        {
            _vacancy.ProgrammeId = "-1";
            int sequence = 1;

            _mockQueryReader.Setup(x => x.GetClosedVacancy(_vacancy.VacancyReference.Value))
                .ReturnsAsync(new Projections.ClosedVacancy());
            
            _mockQueryStore
                .Setup(x => x.DeleteLiveVacancyAsync(_vacancy.VacancyReference.Value))
                .Callback(() => Assert.Equal(1, sequence++))
                .Returns(Task.CompletedTask);

            _mockQueryStore
                .Setup(x => x.UpdateClosedVacancyAsync(It.IsAny<Projections.ClosedVacancy>()))
                .Callback(() => Assert.Equal(2, sequence++))
                .Returns(Task.CompletedTask);

            await _handler.Handle(_event, CancellationToken.None);

            _mockCommunicationQueueService.Verify(x=>x.AddMessageAsync(It.IsAny<CommunicationRequest>()), Times.Never);
            Assert.Equal(2, sequence);
        }
        
        public VacancyClosedEventHandlerTests()
        {
            _currentTime = DateTime.UtcNow;
            _vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                AccountLegalEntityPublicHashedId = "299792458",
                ProgrammeId = "42",
                EmployerLocation = new Address(),
                Qualifications = new List<Qualification>(),
                TrainingProvider = new TrainingProvider
                {
                    Ukprn = 1618
                },
                VacancyReference = 1234567,
                Wage = new Wage
                {
                    Duration = 12,
                    DurationUnit = DurationUnit.Month,
                    FixedWageYearlyAmount = 32000,
                    WageType = WageType.FixedWage,
                    WeeklyHours = 37,
                }
            };
            _event = new VacancyClosedEvent
            {
                VacancyId = _vacancy.Id,
                VacancyReference = _vacancy.VacancyReference.Value
            };
            _apprenticeshipProgramme = 
                
                    new ApprenticeshipProgramme {
                        Id = _vacancy.ProgrammeId
                    };

            _mockLogger = new Mock<ILogger<VacancyClosedEventHandler>>();
            _mockQueryStore = new Mock<IQueryStoreWriter>();

            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockVacancyRepository
                .Setup(x => x.GetVacancyAsync(_vacancy.Id))
                .ReturnsAsync(_vacancy);

            _mockReferenceDataReader = new Mock<IApprenticeshipProgrammeProvider>();
            _mockReferenceDataReader
                .Setup(x => x.GetApprenticeshipProgrammeAsync(_vacancy.ProgrammeId))
                .ReturnsAsync(_apprenticeshipProgramme);

            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider
                .Setup(x => x.Now)
                .Returns(() => _currentTime);

            _mockCommunicationQueueService = new Mock<ICommunicationQueueService>();

            _mockQueryReader = new Mock<IQueryStoreReader>();

            _handler = new VacancyClosedEventHandler(
                _mockLogger.Object,
                _mockQueryStore.Object,
                _mockVacancyRepository.Object,
                _mockReferenceDataReader.Object,
                _mockTimeProvider.Object,
                _mockCommunicationQueueService.Object,
                _mockQueryReader.Object);
        }
    }
}
