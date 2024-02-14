using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class DeleteVacancyCommandHandlerTests
    {
        private readonly Mock<ILogger<DeleteVacancyCommandHandler>> _mockLogger = new Mock<ILogger<DeleteVacancyCommandHandler>>();
        private readonly Mock<IVacancyRepository> _mockVacancyRepository = new Mock<IVacancyRepository>();
        private readonly Mock<IMessaging> _mockMessaging = new Mock<IMessaging>();
        private readonly Mock<ITimeProvider> _mockTimeProvider = new Mock<ITimeProvider>();

        [Theory]
        [InlineData(VacancyStatus.Live)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        [InlineData(VacancyStatus.Submitted)]
        public async Task WhenVacancyIsNotInValidState_ShouldNotSetVacancyDeleted(VacancyStatus status)
        {
            var fixture = new Fixture();
            var vacancy = fixture.Build<Vacancy>().With(v => v.Status, status).Create();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
            var sut = GetSut();
            await sut.Handle(fixture.Create<DeleteVacancyCommand>(), new CancellationToken());
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task WhenVacancyIsAlreadyDeleted_ShouldNotSetVacancyDeleted()
        {
            var fixture = new Fixture();
            var vacancy = fixture.Build<Vacancy>().With(v => v.IsDeleted, true).Create();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
            var sut = GetSut();
            await sut.Handle(fixture.Create<DeleteVacancyCommand>(), new CancellationToken());
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task WhenVacancyIsNotFound_ShouldNotSetVacancyDeleted()
        {
            var fixture = new Fixture();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync((Vacancy)null);
            var sut = GetSut();
            await sut.Handle(fixture.Create<DeleteVacancyCommand>(), new CancellationToken());
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task WhenUserInformationIsMissing_SetVacancyDeletedWithoutUser()
        {
            var fixture = new Fixture();
            var user = fixture.Create<VacancyUser>();
            var vacancy = 
                fixture.Build<Vacancy>()
                    .With(v => v.IsDeleted, false)
                    .With(v => v.LastUpdatedByUser, user)
                    .Without(v => v.DeletedByUser).Create();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
            var command = fixture.Build<DeleteVacancyCommand>().Without(v => v.User).Create();
            var sut = GetSut();
            await sut.Handle(command, new CancellationToken());
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.Is<Vacancy>(v => v.DeletedByUser == null && v.LastUpdatedByUser == user)));
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyDeletedEvent>()));
        }

        [Fact]
        public async Task WhenUserInformationIsAvailable_SetVacancyDeletedByUser()
        {
            var fixture = new Fixture();
            var lastUpdatedByUser = fixture.Create<VacancyUser>();
            var deletedByUser = fixture.Create<VacancyUser>();
            var vacancy = 
                fixture.Build<Vacancy>()
                    .With(v => v.IsDeleted, false)
                    .With(v => v.LastUpdatedByUser, lastUpdatedByUser).Create();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
            var command = fixture.Build<DeleteVacancyCommand>().With(v => v.User, deletedByUser).Create();
            var sut = GetSut();
            await sut.Handle(command, new CancellationToken());
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.Is<Vacancy>(v => v.DeletedByUser == deletedByUser && v.LastUpdatedByUser == deletedByUser)));
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyDeletedEvent>()));
        }

        private DeleteVacancyCommandHandler GetSut()
        {
            return new DeleteVacancyCommandHandler(
                _mockLogger.Object, _mockVacancyRepository.Object, _mockMessaging.Object, _mockTimeProvider.Object);
        }
    }
}