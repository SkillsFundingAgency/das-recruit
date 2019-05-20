using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class SubmitVacancyCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldChangeEmployerDescriptionIfSpecifiedInCommand()
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = "initial description",
                IsDeleted = false,
                Status = VacancyStatus.Draft,
                VacancyReference = 1234567890
            };

            var user = new VacancyUser();
            var now = DateTime.Now;

            var handler = GetSubmitVacancyCommandHandler(vacancy, now);

            var message = new SubmitVacancyCommand(vacancy.Id, user, "updated description");

            var cancel = new CancellationToken();
            await handler.Handle(message, cancel);

            vacancy.Status.Should().Be(VacancyStatus.Submitted);
            vacancy.SubmittedDate.Should().Be(now);
            vacancy.SubmittedByUser.Should().Be(user);
            vacancy.LastUpdatedDate.Should().Be(now);
            vacancy.LastUpdatedByUser.Should().Be(user);

            vacancy.EmployerDescription.Should().Be("updated description");
        }

        [Fact]
        public async Task Handle_ShouldNotChangeEmployerDescriptionIfNotSpecifiedInCommand()
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = "initial description",
                IsDeleted = false,
                Status = VacancyStatus.Draft,
                VacancyReference = 1234567890
            };

            var user = new VacancyUser();
            var now = DateTime.Now;

            var handler = GetSubmitVacancyCommandHandler(vacancy, now);

            var message = new SubmitVacancyCommand(vacancy.Id, user);

            var cancel = new CancellationToken();
            await handler.Handle(message, cancel);

            vacancy.Status.Should().Be(VacancyStatus.Submitted);
            vacancy.SubmittedDate.Should().Be(now);
            vacancy.SubmittedByUser.Should().Be(user);
            vacancy.LastUpdatedDate.Should().Be(now);
            vacancy.LastUpdatedByUser.Should().Be(user);

            vacancy.EmployerDescription.Should().Be("initial description");
        }

        public SubmitVacancyCommandHandler GetSubmitVacancyCommandHandler(Vacancy vacancy, DateTime now)
        {
            var mockLogger = new Mock<ILogger<SubmitVacancyCommandHandler>>();

            var mockRepository = new Mock<IVacancyRepository>();
            mockRepository.Setup(r => r.GetVacancyAsync(vacancy.Id)).ReturnsAsync(vacancy);

            var mockMessaging = new Mock<IMessaging>();

            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Now).Returns(now);

            var mockEmployerNameService = new Mock<IEmployerService>();

            var handler = new SubmitVacancyCommandHandler(
                mockLogger.Object, mockRepository.Object, mockMessaging.Object, mockTimeProvider.Object, mockEmployerNameService.Object);

            return handler;
        }
    }
}
