using System.Collections.Generic;
using AutoFixture.NUnit3;
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
            List<long> vacancyReferences,
            List<ApplicationReviewStats> response,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            [Greedy] EmployerAccountProvider employerAccountProvider)
        {
            encodingService.Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId))
                .Returns(accountId);

            var expectedGetUrl = new GetEmployerApplicationReviewsCountApiRequest(accountId, vacancyReferences);
            outerApiClient.Setup(x => x.Post<List<ApplicationReviewStats>>(
                    It.Is<GetEmployerApplicationReviewsCountApiRequest>(r => r.PostUrl == expectedGetUrl.PostUrl)))
                .ReturnsAsync(response);

            var result = await employerAccountProvider.GetEmployerDashboardApplicationReviewStats(hashedAccountId, vacancyReferences);


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
    }
}
