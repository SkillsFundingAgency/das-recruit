using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Recruit.Api.Services;
using Xunit;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries
{
    public class GetVacanciesQueryHandlerTests
    {
        private const string ValidEmployerAccountId = "MYJR4X";
        private const string UnmatchedEmployerAccountId = "MYJR55";
        private const long ValidUkprn = 10000020;
        private const long UnmatchedUkprn = 11110000;
        private const int UnmatchedLegalEntityId = 4007;
        private readonly Mock<IVacancySummaryMapper> _mockVacancySummaryMapper;
        private readonly Mock<IQueryStoreReader> _mockQueryStoreReader;
        private readonly GetVacanciesQueryHandler _sut;

        public GetVacanciesQueryHandlerTests()
        {
            _mockVacancySummaryMapper = new Mock<IVacancySummaryMapper>();
            _mockQueryStoreReader = new Mock<IQueryStoreReader>();
            var vacancySummaryFixture = new Fixture();
            var vacancySummarySetOne = vacancySummaryFixture.Build<VacancySummaryProjection>()
                                .With(vs => vs.LegalEntityId, 130)
                                .CreateMany(6);
            var vacancySummarySetTwo = vacancySummaryFixture.Build<VacancySummaryProjection>()
                                .With(vs => vs.LegalEntityId, 140)
                                .With(vs => vs.Ukprn, ValidUkprn)
                                .CreateMany(8);

            _mockQueryStoreReader.Setup(qsr => qsr.GetEmployerDashboardAsync(ValidEmployerAccountId))
                                .ReturnsAsync(new EmployerDashboard
                                {
                                    Id = $"{nameof(EmployerDashboard)}_{ValidEmployerAccountId}",
                                    Vacancies = vacancySummarySetOne.Concat(vacancySummarySetTwo)
                                });

            _mockQueryStoreReader.Setup(qsr => qsr.GetProviderDashboardAsync(ValidUkprn))
                                .ReturnsAsync(new ProviderDashboard
                                {
                                    Id = $"{nameof(ProviderDashboard)}_{ValidEmployerAccountId}",
                                    Vacancies = vacancySummarySetOne.Concat(vacancySummarySetTwo)
                                });


            _sut = new GetVacanciesQueryHandler(Mock.Of<ILogger<GetVacanciesQueryHandler>>(), _mockQueryStoreReader.Object, _mockVacancySummaryMapper.Object);
        }

        [Fact]
        public async Task GivenRequestWithInvalidEmployerAccountId_ShouldSetInvalidValidationError()
        {
            var query = new GetVacanciesQuery("XX1", null, null, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }

        [Fact]
        public async Task GivenRequestWithInvalidUkprn_ShouldSetInvalidValidationError()
        {
            const long NineDigitInvalidUkprn = 111111111;
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, NineDigitInvalidUkprn, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdOnly_ShouldCallToRetrieveEmployerDashboard()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, null, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            _mockQueryStoreReader.Verify(qsr => qsr.GetEmployerDashboardAsync(ValidEmployerAccountId), Times.Once);
            _mockQueryStoreReader.Verify(qsr => qsr.GetProviderDashboardAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task GivenRequestWithUnmatchedEmployerAccountIdOnly_ShouldReturnNotFoundResponse()
        {
            var query = new GetVacanciesQuery(UnmatchedEmployerAccountId, null, null, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.NotFound);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndUnmatchedUkprn_ShouldReturnNotFoundResponse()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, UnmatchedUkprn, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.NotFound);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndUnmatchedLegalEntityId_ShouldReturnNotFoundResponse()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, UnmatchedLegalEntityId, null, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.NotFound);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdOnly_ShouldReturnVacanciesSummaryWithFourteenVacancies()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, null, 100, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            var dataResult = result.Data.As<VacanciesSummary>();
            dataResult.Vacancies.Count().Should().Be(14);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndPageSizeTen_ShouldReturnVacanciesSummaryWithTenVacancies()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, null, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            var dataResult = result.Data.As<VacanciesSummary>();
            dataResult.Vacancies.Count().Should().Be(10);
            dataResult.TotalResults.Should().Be(14);
            dataResult.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndLegalEntityId_ShouldCallToRetrieveEmployerDashboard()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, 130, null, 100, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            var dataResult = result.Data.As<VacanciesSummary>();
            dataResult.Vacancies.Count().Should().Be(6);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndUkprn_ShouldCallToRetrieveProviderDashboard()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, ValidUkprn, 10, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            _mockQueryStoreReader.Verify(qsr => qsr.GetEmployerDashboardAsync(ValidEmployerAccountId), Times.Never);
            _mockQueryStoreReader.Verify(qsr => qsr.GetProviderDashboardAsync(ValidUkprn), Times.Once);
        }
    }
}