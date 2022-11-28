using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services.EmployerAccount;

public class WhenGettingEmployerIdentifiers
{
    [Test, MoqAutoData]
    public async Task Then_The_Request_Is_MAde_And_Data_returned(
        string email,
        string userId,
        GetUserAccountsResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        EmployerAccountProvider provider)
    {
        var request = new GetUserAccountsRequest(userId, email);
        outerApiClient
            .Setup(x => x.Get<GetUserAccountsResponse>(
                It.Is<GetUserAccountsRequest>(c => c.GetUrl.Equals(request.GetUrl)))).ReturnsAsync(response);

        var actual = await provider.GetEmployerIdentifiersAsync(userId, email);

        actual.Should().BeEquivalentTo(response);
    }
}