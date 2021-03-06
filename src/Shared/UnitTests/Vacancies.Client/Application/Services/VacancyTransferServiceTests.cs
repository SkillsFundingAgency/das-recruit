using System;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services
{
    public class VacancyTransferServiceTests
    {
        private readonly Fixture _autoFixture = new Fixture();
        private const string TodaysDate = "2019-03-24";
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<IVacancyReviewQuery> _mockVacancyReviewQuery;
        private VacancyTransferService _sut;

        public VacancyTransferServiceTests()
        {
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockTimeProvider.Setup(t => t.Now).Returns(DateTime.Parse(TodaysDate));
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockVacancyReviewQuery = new Mock<IVacancyReviewQuery>();
            _sut = new VacancyTransferService(_mockTimeProvider.Object, _mockVacancyRepository.Object, _mockVacancyReviewQuery.Object);
        }

        [Fact]
        public async Task GivenProviderOwnedVacancyToTransfer_WhenProviderIsNotBlocked_ThenSetTransferInfoOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.TransferInfo.Should().NotBeNull();
            vacancy.TransferInfo.Reason.Should().Be(TransferReason.EmployerRevokedPermission);
            vacancy.TransferInfo.Ukprn.Should().Be(vacancy.TrainingProvider.Ukprn);
            vacancy.TransferInfo.ProviderName.Should().Be(vacancy.TrainingProvider.Name);
            vacancy.TransferInfo.LegalEntityName.Should().Be(vacancy.LegalEntityName);
            vacancy.TransferInfo.TransferredDate.Should().Be(DateTime.Parse(TodaysDate));
            vacancy.TransferInfo.TransferredByUser.Should().Be(vacancyUser);
        }

        [Fact]
        public async Task GivenProviderOwnedVacancyToTransfer_WhenProviderIsBlocked_ThenSetTransferInfoOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.BlockedByQa);

            vacancy.TransferInfo.Should().NotBeNull();
            vacancy.TransferInfo.Reason.Should().Be(TransferReason.BlockedByQa);
            vacancy.TransferInfo.Ukprn.Should().Be(vacancy.TrainingProvider.Ukprn);
            vacancy.TransferInfo.ProviderName.Should().Be(vacancy.TrainingProvider.Name);
            vacancy.TransferInfo.LegalEntityName.Should().Be(vacancy.LegalEntityName);
            vacancy.TransferInfo.TransferredDate.Should().Be(DateTime.Parse(TodaysDate));
            vacancy.TransferInfo.TransferredByUser.Should().Be(vacancyUser);
        }

        [Fact]
        public async Task GivenProviderOwnedVacancyToTransfer_ThenSetEmployerOwnerTypeOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.OwnerType.Should().NotBe(OwnerType.Provider);
        }

        [Fact]
        public async Task GivenProviderOwnedVacancyToTransfer_ThenClearSubmittedByUserOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.SubmittedByUser.Should().BeNull();
        }

        [Fact]
        public async Task GivenProviderOwnedVacancyToTransfer_ThenClearProviderContactDetailsOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.ProviderContact.Should().BeNull();
            vacancy.EmployerContact.Should().BeNull();
        }

        [Fact]
        public async Task GivenLiveProviderOwnedVacancyToTransfer_ThenSetClosedStatusOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.Status.Should().Be(VacancyStatus.Closed);
        }

        [Fact]
        public async Task GivenLiveProviderOwnedVacancyToTransfer_ThenSetClosedReasonOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.ClosureReason.Should().Be(ClosureReason.TransferredByEmployer);
        }

        [Fact]
        public async Task GivenLiveProviderOwnedVacancyToTransfer_ThenSetClosedByUserOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Live);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.ClosedByUser.Should().Be(vacancyUser);
        }

        [Theory]
        [InlineData(ReviewStatus.New)]
        [InlineData(ReviewStatus.PendingReview)]
        [InlineData(ReviewStatus.Closed)]
        public async Task GivenSubmittedProviderOwnedVacancyToTransfer_AndReviewIsNotUnderReview_ThenSetDraftStatusOfVacancy(ReviewStatus reviewStatus)
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Submitted);
            var vacancyUser = _autoFixture.Create<VacancyUser>();
            _mockVacancyReviewQuery.Setup(x => x.GetLatestReviewByReferenceAsync(vacancy.VacancyReference.Value))
                                    .ReturnsAsync(new VacancyReview { Status = reviewStatus });

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.Status.Should().Be(VacancyStatus.Draft);
        }

        [Fact]
        public async Task GivenSubmittedProviderOwnedVacancyToTransfer_AndReviewIsUnderReview_ThenLeaveStatusOfVacancyAsSubmitted()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Submitted);
            var vacancyUser = _autoFixture.Create<VacancyUser>();
            _mockVacancyReviewQuery.Setup(x => x.GetLatestReviewByReferenceAsync(vacancy.VacancyReference.Value))
                                    .ReturnsAsync(new VacancyReview { Status = ReviewStatus.UnderReview });

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.Status.Should().Be(VacancyStatus.Submitted);
        }

        [Fact]
        public async Task GivenApprovedProviderOwnedVacancyToTransfer_ThenSetClosedStatusOfVacancy()
        {
            var vacancy = GetTestProviderOwnedVacancy(VacancyStatus.Approved);
            vacancy.ApprovedDate = DateTime.UtcNow;
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.Status.Should().Be(VacancyStatus.Closed);
            vacancy.ApprovedDate.Should().BeNull();
        }

        [Theory]
        [InlineData(VacancyStatus.Draft)]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Closed)]
        public async Task GivenNonLiveOrReviewableProviderOwnedVacancyToTransfer_ThenLeaveStatusOfVacancy(VacancyStatus statusArg)
        {
            var vacancy = GetTestProviderOwnedVacancy(statusArg);
            var vacancyUser = _autoFixture.Create<VacancyUser>();

            await _sut.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, TransferReason.EmployerRevokedPermission);

            vacancy.Status.Should().Be(statusArg);
        }

        private Vacancy GetTestProviderOwnedVacancy(VacancyStatus status)
        {
            return  _autoFixture
                    .Build<Vacancy>()
                    .Without(v => v.TransferInfo)
                    .Without(v => v.EmployerContact)
                    .With(v => v.OwnerType, OwnerType.Provider)
                    .With(v => v.Status, status)
                    .With(v => v.ProviderContact, _autoFixture.Create<ContactDetail>())
                    .Create();
        }
    }
}