using System;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services
{
    public class VacancyReviewTransferServiceTests
    {
        private readonly Fixture _autoFixture = new Fixture();
        private readonly Mock<IVacancyReviewRepository> _mockVacancyReviewRepository;
        private readonly Mock<IVacancyReviewQuery> _mockVacancyReviewQuery;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private VacancyReviewTransferService _sut;

        public VacancyReviewTransferServiceTests()
        {
            _mockVacancyReviewRepository = new Mock<IVacancyReviewRepository>();
            _mockVacancyReviewQuery = new Mock<IVacancyReviewQuery>();
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider.Setup(t => t.Now).Returns(DateTime.Now);

            _sut = new VacancyReviewTransferService(Mock.Of<ILogger<VacancyReviewTransferService>>(), _mockVacancyReviewQuery.Object,
                                                    _mockVacancyReviewRepository.Object, _mockTimeProvider.Object);
        }

        [Theory]
        [InlineData(ReviewStatus.New)]
        [InlineData(ReviewStatus.PendingReview)]
        public async Task GivenVacancyReview_AndDueToProviderPermissionRevokeTransfer_ThenSetClosedStatusOfVacancyReview(ReviewStatus reviewStatus)
        {
            var vacancyReview = _autoFixture.Build<VacancyReview>()
                                            .Without(x => x.AutomatedQaOutcome)
                                            .With(x => x.Status, reviewStatus)
                                            .Create();
            _mockVacancyReviewQuery.Setup(x => x.GetLatestReviewByReferenceAsync(vacancyReview.VacancyReference))
                                    .ReturnsAsync(vacancyReview);

            await _sut.CloseVacancyReview(vacancyReview.VacancyReference, TransferReason.EmployerRevokedPermission);

            vacancyReview.ManualOutcome.Should().Be(ManualQaOutcome.Transferred);
            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(vacancyReview), Times.Once);
        }

        [Fact]
        public async Task GivenClosedVacancyReview_AndDueToProviderPermissionRevokeTransfer_ThenVacancyReviewIsNotUpdated()
        {
            var vacancyReview = _autoFixture.Build<VacancyReview>()
                                            .Without(x => x.AutomatedQaOutcome)
                                            .With(x => x.Status, ReviewStatus.Closed)
                                            .Create();
            _mockVacancyReviewQuery.Setup(x => x.GetLatestReviewByReferenceAsync(vacancyReview.VacancyReference))
                                    .ReturnsAsync(vacancyReview);

            await _sut.CloseVacancyReview(vacancyReview.VacancyReference, TransferReason.EmployerRevokedPermission);

            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(vacancyReview), Times.Never);
        }

        [Fact]
        public async Task GivenUnderPendingReviewVacancyReview_AndeDueToProviderPermissionRevokeTransfer_ThenVacancyReviewIsNotClosed()
        {
            var vacancyReview = _autoFixture.Build<VacancyReview>()
                                            .Without(x => x.AutomatedQaOutcome)
                                            .Without(x => x.ManualOutcome)
                                            .Without(x => x.ClosedDate)
                                            .With(x => x.Status, ReviewStatus.UnderReview)
                                            .Create();
            _mockVacancyReviewQuery.Setup(x => x.GetLatestReviewByReferenceAsync(vacancyReview.VacancyReference))
                                    .ReturnsAsync(vacancyReview);

            await _sut.CloseVacancyReview(vacancyReview.VacancyReference, TransferReason.EmployerRevokedPermission);

            vacancyReview.ManualOutcome.Should().BeNull();
            vacancyReview.Status.Should().Be(ReviewStatus.UnderReview);
            vacancyReview.ClosedDate.Should().BeNull();
            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(vacancyReview), Times.Never);
        }

        [Theory]
        [InlineData(ReviewStatus.New)]
        [InlineData(ReviewStatus.PendingReview)]
        public async Task GivenToBeReviewedVacancyReview_AndDueToProviderPermissionRevokeTransfer_ThenSetClosedStatusOfVacancyReview(ReviewStatus vacancyReviewStatus)
        {
            var vacancyReview = _autoFixture.Build<VacancyReview>()
                                            .Without(x => x.AutomatedQaOutcome)
                                            .Without(x => x.ManualOutcome)
                                            .With(x => x.Status, vacancyReviewStatus)
                                            .Create();
            _mockVacancyReviewQuery.Setup(x => x.GetLatestReviewByReferenceAsync(vacancyReview.VacancyReference))
                                    .ReturnsAsync(vacancyReview);

            await _sut.CloseVacancyReview(vacancyReview.VacancyReference, TransferReason.EmployerRevokedPermission);

            vacancyReview.Status.Should().Be(ReviewStatus.Closed);
            vacancyReview.ClosedDate.Should().Be(_mockTimeProvider.Object.Now);
            _mockVacancyReviewRepository.Verify(x => x.UpdateAsync(vacancyReview), Times.Once);
        }
    }
}