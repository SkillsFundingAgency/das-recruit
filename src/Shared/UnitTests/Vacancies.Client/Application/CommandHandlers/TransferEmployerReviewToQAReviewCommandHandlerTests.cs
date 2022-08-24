using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using System.Threading.Tasks;
using System.Threading;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using FluentAssertions;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class TransferEmployerReviewToQAReviewCommandHandlerTests
    {
        private readonly Guid _existingUserGuid = Guid.NewGuid();
        private const string UserEmailAddress = "test@test.com";
        private const string UserName = "FirstName LastName";
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly TransferEmployerReviewToQAReviewCommandHandler _sut;

        public TransferEmployerReviewToQAReviewCommandHandlerTests()
        {
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));

            _mockMessaging = new Mock<IMessaging>();

            _sut = new TransferEmployerReviewToQAReviewCommandHandler(Mock.Of<ILogger<TransferEmployerReviewToQAReviewCommandHandler>>(), _mockVacancyRepository.Object, _mockMessaging.Object, _mockTimeProvider.Object);
        }

        [Fact]
        public async Task WhenVacancyNotFound_ShouldRaiseException()
        {
            var id = Guid.NewGuid();
            var expectedExceptionMessage = string.Format(TransferEmployerReviewToQAReviewCommandHandler.VacancyNotFoundExceptionMessageFormat, id);

            var command = new TransferEmployerReviewToQAReviewCommand(id, _existingUserGuid, UserEmailAddress, UserName);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.Handle(command, new CancellationToken()));
            exception.Message.Should().Be(expectedExceptionMessage);
        }

        [Fact]
        public async Task WhenVacancyReferenceNotSet_ShouldRaiseException()
        {
            var id = Guid.NewGuid();
            var expectedExceptionMessage = string.Format(TransferEmployerReviewToQAReviewCommandHandler.MissingReferenceNumberExceptionMessageFormat, id);

            var existingVacancy = new Vacancy
            {
                Id = id
            };

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id))
                                    .ReturnsAsync(existingVacancy);

            var command = new TransferEmployerReviewToQAReviewCommand(id, _existingUserGuid, UserEmailAddress, UserName);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.Handle(command, new CancellationToken()));
            exception.Message.Should().Be(expectedExceptionMessage);
        }

        [Fact]
        public async Task WhenVacancyStateIsNotReview_ShouldRaiseException()
        {
            var id = Guid.NewGuid();
            var expectedExceptionMessage = string.Format(TransferEmployerReviewToQAReviewCommandHandler.InvalidStateExceptionMessageFormat, id, VacancyStatus.Draft);

            var existingVacancy = new Vacancy
            {
                Id = id,
                VacancyReference = 1,
                Status = VacancyStatus.Draft
            };

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id))
                .ReturnsAsync(existingVacancy);

            var command = new TransferEmployerReviewToQAReviewCommand(id, _existingUserGuid, UserEmailAddress, UserName);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.Handle(command, new CancellationToken()));
            exception.Message.Should().Be(expectedExceptionMessage);
        }

        [Fact]
        public async Task WhenVacancyIsValid_ShouldUpdateVacancyAndPublishMessage()
        {
            var id = Guid.NewGuid();
            var expectedExceptionMessage = string.Format(TransferEmployerReviewToQAReviewCommandHandler.InvalidStateExceptionMessageFormat, id, VacancyStatus.Draft);

            var existingVacancy = new Vacancy
            {
                Id = id,
                VacancyReference = 1,
                Status = VacancyStatus.Review
            };

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.Id))
                .ReturnsAsync(existingVacancy);

            var command = new TransferEmployerReviewToQAReviewCommand(id, _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, new CancellationToken());

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<Guid>()), Times.Once);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(It.Is<Vacancy>(v => v.Status == VacancyStatus.Submitted)), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancySubmittedEvent>()), Times.Once);
        }
    }
}