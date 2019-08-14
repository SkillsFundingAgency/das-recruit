using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Models.WithdrawVacancy;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTests.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator
{
    public class PostFindVacancyEditModelAsyncTests
    {
        private const long VacancyReference = 12345678;

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Approved)]
        public async Task ShouldReturnNotLiveIfVacancyExistsAndIsNotLive(VacancyStatus status)
        {
            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy {Status = status});

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new FindVacancyEditModel
            {
                VacancyReference = VacancyReference.ToString()
            };

            var result = await orch.PostFindVacancyEditModelAsync(m);

            result.ResultType.Should().Be(PostFindVacancyEditModelResultType.NotLive);
        }

        [Fact]
        public async Task ShouldReturnNotFoundIfVacancyDoesNotExist()
        {
            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ThrowsAsync(new VacancyNotFoundException("not found"));

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new FindVacancyEditModel
            {
                VacancyReference = VacancyReference.ToString()
            };

            var result = await orch.PostFindVacancyEditModelAsync(m);

            result.ResultType.Should().Be(PostFindVacancyEditModelResultType.NotFound);
        }

        [Fact]
        public async Task ShouldReturnAlreadyClosedIfVacancyIsClosed()
        {
            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Closed });

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new FindVacancyEditModel
            {
                VacancyReference = VacancyReference.ToString()
            };

            var result = await orch.PostFindVacancyEditModelAsync(m);

            result.ResultType.Should().Be(PostFindVacancyEditModelResultType.AlreadyClosed);
        }

        [Fact]
        public async Task ShouldReturnCanCloseIfVacancyCanBeClosed()
        {
            var mockClient = new Mock<IQaVacancyClient>();

            mockClient.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Live });

            var orch = new Esfa.Recruit.Qa.Web.Orchestrators.WithdrawVacancyOrchestrator(mockClient.Object);

            var m = new FindVacancyEditModel
            {
                VacancyReference = VacancyReference.ToString()
            };

            var result = await orch.PostFindVacancyEditModelAsync(m);

            result.ResultType.Should().Be(PostFindVacancyEditModelResultType.CanClose);
        }

    }
}
