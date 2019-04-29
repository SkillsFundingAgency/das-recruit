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
        public NotifyOnVacancyActionsEventHandlerTests()
        {
            
        }

        [Fact]
        public async Task X()
        {
            var newVacancyId = Guid.NewGuid();
            var existingVacancy = GetTestVacancy();
            var currentTime = DateTime.UtcNow;
            var startDate = DateTime.Now.AddDays(20);
            var closingDate = DateTime.Now.AddDays(10);
            Vacancy clone = null;

            var mockRepository = new Mock<IVacancyRepository>();
            var mockVacancyReviewRepository = new Mock<IVacancyReviewRepository>();
            //var mockTimeProvider = new Mock<ITimeProvider>();

            var mockNotifier = new Mock<INotifyVacancyUpdates>();

            //mockTimeProvider.Setup(x => x.Now).Returns(currentTime);
            mockRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id)).ReturnsAsync(existingVacancy);
            mockRepository.Setup(x => x.CreateAsync(It.IsAny<Vacancy>()))
                            .Callback<Vacancy>(arg => clone = arg)
                            .Returns(Task.CompletedTask);

            var handler = new NotifyOnVacancyActionsEventHandler(
                Mock.Of<ILogger<NotifyOnVacancyActionsEventHandler>>(),
                mockNotifier.Object,
                mockRepository.Object,
                mockVacancyReviewRepository.Object
            );

            var command = new LiveVacancyUpdatedEvent(
                newVacancyId,
                existingVacancy.VacancyReference.Value,
                LiveUpdateKind.ClosingDate);

            await handler.Handle(command, CancellationToken.None);
        }

        private static Vacancy GetTestVacancy()
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