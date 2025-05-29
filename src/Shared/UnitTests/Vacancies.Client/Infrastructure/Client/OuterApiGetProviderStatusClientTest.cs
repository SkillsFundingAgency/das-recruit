using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client
{
    public class OuterApiGetProviderStatusClientTest
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Made_And_ProviderResponse_Returned(
            long ukprn,
            ProviderAccountResponse apiResponse,
            [Frozen] Mock<IOuterApiClient> apiClient,
            OuterApiGetProviderStatusClient service)
        {
            //Arrange
            var request = new GetProviderStatusDetails(ukprn);
            apiClient.Setup(x =>
                    x.Get<ProviderAccountResponse>(
                        It.Is<GetProviderStatusDetails>(c => c.GetUrl.Equals(request.GetUrl))))
                .ReturnsAsync(apiResponse);

            //Act
            var actual = await service.GetProviderStatus(ukprn);

            //Assert
            actual.CanAccessService.Should().Be(apiResponse.CanAccessService);
        }
    }
}
