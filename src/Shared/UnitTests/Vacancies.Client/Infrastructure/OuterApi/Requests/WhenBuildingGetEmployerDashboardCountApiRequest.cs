using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

[TestFixture]
public class WhenBuildingGetEmployerDashboardCountApiRequest
{
    [Test, AutoData]
    public void Then_It_Is_Correctly_Constructed(
        long accountId)
    {
        var actual = new GetEmployerDashboardCountApiRequest(accountId);

        actual.GetUrl.Should().Be($"employerAccounts/{accountId}/dashboard");
    }
}