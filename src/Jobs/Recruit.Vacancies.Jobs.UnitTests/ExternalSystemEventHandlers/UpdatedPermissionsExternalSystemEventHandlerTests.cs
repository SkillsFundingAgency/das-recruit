using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Jobs.ExternalSystemEventHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;
using Xunit;

namespace Recruit.Vacancies.Jobs.UnitTests.ExternalSystemEventHandlers
{
    public class UpdatedPermissionsExternalSystemEventHandlerTests
    {
        private const string UserEmailAddress = "JohnSmith@test.com";
        private const string UserFirstName = "John";
        private const string UserLastName = "Smith";
        private const long EmployerAccountId = 12345;
        private const string EmployerAccountIdEncoded = "ABC123";
        private const long AccountLegalEntityId = 1231;
        private const long AccountProviderId = 321;
        private const long AccountProviderLegalEntityId = 1223;
        private const long Ukprn = 11111111;
        private const long RecruitLegalEntityId = 5678;
        private const int NoOfDummyLegalEntitiesToCreate = 10;
        private readonly Fixture _autoFixture = new Fixture();
        private readonly IEnumerable<AccountLegalEntity> _dummyLegalEntities;
        private readonly Mock<IRecruitQueueService> _mockRecruitQueueService;
        private readonly Mock<IEmployerAccountProvider> _mockEmployerAccountProvider;
        private readonly Mock<IEncodingService> _mockEncoder;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly UpdatedPermissionsExternalSystemEventsHandler _sut;

        public UpdatedPermissionsExternalSystemEventHandlerTests()
        {
            _dummyLegalEntities = _autoFixture.CreateMany<AccountLegalEntity>(NoOfDummyLegalEntitiesToCreate);
            _mockRecruitQueueService = new Mock<IRecruitQueueService>();
            _mockEmployerAccountProvider = new Mock<IEmployerAccountProvider>();
            _mockEncoder = new Mock<IEncodingService>();
            _mockMessaging = new Mock<IMessaging>();

            _sut = new UpdatedPermissionsExternalSystemEventsHandler(Mock.Of<ILogger<UpdatedPermissionsExternalSystemEventsHandler>>(),
                                                        _mockRecruitQueueService.Object, _mockEmployerAccountProvider.Object,
                                                        _mockEncoder.Object, _mockMessaging.Object);
        }

        [Fact]
        public async Task GivenEventWithDefaultGuidUserRef_ThenVerifyNoDependenciesAreCalled()
        {
            var grantedOperations = new HashSet<Operation> { Operation.Recruitment, Operation.RecruitmentRequiresReview };
            var previousOperations = new HashSet<Operation>();

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, Guid.Empty, string.Empty, string.Empty, string.Empty, grantedOperations, previousOperations, DateTime.UtcNow), null);

