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
    public class TransferProviderVacancyToLegalEntityCommandHandlerTests
    {
        private readonly Guid _existingUserGuid = Guid.NewGuid();
        private const string UserEmailAddress = "test@test.com";
        private const string UserName = "FirstName LastName";
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<IVacancyTransferService> _mockVacancyTransferService;
        private readonly Mock<IVacancyReviewTransferService> _mockVacancyReviewTransferService;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly TransferProviderVacancyToLegalEntityCommandHandler _sut;

        public TransferProviderVacancyToLegalEntityCommandHandlerTests()
        {
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockVacancyTransferService = new Mock<IVacancyTransferService>();
            _mockVacancyReviewTransferService = new Mock<IVacancyReviewTransferService>();

            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));

            _mockMessaging = new Mock<IMessaging>();

            _sut = new TransferProviderVacancyToLegalEntityCommandHandler(Mock.Of<ILogger<TransferProviderVacancyToLegalEntityCommandHandler>>(), _mockVacancyRepository.Object,
                                                                _mockVacancyTransferService.Object, _mockVacancyReviewTransferService.Object,
                                                                _mockTimeProvider.Object, _mockMessaging.Object);
        }

        [Fact]
        public async Task GivenExistingEmployerOwnedVacancy_ThenDoNotProcessTransfer()
        {
            var existingVacancy = GetTestEmployerOwnedVacancy();

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.Value, _existingUserGuid, UserEmailAddress, UserName, TransferReason.BlockedByQa);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Never);
        }

        [Theory]
        [InlineData(TransferReason.BlockedByQa)]
        [InlineData(TransferReason.EmployerRevokedProviderPermission)]
        public async Task GivenExistingSubmittedProviderOwnedVacancy_ThenProcessTransferAndVerifyVacancyReviewClosure(TransferReason transferReason)
        {
            var existingVacancy = GetTestProviderOwnedVacancy();
            existingVacancy.Status = VacancyStatus.Submitted;
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName, transferReason);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockVacancyReviewTransferService.Verify(x => x.CloseVacancyReview(existingVacancy.VacancyReference.Value, transferReason), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyClosedEvent>()), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyTransferredEvent>()), Times.Once);
        }

        [Theory]
        [InlineData(TransferReason.BlockedByQa)]
        [InlineData(TransferReason.EmployerRevokedProviderPermission)]
        public async Task GivenExistingLiveProviderOwnedVacancy_ThenProcessTransferAndVerifyVacancyClosedEventRaised(TransferReason transferReason)
        {
            var existingVacancy = GetTestProviderOwnedVacancy();
            existingVacancy.Status = VacancyStatus.Live;
            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.GetValueOrDefault()))
                                    .ReturnsAsync(existingVacancy);

            var command = new TransferVacancyToLegalEntityCommand(existingVacancy.VacancyReference.GetValueOrDefault(), _existingUserGuid, UserEmailAddress, UserName, transferReason);

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyRepository.Verify(x => x.GetVacancyAsync(It.IsAny<long>()), Times.Once);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
            _mockVacancyReviewTransferService.Verify(x => x.CloseVacancyReview(existingVacancy.VacancyReference.Value, transferReason), Times.Never);
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