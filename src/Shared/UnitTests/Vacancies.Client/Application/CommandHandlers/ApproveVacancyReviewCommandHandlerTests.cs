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
using System.Collections.Generic;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    [Trait("Category", "Unit")]
    public class ApproveVacancyReviewCommandHandlerTests
    {
        private readonly Guid _existingReviewId;
        private readonly Mock<IVacancyReviewRepository> _mockVacancyReviewRepository;
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly VacancyReviewValidator _mockValidator;
        private readonly ApproveVacancyReviewCommandHandler _sut;

        public ApproveVacancyReviewCommandHandlerTests()
        {
            _existingReviewId = Guid.NewGuid();
            _mockVacancyReviewRepository = new Mock<IVacancyReviewRepository>();
            _mockVacancyRepository = new Mock<IVacancyRepository>();

            _mockMessaging = new Mock<IMessaging>();
            _mockValidator = new VacancyReviewValidator();

            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider.Setup(t => t.Today).Returns(DateTime.Parse("2019-03-24"));

            _sut = new ApproveVacancyReviewCommandHandler(Mock.Of<ILogger<ApproveVacancyReviewCommandHandler>>(), _mockVacancyReviewRepository.Object,
                                                        _mockVacancyRepository.Object, _mockMessaging.Object, _mockValidator, _mockTimeProvider.Object);
        }

        [Theory]
        [InlineData(ReviewStatus.Closed)]
        [InlineData(ReviewStatus.New)]
        [InlineData(ReviewStatus.PendingReview)]
        public async Task GivenApprovedVacancyReviewCommand_AndVacancyReviewIsNotUnderReview_ThenDoNotProcessApprovingReview(ReviewStatus reviewStatus)
        {
            _mockVacancyReviewRepository.Setup(x => x.GetAsync(_existingReviewId)).ReturnsAsync(new VacancyReview { Status = reviewStatus});

            var command = new ApproveVacancyReviewCommand(_existingReviewId, "comment", new List<ManualQaFieldIndicator>() {}, new List<Guid>());

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(It.IsAny<VacancyReview>()), Times.Never);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyReviewApprovedEvent>()), Times.Never);
        }

        [Fact]
        public async Task GivenApprovedVacancyReviewCommand_AndVacancyIsDraft_ThenDoNotRaiseVacancyApprovedEvent()
        {
            var newVacancyId = Guid.NewGuid();
            var existingVacancy = GetTestVacancy(VacancyStatus.Draft);

            _mockVacancyRepository.Setup(x => x.GetVacancyAsync(existingVacancy.VacancyReference.Value)).ReturnsAsync(existingVacancy);

            _mockVacancyReviewRepository.Setup(x => x.GetAsync(_existingReviewId)).ReturnsAsync(new VacancyReview
            {
                Id = _existingReviewId,
                Status = ReviewStatus.UnderReview,
                VacancyReference = existingVacancy.VacancyReference.Value
            });

            var command = new ApproveVacancyReviewCommand(_existingReviewId, "comment", new List<ManualQaFieldIndicator>() {}, new List<Guid>());

            await _sut.Handle(command, CancellationToken.None);

            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(It.Is<VacancyReview>(r => r.Id == _existingReviewId)), Times.Once);
            _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<VacancyReviewApprovedEvent>()), Times.Never);
        }

        private static Vacancy GetTestVacancy(VacancyStatus status)
        {
            var fixture = new Fixture();
            var vacancy = fixture.Create<Vacancy>();
            vacancy.Status = status;
            return vacancy;
        }
    }
}