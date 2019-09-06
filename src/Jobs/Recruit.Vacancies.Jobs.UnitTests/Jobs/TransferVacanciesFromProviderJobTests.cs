using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.Jobs
{
    public class TransferVacanciesFromProviderJobTests
    {
        private readonly Mock<IVacancyQuery> _mockVacancyQuery;
        private readonly Mock<IRecruitQueueService> _mockRecruitQueueService;
        private readonly Mock<IQueryStoreReader> _mockQueryStoreReader;
        private readonly TransferVacanciesFromProviderJob _sut;
        
        public TransferVacanciesFromProviderJobTests()
        {
            _mockVacancyQuery = new Mock<IVacancyQuery>();
            _mockRecruitQueueService = new Mock<IRecruitQueueService>();
            _mockQueryStoreReader = new Mock<IQueryStoreReader>();

            _sut = new TransferVacanciesFromProviderJob(_mockVacancyQuery.Object, _mockRecruitQueueService.Object, _mockQueryStoreReader.Object);
        }

        [Fact]
        public async Task GivenRequest_WhenNoMatchingVacanciesToTransfer_AndProviderHasManyLegalEntityPermissions_ThenShouldNotQueueAnyTransfers()
        {
            const long Ukprn = 1;
            const long LegalEntityId = 2;
            const string EmployerAccountId = "employer-account-id";

            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesForLegalEntityAsync(Ukprn, LegalEntityId))
                                .ReturnsAsync(Enumerable.Empty<Vacancy>());

            var employerInfo = new EmployerInfo
            {
                LegalEntities = new List<LegalEntity>
                {
                    new LegalEntity{LegalEntityId = LegalEntityId},
                    new LegalEntity{LegalEntityId = 12345}
                }
            };

            _mockQueryStoreReader.Setup(q => q.GetProviderEmployerVacancyDataAsync(Ukprn, EmployerAccountId))
                .ReturnsAsync(employerInfo);

            await _sut.Run(Ukprn, EmployerAccountId, LegalEntityId, Guid.NewGuid(), string.Empty, string.Empty, TransferReason.EmployerRevokedPermission);

            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()), Times.Never);
            _mockVacancyQuery.Verify(x => x.GetProviderOwnedVacanciesForEmployerWithoutLegalEntityAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GivenRequest_WhenMatchingVacanciesToTransfer_AndProviderHasSingleLegalEntityPermission_ThenShouldQueueTransfers()
        {
            const int NoOfMatchingVacancies = 99;
            const int NoOfVacanciesWithoutLegalEntity = 3;
            var userRef = Guid.NewGuid();
            const string UserEmail = "test@test.com";
            const string UserName = "John Smith";
            const long Ukprn = 1;
            const long LegalEntityId = 2;
            const string EmployerAccountId = "employer-account-id";

            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesForLegalEntityAsync(Ukprn, LegalEntityId))
                .ReturnsAsync(new Fixture().CreateMany<Vacancy>(NoOfMatchingVacancies));

            var employerInfo = new EmployerInfo
            {
                LegalEntities = new List<LegalEntity>
                {
                    new LegalEntity{LegalEntityId = LegalEntityId}
                }
            };

            _mockQueryStoreReader.Setup(q => q.GetProviderEmployerVacancyDataAsync(Ukprn, EmployerAccountId))
                .ReturnsAsync(employerInfo);

            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesForEmployerWithoutLegalEntityAsync(Ukprn, EmployerAccountId))
                .ReturnsAsync(new Fixture().CreateMany<Vacancy>(NoOfVacanciesWithoutLegalEntity));

            await _sut.Run(Ukprn, EmployerAccountId, LegalEntityId, userRef, UserEmail, UserName, TransferReason.EmployerRevokedPermission);

            var expectedCallCount = NoOfMatchingVacancies + NoOfVacanciesWithoutLegalEntity;
            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()), Times.Exactly(expectedCallCount));
        }

        [Fact]
        public async Task GivenRequest_WhenMatchingVacanciesToTransfer_AndProviderHasManyLegalEntityPermissions_ThenShouldQueueTransfers()
        {
            const int NoOfMatchingVacancies = 99;
            var userRef = Guid.NewGuid();
            const string UserEmail = "test@test.com";
            const string UserName = "John Smith";
            const long Ukprn = 1;
            const long LegalEntityId = 2;
            const string EmployerAccountId = "employer-account-id";

            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesForLegalEntityAsync(Ukprn, LegalEntityId))
                .ReturnsAsync(new Fixture().CreateMany<Vacancy>(NoOfMatchingVacancies));

            var employerInfo = new EmployerInfo
            {
                LegalEntities = new List<LegalEntity>
                {
                    new LegalEntity{LegalEntityId = LegalEntityId},
                    new LegalEntity{LegalEntityId = 1234}
                }
            };

            _mockQueryStoreReader.Setup(q => q.GetProviderEmployerVacancyDataAsync(Ukprn, EmployerAccountId))
                .ReturnsAsync(employerInfo);
            
            await _sut.Run(Ukprn, EmployerAccountId, LegalEntityId, userRef, UserEmail, UserName, TransferReason.EmployerRevokedPermission);

            _mockVacancyQuery.Verify(x => x.GetProviderOwnedVacanciesForEmployerWithoutLegalEntityAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacancyToLegalEntityQueueMessage>()), Times.Exactly(NoOfMatchingVacancies));
        }
    }
}