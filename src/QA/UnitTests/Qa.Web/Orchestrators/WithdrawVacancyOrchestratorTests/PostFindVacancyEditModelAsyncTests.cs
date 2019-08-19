using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Models.WithdrawVacancy;
using Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Qa.Web.Orchestrators;
using FluentAssertions;
using Moq;
using Xunit;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace UnitTests.Qa.Web.Orchestrators.WithdrawVacancyOrchestratorTests
{
    public class PostFindVacancyEditModelAsyncTests
    {
        private const long VacancyReference = 12345678;
        private readonly Mock<IMessaging> _messagingMock = new Mock<IMessaging>();
        private readonly Mock<IQaVacancyClient> _clientMock = new Mock<IQaVacancyClient>();
        private readonly WithdrawVacancyOrchestrator _sut;
        public PostFindVacancyEditModelAsyncTests()
        {
            _sut = new WithdrawVacancyOrchestrator(_clientMock.Object, _messagingMock.Object);
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Submitted)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Approved)]
        public async Task ShouldReturnNotLiveIfVacancyExistsAndIsNotLive(VacancyStatus status)
        {
            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy {Status = status});

            var orch = _sut;

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
            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ThrowsAsync(new VacancyNotFoundException("not found"));

            var orch = _sut;

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
            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Closed });

            var orch = _sut;

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
            _clientMock.Setup(c => c.GetVacancyAsync(VacancyReference))
                .ReturnsAsync(new Vacancy { Status = VacancyStatus.Live });

            var orch = _sut;

            var m = new FindVacancyEditModel
            {
                VacancyReference = VacancyReference.ToString()
            };

            var result = await orch.PostFindVacancyEditModelAsync(m);

            result.ResultType.Should().Be(PostFindVacancyEditModelResultType.CanClose);
        }

    }
}
