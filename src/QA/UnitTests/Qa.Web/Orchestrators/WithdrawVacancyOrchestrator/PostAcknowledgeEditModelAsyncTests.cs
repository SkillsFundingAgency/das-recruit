using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTests.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator
{
    public class PostAcknowledgeEditModelAsyncTests
    {
        private const long VacancyReference = 12345678;

        private readonly Mock<IQaVacancyClient> _mockClient = new Mock<IQaVacancyClient>();
        private readonly Mock<IMessaging> _mockMessaging = new Mock<IMessaging>();
        
        [Fact]
        public async Task ShouldNotCloseVacancyIfNotAcknowledged()
        {
            _mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy
                {
                    Status = VacancyStatus.Closed,
                    Id = Guid.NewGuid()
                });

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(_mockClient.Object, _mockMessaging.Object);

            var m = new ConsentEditModel
            {
                Acknowledged = false
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeFalse();

            _mockMessaging.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), Times.Never);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        public async Task ShouldNotCloseIfVacancyIsInWrongState(VacancyStatus status)
        {
            _mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = status, Id = Guid.NewGuid()});

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(_mockClient.Object, _mockMessaging.Object);

            var m = new ConsentEditModel
            {
                Acknowledged = true
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeFalse();

            _mockMessaging.Verify(c => c.SendCommandAsync(It.IsAny<ICommand>()), Times.Never);
        }

        [Fact]
        public async Task ShouldCloseVacancyIfVacancyIsLive()
        {
            var vacancyId = Guid.NewGuid();

            _mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Live, Id = vacancyId });

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(_mockClient.Object, _mockMessaging.Object);

            var m = new ConsentEditModel
            {
                Acknowledged = true
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeTrue();

            _mockMessaging.Verify(c => c.SendCommandAsync(It.Is<CloseVacancyCommand>(p => p.ClosureReason == ClosureReason.WithdrawnByQa)), Times.Once);
        }
    }
}
