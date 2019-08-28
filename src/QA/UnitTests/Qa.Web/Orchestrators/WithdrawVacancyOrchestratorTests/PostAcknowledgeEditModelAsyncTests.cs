using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Qa.Web.Orchestrators;
using FluentAssertions;
using Moq;
using Xunit;
using Esfa.Recruit.Vacancies.Client.Application.Commands;

namespace UnitTests.Qa.Web.Orchestrators.WithdrawVacancyOrchestratorTests
{
    public class PostAcknowledgeEditModelAsyncTests
    {
        private const long VacancyReference = 12345678;

        private readonly Mock<IMessaging> _messagingMock = new Mock<IMessaging>();
        private readonly Mock<IQaVacancyClient> _clientMock = new Mock<IQaVacancyClient>();
        private readonly WithdrawVacancyOrchestrator _sut;
        public PostAcknowledgeEditModelAsyncTests()
        {
            _sut = new WithdrawVacancyOrchestrator(_clientMock.Object, _messagingMock.Object);
        }

        [Fact]
        public async Task ShouldNotCloseVacancyIfNotAcknowledged()
        {
            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy
                {
                    Status = VacancyStatus.Closed,
                    Id = Guid.NewGuid()
                });

            var orch = _sut;

            var m = new ConsentEditModel
            {
                Acknowledged = false
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeFalse();

            _messagingMock.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), Times.Never);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        public async Task ShouldNotCloseIfVacancyIsInWrongState(VacancyStatus status)
        {
            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = status, Id = Guid.NewGuid()});

            var orch = _sut;

            var m = new ConsentEditModel
            {
                Acknowledged = true
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeFalse();

            _messagingMock.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), Times.Never);
        }

        [Fact]
        public async Task ShouldCloseVacancyIfVacancyIsLive()
        {
            var vacancyId = Guid.NewGuid();

            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Live, Id = vacancyId });

            var orch = _sut;

            var m = new ConsentEditModel
            {
                Acknowledged = true
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeTrue();

            _messagingMock.Verify(c => c.SendCommandAsync(It.IsAny<CloseVacancyCommand>()), Times.Once);
        }
    }
}
