using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTests.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator
{
    public class PostAcknowledgeEditModelAsyncTests
    {
        private const long VacancyReference = 12345678;

        [Fact]
        public async Task ShouldNotCloseVacancyIfNotAcknowledged()
        {
            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy
                {
                    Status = VacancyStatus.Closed,
                    Id = Guid.NewGuid()
                });

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new ConsentEditModel
            {
                Acknowledged = false
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeFalse();

            mockClient.Verify(c => c.CloseVacancyAsync(It.IsAny<Guid>(), It.IsAny<VacancyUser>()), Times.Never);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Approved)]
        [InlineData(VacancyStatus.Closed)]
        public async Task ShouldNotCloseIfVacancyIsInWrongState(VacancyStatus status)
        {
            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = status, Id = Guid.NewGuid()});

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new ConsentEditModel
            {
                Acknowledged = true
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeFalse();

            mockClient.Verify(c => c.CloseVacancyAsync(It.IsAny<Guid>(), It.IsAny<VacancyUser>()), Times.Never);
        }

        [Fact]
        public async Task ShouldCloseVacancyIfVacancyIsLive()
        {
            var vacancyId = Guid.NewGuid();

            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Live, Id = vacancyId });

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new ConsentEditModel
            {
                Acknowledged = true
            };

            var user = new VacancyUser();

            var result = await orch.PostConsentEditModelAsync(m, VacancyReference, user);

            result.Should().BeTrue();

            mockClient.Verify(c => c.CloseVacancyAsync(vacancyId, user), Times.Once);
        }
    }
}
