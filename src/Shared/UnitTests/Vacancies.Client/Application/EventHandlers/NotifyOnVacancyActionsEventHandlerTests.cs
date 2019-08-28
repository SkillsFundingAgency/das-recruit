using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.EventHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.EventHandlers
{
    [Trait("Category", "Unit")]
    public class NotifyOnVacancyActionsEventHandlerTests
    {
        private readonly NotifyOnVacancyActionsEventHandler _handler;
        private readonly Mock<INotifyVacancyUpdates> _mockNotifier;
        private readonly Vacancy _existingVacancy;

        public NotifyOnVacancyActionsEventHandlerTests()
        {
            var newVacancyId = Guid.NewGuid();
            _existingVacancy = GetTestVacancy();
            var currentTime = DateTime.UtcNow;
            var startDate = DateTime.Now.AddDays(20);
            var closingDate = DateTime.Now.AddDays(10);

            var mockRepository = new Mock<IVacancyRepository>();

            _mockNotifier = new Mock<INotifyVacancyUpdates>();

            mockRepository.Setup(x => x.GetVacancyAsync(_existingVacancy.Id)).ReturnsAsync(_existingVacancy);

            _handler = new NotifyOnVacancyActionsEventHandler(
                Mock.Of<ILogger<NotifyOnVacancyActionsEventHandler>>(),
                _mockNotifier.Object,
                mockRepository.Object
            );
        }

        [Theory]
        [InlineData(LiveUpdateKind.ClosingDate)]
        [InlineData(LiveUpdateKind.StartDate)]
        [InlineData(LiveUpdateKind.StartDate | LiveUpdateKind.ClosingDate)]
        public async Task GivenLiveVacancyUpdatedEventWithDateChange_NotifyLiveVacancyChanged(LiveUpdateKind updateKind)
        {
            var @event = new LiveVacancyUpdatedEvent
            {
                VacancyId = _existingVacancy.Id,
                VacancyReference = _existingVacancy.VacancyReference.Value,
                UpdateKind = updateKind
            };

            await _handler.Handle(@event, CancellationToken.None);

            _mockNotifier
                .Verify(x => x.LiveVacancyChanged(_existingVacancy), Times.Once);
        }

        [Fact]
        public async Task GivenLiveVacancyUpdatedEventWithNoDateChange_ThenDoNotNotifyLiveVacancyChanged()
        {
            var @event = new LiveVacancyUpdatedEvent
            {
                VacancyId = _existingVacancy.Id,
                VacancyReference = _existingVacancy.VacancyReference.Value,
                UpdateKind = LiveUpdateKind.None
            };

            await _handler.Handle(@event, CancellationToken.None);

            _mockNotifier
                .Verify(x => x.LiveVacancyChanged(_existingVacancy), Times.Never);
        }

        private Vacancy GetTestVacancy()
        {
            var fixture = new Fixture();
            var vacancy = fixture.Create<Vacancy>();

            // Set enum values to be non zero so not assuming a default value.
            vacancy.DisabilityConfident = DisabilityConfident.Yes;
            vacancy.GeoCodeMethod = GeoCodeMethod.PostcodesIo;
            vacancy.SourceOrigin = SourceOrigin.EmployerWeb;
            vacancy.SourceType = SourceType.Extension;
            vacancy.Status = VacancyStatus.Live;
            vacancy.Wage.DurationUnit = DurationUnit.Year;
            vacancy.Wage.WageType = WageType.NationalMinimumWage;

            return vacancy;
        }
    }
}