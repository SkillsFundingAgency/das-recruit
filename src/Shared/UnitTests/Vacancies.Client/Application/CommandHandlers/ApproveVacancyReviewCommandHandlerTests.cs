using System.Collections.Generic;
using System.Threading;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class ApproveVacancyReviewCommandHandlerTests
    {
        private readonly Guid _existingReviewId;
        private readonly Fixture _autoFixture = new Fixture();
        private readonly Mock<IVacancyReviewQuery> _mockVacancyReviewQuery;
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly ApproveVacancyReviewCommandHandler _sut;
        private readonly Mock<ICommunicationQueueService> _mockCommunicationQueueService;
        private readonly Mock<IVacancyReviewRepository> _mockVacancyReviewRepository;

        public ApproveVacancyReviewCommandHandlerTests()
        {
            _existingReviewId = Guid.NewGuid();
            _mockVacancyReviewRepository = new Mock<IVacancyReviewRepository>();
            _mockVacancyRepository = new Mock<IVacancyRepository>();

            _mockMessaging = new Mock<IMessaging>();
            var mockValidator = new VacancyReviewValidator();

            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider.Setup(t => t.Now).Returns(DateTime.UtcNow);


            _mockCommunicationQueueService = new Mock<ICommunicationQueueService>();
            _mockVacancyReviewQuery = new Mock<IVacancyReviewQuery>();

            _sut = new ApproveVacancyReviewCommandHandler(Mock.Of<ILogger<ApproveVacancyReviewCommandHandler>>(), _mockVacancyReviewRepository.Object,
                                                        _mockVacancyReviewQuery.Object, _mockVacancyRepository.Object, _mockMessaging.Object, mockValidator, 
                                                        _mockTimeProvider.Object, _mockCommunicationQueueService.Object);
        }

        [Theory]
        [InlineData(ReviewStatus.Closed)]
        [InlineData(ReviewStatus.New)]
        [InlineData(ReviewStatus.PendingReview)]
        public async Task GivenApprovedVacancyReviewCommand_AndVacancyReviewIsNotUnderReview_ThenDoNotProcessApprovingReview(ReviewStatus reviewStatus)
        {
            _mockVacancyReviewQuery.Setup(x => x.GetAsync(_existingReviewId)).ReturnsAsync(new VacancyReview { Status = reviewStatus});

            var command = new ApproveVacancyReviewCommand(_existingReviewId, "comment", new List<ManualQaFieldIndicator>(), new List<Guid>(), new List<ManualQaFieldEditIndicator>());

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(It.IsAny<VacancyReview>()), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyReviewApprovedEvent>()), Times.Never);
        }

        [Theory]
        [InlineData(TransferReason.EmployerRevokedPermission, ClosureReason.TransferredByEmployer)]
        [InlineData(TransferReason.BlockedByQa, ClosureReason.TransferredByQa)]
        public async Task GivenApprovedVacancyReviewCommand_AndVacancyHasBeenTransferredSinceReviewWasCreated_ThenDoNotRaiseVacancyApprovedEventAndCloseVacancy(TransferReason transferReason, ClosureReason expectedClosureReason)
        {
            var transferInfo = new TransferInfo
            {
                Reason = transferReason
            };
            
            var existingVacancy = _autoFixture.Build<Vacancy>()
                                                .With(x => x.TransferInfo, transferInfo)
                                                .Create();

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.Value)).ReturnsAsync(existingVacancy);

            _mockVacancyReviewQuery.Setup(x => x.GetAsync(_existingReviewId)).ReturnsAsync(new VacancyReview
            {
                Id = _existingReviewId,
                CreatedDate = _mockTimeProvider.Object.Now.AddHours(-5),
                Status = ReviewStatus.UnderReview,
                VacancyReference = existingVacancy.VacancyReference.Value,
                VacancySnapshot = new Vacancy()
            });

            var command = new ApproveVacancyReviewCommand(_existingReviewId, "comment", new List<ManualQaFieldIndicator>(), new List<Guid>(), new List<ManualQaFieldEditIndicator>());

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(It.Is<VacancyReview>(r => r.Id == _existingReviewId)), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyReviewApprovedEvent>()), Times.Never);

            existingVacancy.Status.Should().Be(VacancyStatus.Closed);
            existingVacancy.ClosureReason.Should().Be(expectedClosureReason);
            _mockVacancyRepository.Verify(x => x.UpdateAsync(existingVacancy), Times.Once);
        }
    }
}