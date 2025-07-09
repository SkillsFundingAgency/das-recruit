using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using NUnit.Framework;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    [TestFixture]
    public class EmployerAccountProviderTests
    {
        [Test, MoqAutoData]
        public async Task GetEmployerDashboardApplicationReviewStats_Should_Return_As_Expected(
            string hashedAccountId,
            long accountId,
            string applicationSharedFilteringStatus,
            List<long> vacancyReferences,
            GetApplicationReviewStatsResponse response,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] EmployerAccountProvider employerAccountProvider)
        {
            encodingService.Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId))
                .Returns(accountId);

            var expectedGetUrl = new GetEmployerApplicationReviewsCountApiRequest(accountId, vacancyReferences, applicationSharedFilteringStatus);
            outerApiClient.Setup(x => x.Post<GetApplicationReviewStatsResponse>(
                    It.Is<GetEmployerApplicationReviewsCountApiRequest>(r => r.PostUrl == expectedGetUrl.PostUrl)))
                .ReturnsAsync(response);

            var result = await employerAccountProvider.GetEmployerDashboardApplicationReviewStats(hashedAccountId, vacancyReferences, applicationSharedFilteringStatus);


            result.Should().BeEquivalentTo(response);
        }

        [Test, MoqAutoData]
        public async Task GetEmployerDashboardStats_Should_Return_As_Expected(
            string hashedAccountId,
            long accountId,
            GetDashboardCountApiResponse response,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] EmployerAccountProvider employerAccountProvider)
        {
            encodingService.Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId))
                .Returns(accountId);

            var expectedGetUrl = new GetEmployerDashboardCountApiRequest(accountId);
            outerApiClient.Setup(x => x.Get<GetDashboardCountApiResponse>(
                    It.Is<GetEmployerDashboardCountApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
                .ReturnsAsync(response);

            var result = await employerAccountProvider.GetEmployerDashboardStats(hashedAccountId);


            result.Should().BeEquivalentTo(response);
        }

        [Test, MoqAutoData]
        public async Task GetEmployerVacancyDashboardStats_Should_Return_As_Expected(
            string hashedAccountId,
            long accountId,
            int pageNumber, 
            List<ApplicationReviewStatus> statuses,
            GetVacanciesDashboardResponse response,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] EmployerAccountProvider employerAccountProvider)
        {
            encodingService.Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId))
                .Returns(accountId);

            var expectedGetUrl = new GetEmployerDashboardVacanciesApiRequest(accountId, pageNumber, statuses);
            outerApiClient.Setup(x => x.Get<GetVacanciesDashboardResponse>(
                    It.Is<GetEmployerDashboardVacanciesApiRequest>(r => r.GetUrl == expectedGetUrl.GetUrl)))
                .ReturnsAsync(response);

            var result = await employerAccountProvider.GetEmployerVacancyDashboardStats(hashedAccountId, pageNumber, statuses);


            result.Should().BeEquivalentTo(response);
        }
        
        [Test, MoqAutoData]
        public async Task GetAllLegalEntitiesConnectedToAccountAsync_Should_Return_As_Expected(
            long accountId,
            List<string> hashedAccountIds,
            string searchTerm,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending,
            GetAllAccountLegalEntitiesApiResponse response,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] EmployerAccountProvider employerAccountProvider)
        {
            var accountIds = new List<long>();
            foreach (var hashedAccountId in hashedAccountIds)
            {
                accountIds.Add(accountId);
                encodingService.Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId))
                    .Returns(accountId);
            }

            var expectedGetUrl = new GetAllAccountLegalEntitiesApiRequest(new GetAllAccountLegalEntitiesApiRequest.GetAllAccountLegalEntitiesApiRequestData
            {
                AccountIds = accountIds,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                IsAscending = isAscending
            });
            outerApiClient.Setup(x => x.Post<GetAllAccountLegalEntitiesApiResponse>(
                    It.Is<GetAllAccountLegalEntitiesApiRequest>(r => r.PostUrl == expectedGetUrl.PostUrl)))
                .ReturnsAsync(response);
            var result = await employerAccountProvider.GetAllLegalEntitiesConnectedToAccountAsync(hashedAccountIds,
                searchTerm, pageNumber, pageSize, sortColumn, isAscending);
            
            result.Should().BeEquivalentTo(response);
        }
    }
}