            _mockRecruitQueueService.VerifyNoOtherCalls();
            _mockEmployerAccountProvider.VerifyNoOtherCalls();
            _mockEncoder.VerifyNoOtherCalls();
            _mockMessaging.Verify(x => x.SendCommandAsync(It.Is<SetupProviderCommand>(c => c.Ukprn == Ukprn)), Times.Once);
        }

        [Fact]
        public async Task GivenEventWithGrantedRecruitmentPermissions_ThenVerifyNoDependenciesAreCalled()
        {
            var grantedOperations = new HashSet<Operation> { Operation.Recruitment, Operation.RecruitmentRequiresReview };
            var previousOperations = new HashSet<Operation>();

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, Guid.NewGuid(), UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null);

            _mockRecruitQueueService.VerifyNoOtherCalls();
            _mockEmployerAccountProvider.VerifyNoOtherCalls();
            _mockEncoder.VerifyNoOtherCalls();
            _mockMessaging.Verify(x => x.SendCommandAsync(It.Is<SetupProviderCommand>(c => c.Ukprn == Ukprn)), Times.Once);
        }

        [Fact]
        public async Task GivenEventWithGrantedRecruitmentPermissionsAndMore_ThenVerifyNoDependenciesAreCalled()
        {
            var grantedOperations = new HashSet<Operation> { Operation.Recruitment, Operation.RecruitmentRequiresReview, Operation.CreateCohort };
            var previousOperations = new HashSet<Operation>();

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, Guid.NewGuid(), UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null);

            _mockRecruitQueueService.VerifyNoOtherCalls();
            _mockEmployerAccountProvider.VerifyNoOtherCalls();
            _mockEncoder.VerifyNoOtherCalls();
            _mockMessaging.Verify(x => x.SendCommandAsync(It.Is<SetupProviderCommand>(c => c.Ukprn == Ukprn)), Times.Once);
        }

        [Fact]
        public async Task GivenNotExistingLegalEntityId_ThenExceptionIsThrown()
        {
            var grantedOperations = new HashSet<Operation> { Operation.CreateCohort };
            var previousOperations = new HashSet<Operation>();

            _mockEncoder.Setup(x => x.Encode(EmployerAccountId, EncodingType.AccountId)).Returns(EmployerAccountIdEncoded);
            _mockEmployerAccountProvider.Setup(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded))
                                        .ReturnsAsync(_dummyLegalEntities);

            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, Guid.NewGuid(), UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null));

            exception.Message.Should().Be($"Could not find matching Account Legal Entity Id {AccountLegalEntityId} for Employer Account {EmployerAccountId}");
            _mockEncoder.Verify(x => x.Encode(EmployerAccountId, EncodingType.AccountId), Times.Once);
            _mockEmployerAccountProvider.Verify(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded), Times.Once);
        }

        [Fact]
        public async Task GivenMatchingExistingLegalEntityId_ThenVerifyTransferProcessIsQueued()
        {
            var matchingLegalEntityViewModel = _autoFixture.Build<AccountLegalEntity>()
                                                            .With(l => l.DasAccountId, EmployerAccountIdEncoded)
                                                            .With(l => l.AccountLegalEntityId, AccountLegalEntityId)
                                                            .With(l => l.LegalEntityId, RecruitLegalEntityId)
                                                            .Create();

            var grantedOperations = new HashSet<Operation> { Operation.CreateCohort };
            var previousOperations = new HashSet<Operation>();

            _mockEncoder.Setup(x => x.Encode(EmployerAccountId, EncodingType.AccountId)).Returns(EmployerAccountIdEncoded);
            _mockEmployerAccountProvider.Setup(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded))
                                        .ReturnsAsync(_dummyLegalEntities.Append(matchingLegalEntityViewModel));

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, Guid.NewGuid(), UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null);

            _mockEncoder.Verify(x => x.Encode(EmployerAccountId, EncodingType.AccountId), Times.Once);
            _mockEmployerAccountProvider.Verify(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded), Times.Once);
            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacanciesFromProviderQueueMessage>()), Times.Once);
            _mockMessaging.Verify(x => x.SendCommandAsync(It.Is<SetupProviderCommand>(c => c.Ukprn == Ukprn)), Times.Once);
        }

        [Fact]
        public async Task GivenMatchingExistingLegalEntityId_ThenVerifyTransferReviewProcessIsQueued()
        {
            var matchingLegalEntityViewModel = _autoFixture.Build<AccountLegalEntity>()
                .With(l => l.DasAccountId, EmployerAccountIdEncoded)
                .With(l => l.AccountLegalEntityId, AccountLegalEntityId)
                .With(l => l.LegalEntityId, RecruitLegalEntityId)
                .Create();

            var grantedOperations = new HashSet<Operation> { Operation.Recruitment, Operation.CreateCohort };
            var previousOperations = new HashSet<Operation>();

            _mockEncoder.Setup(x => x.Encode(EmployerAccountId, EncodingType.AccountId)).Returns(EmployerAccountIdEncoded);
            _mockEmployerAccountProvider.Setup(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded))
                .ReturnsAsync(_dummyLegalEntities.Append(matchingLegalEntityViewModel));

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, Guid.NewGuid(), UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null);

            _mockEncoder.Verify(x => x.Encode(EmployerAccountId, EncodingType.AccountId), Times.Once);
            _mockEmployerAccountProvider.Verify(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded), Times.Once);
            _mockRecruitQueueService.Verify(x => x.AddMessageAsync(It.IsAny<TransferVacanciesFromEmployerReviewToQAReviewQueueMessage>()), Times.Once);
            _mockMessaging.Verify(x => x.SendCommandAsync(It.Is<SetupProviderCommand>(c => c.Ukprn == Ukprn)), Times.Once);
        }

        [Fact]
        public async Task GivenMatchingExistingLegalEntityId_ThenTransferQueuedMessageIsMappedCorrectly()
        {
            var matchingLegalEntityViewModel = _autoFixture.Build<AccountLegalEntity>()
                                                            .With(l => l.DasAccountId, EmployerAccountIdEncoded)
                                                            .With(l => l.AccountLegalEntityId, AccountLegalEntityId)
                                                            .With(l => l.LegalEntityId, RecruitLegalEntityId)
                                                            .Create();

            var grantedOperations = new HashSet<Operation> { Operation.CreateCohort };
            var previousOperations = new HashSet<Operation>();
            _mockEncoder.Setup(x => x.Encode(EmployerAccountId, EncodingType.AccountId)).Returns(EmployerAccountIdEncoded);
            _mockEmployerAccountProvider.Setup(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded))
                                        .ReturnsAsync(_dummyLegalEntities.Append(matchingLegalEntityViewModel));

            var userRef = Guid.NewGuid();

            TransferVacanciesFromProviderQueueMessage queuedMessage = null;
            _mockRecruitQueueService.Setup(mock => mock.AddMessageAsync(It.IsAny<TransferVacanciesFromProviderQueueMessage>())).Callback((TransferVacanciesFromProviderQueueMessage message) =>
            {
                queuedMessage = message;
            });

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, userRef, UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null);          

            Assert.NotNull(queuedMessage);
            Assert.Equal(Ukprn, queuedMessage.Ukprn);
            Assert.Equal(EmployerAccountIdEncoded, queuedMessage.EmployerAccountId);
            Assert.Equal(matchingLegalEntityViewModel.AccountLegalEntityPublicHashedId, queuedMessage.AccountLegalEntityPublicHashedId);
            Assert.Equal(userRef, queuedMessage.UserRef);
            Assert.Equal(UserEmailAddress, queuedMessage.UserEmailAddress);
            Assert.Equal($"{UserFirstName} {UserLastName}", queuedMessage.UserName);
            Assert.Equal(TransferReason.EmployerRevokedPermission, queuedMessage.TransferReason);
        }

        [Fact]
        public async Task GivenMatchingExistingLegalEntityId_ThenReviewQueuedMessageIsMappedCorrectly()
        {
            var matchingLegalEntityViewModel = _autoFixture.Build<AccountLegalEntity>()
                                                            .With(l => l.DasAccountId, EmployerAccountIdEncoded)
                                                            .With(l => l.AccountLegalEntityId, AccountLegalEntityId)
                                                            .With(l => l.LegalEntityId, RecruitLegalEntityId)
                                                            .Create();

            var grantedOperations = new HashSet<Operation> { Operation.Recruitment, Operation.CreateCohort };
            var previousOperations = new HashSet<Operation>();
            _mockEncoder.Setup(x => x.Encode(EmployerAccountId, EncodingType.AccountId)).Returns(EmployerAccountIdEncoded);
            _mockEmployerAccountProvider.Setup(x => x.GetLegalEntitiesConnectedToAccountAsync(EmployerAccountIdEncoded))
                                        .ReturnsAsync(_dummyLegalEntities.Append(matchingLegalEntityViewModel));

            var userRef = Guid.NewGuid();

            TransferVacanciesFromEmployerReviewToQAReviewQueueMessage queuedMessage = null;
            _mockRecruitQueueService.Setup(mock => mock.AddMessageAsync(It.IsAny<TransferVacanciesFromEmployerReviewToQAReviewQueueMessage>())).Callback((TransferVacanciesFromEmployerReviewToQAReviewQueueMessage message) =>
            {
                queuedMessage = message;
            });

            await _sut.Handle(new UpdatedPermissionsEvent(EmployerAccountId, AccountLegalEntityId, AccountProviderId, AccountProviderLegalEntityId, Ukprn, userRef, UserEmailAddress, UserFirstName, UserLastName, grantedOperations, previousOperations, DateTime.UtcNow), null);

            Assert.NotNull(queuedMessage);
            Assert.Equal(Ukprn, queuedMessage.Ukprn);
            Assert.Equal(matchingLegalEntityViewModel.AccountLegalEntityPublicHashedId, queuedMessage.AccountLegalEntityPublicHashedId);
            Assert.Equal(userRef, queuedMessage.UserRef);
            Assert.Equal(UserEmailAddress, queuedMessage.UserEmailAddress);
            Assert.Equal($"{UserFirstName} {UserLastName}", queuedMessage.UserName);
        }
    }
}