using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class RejectVacancyCommandHandlerTests
    {
        private readonly Mock<ILogger<RejectVacancyCommandHandler>> _mockLogger = new Mock<ILogger<RejectVacancyCommandHandler>>();
        private readonly Mock<IVacancyRepository> _mockVacancyRepository = new Mock<IVacancyRepository>();
        private readonly Mock<IMessaging> _mockMessaging = new Mock<IMessaging>();

        [Theory]
        [InlineData(VacancyStatus.Review)]        
        public async Task WhenVacancyIsInValidState_ShouldSetVacancyRejected(VacancyStatus status)
        {
            //Arrange
            var fixture = new Fixture();
            var rejectVacancyCommand = fixture.Create<RejectVacancyCommand>();
            var vacancy = fixture.Build<Vacancy>().With(v => v.Status, status)
                                                  .With(v => v.VacancyReference, rejectVacancyCommand.VacancyReference)
                                                  .With(v => v.IsDeleted, false)
                                                  .Create();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancy);
            var sut = new RejectVacancyCommandHandler(_mockLogger.Object, _mockVacancyRepository.Object, _mockMessaging.Object);

            //Act
            await sut.Handle(rejectVacancyCommand, new CancellationToken());
            
            //Assert
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Once);
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyRejectedEvent>()), Times.Once);
        }

        [Theory]
        [InlineData(VacancyStatus.Live)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        [InlineData(VacancyStatus.Submitted)]
        public async Task WhenVacancyIsInValidState_ShouldNotSetVacancyRejected(VacancyStatus status)
        {
            //Arrange
            var fixture = new Fixture();
            var rejectVacancyCommand = fixture.Create<RejectVacancyCommand>();
            var vacancy = fixture.Build<Vacancy>().With(v => v.Status, status)
                                                  .With(v => v.VacancyReference, rejectVacancyCommand.VacancyReference)
                                                  .With(v => v.IsDeleted, false)
                                                  .Create();
            _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<long>())).ReturnsAsync(vacancy);
            var sut = new RejectVacancyCommandHandler(_mockLogger.Object, _mockVacancyRepository.Object, _mockMessaging.Object);

            //Act
            await sut.Handle(fixture.Create<RejectVacancyCommand>(), new CancellationToken());
            
            //Assert
            _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
            _mockMessaging.Verify(m => m.PublishEvent(It.IsAny<VacancyRejectedEvent>()), Times.Never);
        }
    }
}
