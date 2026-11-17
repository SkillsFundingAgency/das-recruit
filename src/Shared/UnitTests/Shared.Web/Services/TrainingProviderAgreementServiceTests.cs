using AutoFixture.NUnit4;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services;

public class TrainingProviderAgreementServiceTests
{
    private const long Ukprn = 99999999;

    [Test, MoqAutoData]
    public async Task Then_Checks_If_Provider_Can_Access_Service_And_Returns_Response(
        TrainingProviderAgreementService sut,
        [Frozen] Mock<IGetProviderStatusClient> pasAccountProviderMock)
    {
        pasAccountProviderMock.Setup(c => c.GetProviderStatus(Ukprn)).ReturnsAsync(new ProviderAccountResponse{CanAccessService =  true});
        
        var result = await sut.HasAgreementAsync(Ukprn);

        result.Should().BeTrue();
    }
}
