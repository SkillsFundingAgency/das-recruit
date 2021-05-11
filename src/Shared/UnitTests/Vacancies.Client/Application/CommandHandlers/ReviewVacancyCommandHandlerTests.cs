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
    public class ReviewVacancyCommandHandlerTests
    {
        [Fact]
        public async Task GivenEmployerDescription_ThenShouldUpdateVacancyWithThatDescription()
        {
            var expectedDescription = "updated description";
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = "old description",
                IsDeleted = false,
                Status = VacancyStatus.Draft,
                VacancyReference = 1234567890
            };
            vacancy.OwnerType = OwnerType.Employer;
            var user = new VacancyUser();
            var now = DateTime.Now;
            var message = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Employer, expectedDescription);

            var sut = GetSut(vacancy.Id, vacancy, now);
            await sut.Handle(message, new CancellationToken());

            vacancy.Status.Should().Be(VacancyStatus.Review);
            vacancy.ReviewDate.Should().Be(now);
            vacancy.ReviewByUser.Should().Be(user);
            vacancy.LastUpdatedDate.Should().Be(now);
            vacancy.LastUpdatedByUser.Should().Be(user);
            vacancy.EmployerDescription.Should().Be(expectedDescription);
        }

        [Fact]
        public async Task ShouldNotChangeEmployerDescriptionIfNotSpecifiedInCommand()
        {
            var expectedDescription = "initial description";
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = expectedDescription,
                IsDeleted = false,
                Status = VacancyStatus.Draft,
                VacancyReference = 1234567890
            };
            vacancy.OwnerType= OwnerType.Provider;
            var user = new VacancyUser();
            var now = DateTime.Now;
            var message = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Provider);

            var sut = GetSut(vacancy.Id, vacancy, now);
            await sut.Handle(message, new CancellationToken());

            vacancy.Status.Should().Be(VacancyStatus.Review);
            vacancy.ReviewDate.Should().Be(now);
            vacancy.ReviewByUser.Should().Be(user);
            vacancy.LastUpdatedDate.Should().Be(now);
            vacancy.LastUpdatedByUser.Should().Be(user);
            vacancy.EmployerDescription.Should().Be(expectedDescription);
        }

        [Fact]
        public async Task WhenVacancyNotFound_ShouldRaiseException()
        {
            var id = Guid.NewGuid();
            var user = new VacancyUser();
            var now = DateTime.Now;
            var expectedExceptionMessage = string.Format(ReviewVacancyCommandHandler.VacancyNotFoundExceptionMessageFormat, id);
            var message = new ReviewVacancyCommand(id, user, OwnerType.Provider);

            var sut = GetSut(id, null, now);
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await sut.Handle(message, new CancellationToken()));

            exception.Message.Should().Be(expectedExceptionMessage);
        }

        [Fact]
        public async Task WhenStatusIsIncorrect_ShouldRaiseException()
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = "initial description",
                IsDeleted = false,
                Status = VacancyStatus.Live,
                VacancyReference = 1234567890
            };
            vacancy.OwnerType = OwnerType.Employer;
            var user = new VacancyUser();
            var now = DateTime.Now;
            var message = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Provider);
            var expectedExceptionMessage = string.Format(ReviewVacancyCommandHandler.InvalidStateExceptionMessageFormat, vacancy.Id, vacancy.Status);

            var sut = GetSut(vacancy.Id, vacancy, now);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Handle(message, new CancellationToken()));

            exception.Message.Should().Be(expectedExceptionMessage);
        }

        [Fact]
        public async Task WhenReferenceNumberIsNotGenerated_ShouldRaiseException()
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = "initial description",
                IsDeleted = false,
                Status = VacancyStatus.Live,
                VacancyReference = null
            };
            vacancy.OwnerType = OwnerType.Employer;
            var user = new VacancyUser();
            var now = DateTime.Now;
            var message = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Provider);
            var expectedExceptionMessage = string.Format(ReviewVacancyCommandHandler.MissingReferenceNumberExceptionMessageFormat, vacancy.Id);

            var sut = GetSut(vacancy.Id, vacancy, now);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Handle(message, new CancellationToken()));

            exception.Message.Should().Be(expectedExceptionMessage);
        }


        [Fact]
        public async Task WhenOwnerHasChanged_ShouldRaiseException()
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerDescription = "initial description",
                IsDeleted = false,
                Status = VacancyStatus.Draft,
                VacancyReference = 1234567890
            };
            vacancy.OwnerType = OwnerType.Employer;
            var user = new VacancyUser();
            var now = DateTime.Now;
            var message = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Provider);
            var expectedExceptionMessage = string.Format(ReviewVacancyCommandHandler.InvalidOwnerExceptionMessageFormat, vacancy.Id, message.SubmissionOwner, vacancy.OwnerType);

            var sut = GetSut(vacancy.Id, vacancy, now);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Handle(message, new CancellationToken()));

            exception.Message.Should().Be(expectedExceptionMessage);
        }

        public ReviewVacancyCommandHandler GetSut(Guid id, Vacancy vacancy, DateTime now)
        {
            var mockLogger = new Mock<ILogger<SubmitVacancyCommandHandler>>();

            var mockRepository = new Mock<IVacancyRepository>();
            mockRepository.Setup(r => r.GetVacancyAsync(id)).ReturnsAsync(vacancy);

            var mockMessaging = new Mock<IMessaging>();

            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(t => t.Now).Returns(now);

            var mockEmployerNameService = new Mock<IEmployerService>();

            var handler = new ReviewVacancyCommandHandler(mockLogger.Object, mockRepository.Object, mockMessaging.Object, mockTimeProvider.Object, mockEmployerNameService.Object);

            return handler;
        }
    }
}
