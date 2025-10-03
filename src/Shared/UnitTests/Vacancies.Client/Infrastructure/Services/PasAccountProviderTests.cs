using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class PasAccountProviderTests
    {
        [Test, MoqAutoData]
        public async Task HasAgreementAsync_EsfaTestTrainingProviderShouldHaveAgreement(
            [Frozen] Mock<IOuterApiClient> outerApiClient,
            OuterApiGetProviderStatusClient outerApiGetProviderStatusClient)
        {
            var result = await outerApiGetProviderStatusClient.GetProviderStatus(EsfaTestTrainingProvider.Ukprn);
            
            result.CanAccessService.Should().BeTrue();
            outerApiClient.Verify(x=>x.Get<ProviderAccountResponse>(It.IsAny<GetProviderStatusDetails>()), Times.Never);
        }
    }
}
