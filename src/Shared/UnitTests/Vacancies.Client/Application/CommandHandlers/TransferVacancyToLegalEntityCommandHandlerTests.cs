using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using System.Threading.Tasks;
using System.Threading;
using System;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class TransferVacancyToLegalEntityCommandHandlerTests
    {
        private readonly Guid _existingUserGuid = Guid.NewGuid();
        private readonly string _idamsUserId;
        private const string UserEmailAddress = "test@test.com";
        private const string UserName = "FirstName LastName";

        private readonly User _existingUser;
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IBlockedOrganisationQuery> _mockBlockedOrganisationQuery;
        private readonly Mock<IVacancyTransferService> _mockVacancyTransferService;
        private readonly Mock<IVacancyReviewTransferService> _mockVacancyReviewTransferService;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly TransferVacancyToLegalEntityCommandHandler _sut;

        public TransferVacancyToLegalEntityCommandHandlerTests()
        {
            _idamsUserId = _existingUserGuid.ToString();
            _existingUser = new User
            {
                Id = _existingUserGuid,
                IdamsUserId = _idamsUserId,
                UserType = UserType.Employer,
                Email = UserEmailAddress,
                Name = UserName
            };
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockBlockedOrganisationQuery = new Mock<IBlockedOrganisationQuery>();
            _mockVacancyTransferService = new Mock<IVacancyTransferService>();
            _mockVacancyReviewTransferService = new Mock<IVacancyReviewTransferService>();

            _mockTimeProvider = new Mock<ITimeProvider>();

            _mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));

            _mockMessaging = new Mock<IMessaging>();

            _sut = new TransferVacancyToLegalEntityCommandHandler(Mock.Of<ILogger<TransferVacancyToLegalEntityCommandHandler>>(), _mockVacancyRepository.Object,
                                                                _mockUserRepository.Object, _mockBlockedOrganisationQuery.Object, _mockVacancyTransferService.Object, _mockVacancyReviewTransferService.Object,
                                                                _mockTimeProvider.Object, _mockMessaging.Object);
        }

        [Fact]
        public async Task GivenExistingEmployerOwnedVacancy_ThenDoNotProcessTransfer()
        {
            var existingVacancy = GetTestEmployerOwnedVacancy();

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);
            _mockUserRepository.Setup(x => x.GetAsync(_idamsUserId))
                                .ReturnsAsync(_existingUser);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.Value, _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetAsync(_idamsUserId), Times.Once);
            _mockUserRepository.Verify(x => x.UpsertUserAsync(It.IsAny<User>()), Times.Never);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Never);
        }

        [Fact]
        public async Task GivenExistingProviderOwnedVacancy_AndExistingUser_ThenProcessTransfer()
        {
            var existingVacancy = GetTestProviderOwnedVacancy();

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);
            _mockUserRepository.Setup(x => x.GetAsync(_idamsUserId))
                                .ReturnsAsync(_existingUser);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetAsync(_idamsUserId), Times.Once);
            _mockUserRepository.Verify(x => x.UpsertUserAsync(It.IsAny<User>()), Times.Never);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Once);
        }

        [Fact]
        public async Task GivenExistingProviderOwnedVacancy_AndUnknownUser_ThenProcessTransfer()
        {
            var existingVacancy = GetTestProviderOwnedVacancy();

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);
            _mockUserRepository.Setup(x => x.GetAsync(_idamsUserId))
                                .Returns(Task.FromResult<User>(null));

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetAsync(_idamsUserId), Times.Once);
            _mockUserRepository.Verify(x => x.UpsertUserAsync(It.Is<User>(u => u.IdamsUserId == _idamsUserId)), Times.Once);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Once);
        }

        [Fact]
        public async Task GivenExistingSubmittedProviderOwnedVacancy_AndExistingUser_ThenProcessTransferAndVerifyVacancyReviewClosure()
        {
            var existingVacancy = GetTestProviderOwnedVacancy();
            existingVacancy.Status = VacancyStatus.Submitted;
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);
            _mockUserRepository.Setup(x => x.GetAsync(_idamsUserId))
                                .ReturnsAsync(_existingUser);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetAsync(_idamsUserId), Times.Once);
            _mockUserRepository.Verify(x => x.UpsertUserAsync(It.Is<User>(u => u.IdamsUserId == _idamsUserId)), Times.Never);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockVacancyReviewTransferService.Verify(x => x.CloseVacancyReview(existingVacancy.VacancyReference.Value, false), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyClosedEvent>()), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Once);
        }

        [Fact]
        public async Task GivenExistingSubmittedProviderOwnedVacancy_AndExistingUserAndBlockedProvider_ThenProcessTransferAndVerifyVacancyReviewClosure()
        {
            var existingVacancy = GetTestProviderOwnedVacancy();
            existingVacancy.Status = VacancyStatus.Submitted;
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);
            _mockUserRepository.Setup(x => x.GetAsync(_idamsUserId))
                                .ReturnsAsync(_existingUser);
            _mockBlockedOrganisationQuery.Setup(x => x.GetByOrganisationIdAsync(existingVacancy.TrainingProvider.Ukprn.Value.ToString()))
                                            .ReturnsAsync(new BlockedOrganisation { BlockedStatus = BlockedStatus.Blocked });

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetAsync(_idamsUserId), Times.Once);
            _mockUserRepository.Verify(x => x.UpsertUserAsync(It.Is<User>(u => u.IdamsUserId == _idamsUserId)), Times.Never);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockVacancyReviewTransferService.Verify(x => x.CloseVacancyReview(existingVacancy.VacancyReference.Value, true), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyClosedEvent>()), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Once);
        }

        [Fact]
        public async Task GivenExistingLiveProviderOwnedVacancy_AndExistingUser_ThenProcessTransferAndVerifyVacancyClosedEventRaised()
        {
            var existingVacancy = GetTestProviderOwnedVacancy();
            existingVacancy.Status = VacancyStatus.Live;
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);
            _mockUserRepository.Setup(x => x.GetAsync(_idamsUserId))
                                .ReturnsAsync(_existingUser);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetAsync(_idamsUserId), Times.Once);
            _mockUserRepository.Verify(x => x.UpsertUserAsync(It.Is<User>(u => u.IdamsUserId == _idamsUserId)), Times.Never);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockVacancyReviewTransferService.Verify(x => x.CloseVacancyReview(existingVacancy.VacancyReference.Value, false), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyClosedEvent>()), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Once);
        }

        private Vacancy GetTestProviderOwnedVacancy()
        {
            var vacancy = new Vacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.VacancyReference = 10000001;
            vacancy.OwnerType = OwnerType.Provider;
            vacancy.TrainingProvider = new TrainingProvider { Ukprn = 11111111, Name = "TestProvider" };
            return vacancy;
        }

        private Vacancy GetTestEmployerOwnedVacancy()
        {
            var vacancy = new Vacancy();
            vacancy.Id = Guid.NewGuid();
            vacancy.VacancyReference = 10000001;
            vacancy.OwnerType = OwnerType.Employer;
            return vacancy;
        }
    }
}