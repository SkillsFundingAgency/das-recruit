using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.Jobs
{
    public class TransferVacanciesFromProviderJobTests
    {
        private readonly Mock<IVacancyQuery> _mockVacancyQuery;
        private readonly Mock<IRecruitQueueService> _mockRecruitQueueService;
        private readonly TransferVacanciesFromProviderJob _sut;

        public TransferVacanciesFromProviderJobTests()
        {
            _mockVacancyQuery = new Mock<IVacancyQuery>();
            _mockRecruitQueueService = new Mock<IRecruitQueueService>();
            _sut = new TransferVacanciesFromProviderJob(_mockVacancyQuery.Object, _mockRecruitQueueService.Object);
        }

        [Fact]
        public async Task GivenRequest_WhenNoMatchingVacanciesToTransfer_ThenShouldNotQueueAnyTransfers()
        {
            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesForLegalEntityAsync(It.IsAny<long>(), It.IsAny<long>()))
                                .ReturnsAsync(Enumerable.Empty<Vacancy>());

            await _sut.Run(1, 1, Guid.NewGuid(), string.Empty, string.Empty, TransferReason.EmployerRevokedPermission);

            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()), Times.Never);
        }

        [Fact]
        public async Task GivenRequest_WhenMatchingVacanciesToTransfer_ThenShouldNotQueueAnyTransfers()
        {
            const int NoOfMatchingVacancies = 99;
            var userRef = Guid.NewGuid();
            const string UserEmail = "test@test.com";
            const string UserName = "John Smith";
            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesForLegalEntityAsync(It.IsAny<long>(), It.IsAny<long>()))
                                .ReturnsAsync(new Fixture().CreateMany<Vacancy>(NoOfMatchingVacancies));

            await _sut.Run(1, 1, userRef, UserEmail, UserName, TransferReason.EmployerRevokedPermission);

            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()), Times.Exactly(NoOfMatchingVacancies));
        }
    }
}