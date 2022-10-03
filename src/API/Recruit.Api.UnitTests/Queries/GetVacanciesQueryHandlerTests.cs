using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
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
        private const long EmployerVacancyCount = 45;
        private const long ProviderVacancyCount = 55;
        private readonly GetVacanciesQueryHandler _sut;
        private readonly Mock<IEmployerVacancyClient> _employerVacancyClient;
        private readonly Mock<IProviderVacancyClient> _providerVacancyClient;

        public GetVacanciesQueryHandlerTests()
        {
            var mockVacancySummaryMapper = new Mock<IVacancySummaryMapper>();
        
            _employerVacancyClient = new Mock<IEmployerVacancyClient>();
            _providerVacancyClient = new Mock<IProviderVacancyClient>();
            
            var vacancySummaryFixture = new Fixture();
            var vacancySummariesEmployer = vacancySummaryFixture.Build<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary>()
                                .CreateMany(6).ToList();
            
            var vacancySummariesProvider = vacancySummaryFixture.Build<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary>()
                .CreateMany(8).ToList();
            
            _employerVacancyClient.Setup(
                x => x.GetDashboardAsync(ValidEmployerAccountId, 1, FilteringOptions.All, null)).ReturnsAsync(new Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer.EmployerDashboard
            {
                Vacancies = vacancySummariesEmployer
            });
            _employerVacancyClient.Setup(x => x.GetVacancyCount(ValidEmployerAccountId, VacancyType.Apprenticeship, FilteringOptions.All, null))
                .ReturnsAsync(EmployerVacancyCount);

            _providerVacancyClient.Setup(
                x => x.GetDashboardAsync(ValidUkprn, VacancyType.Apprenticeship, 1, FilteringOptions.All, null)).ReturnsAsync(
                new Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider.ProviderDashboard
                {
                    Vacancies = vacancySummariesProvider
                });
            _providerVacancyClient.Setup(x => x.GetVacancyCount(ValidUkprn, VacancyType.Apprenticeship, FilteringOptions.All, null))
                .ReturnsAsync(ProviderVacancyCount);

            _sut = new GetVacanciesQueryHandler(Mock.Of<ILogger<GetVacanciesQueryHandler>>(), mockVacancySummaryMapper.Object, _providerVacancyClient.Object, _employerVacancyClient.Object);
        }

        [Fact]
        public async Task GivenRequestWithInvalidEmployerAccountId_ShouldSetInvalidValidationError()
        {
            var query = new GetVacanciesQuery("XX1", null, 25, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }

        [Fact]
        public async Task GivenRequestWithInvalidUkprn_ShouldSetInvalidValidationError()
        {
            const long NineDigitInvalidUkprn = 111111111;
            var query = new GetVacanciesQuery(ValidEmployerAccountId, NineDigitInvalidUkprn, 25, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            result.ResultCode.Should().Be(ResponseCode.InvalidRequest);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdOnly_ShouldCallToRetrieveEmployerDashboard()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, 25, 1);
            
            await _sut.Handle(query, CancellationToken.None);
            
            _employerVacancyClient.Verify(qsr => qsr.GetDashboardAsync(ValidEmployerAccountId, 1, FilteringOptions.All, null), Times.Once);
            _providerVacancyClient.Verify(
                qsr => qsr.GetDashboardAsync(It.IsAny<long>(), It.IsAny<VacancyType>(), It.IsAny<int>(),
                    It.IsAny<FilteringOptions>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GivenRequestWithUnmatchedEmployerAccountIdOnly_ShouldReturnNotFoundResponse()
        {
            var query = new GetVacanciesQuery(UnmatchedEmployerAccountId, null, 25, 1);
            
            var result = await _sut.Handle(query, CancellationToken.None);
            
            result.ResultCode.Should().Be(ResponseCode.NotFound);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdOnly_ShouldReturnVacanciesSummaryWithFourteenVacancies()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, 25, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            var dataResult = result.Data.As<VacanciesSummary>();
            dataResult.Vacancies.Count().Should().Be(6);
            dataResult.TotalResults.Should().Be(Convert.ToInt32(EmployerVacancyCount));
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndPageSizeTen_ShouldReturnVacanciesSummaryWithTenVacancies()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, null, 25, 1);
            var result = await _sut.Handle(query, CancellationToken.None);
            
            var dataResult = result.Data.As<VacanciesSummary>();

            dataResult.Vacancies.Count().Should().Be(6);
            dataResult.TotalResults.Should().Be(Convert.ToInt32(EmployerVacancyCount));
            dataResult.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GivenRequestWithEmployerAccountIdAndUkprn_ShouldCallToRetrieveProviderDashboard()
        {
            var query = new GetVacanciesQuery(ValidEmployerAccountId, ValidUkprn, 25, 1);
            
            await _sut.Handle(query, CancellationToken.None);
            
            _employerVacancyClient.Verify(qsr => qsr.GetDashboardAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FilteringOptions>(), It.IsAny<string>()), Times.Never);
            _providerVacancyClient.Verify(qsr => qsr.GetDashboardAsync(ValidUkprn, VacancyType.Apprenticeship, 1, FilteringOptions.All, null), Times.Once);
        }
    }
}