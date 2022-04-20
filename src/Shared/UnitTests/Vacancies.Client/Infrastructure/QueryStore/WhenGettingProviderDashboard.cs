using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.QueryStore
{
    public class WhenGettingProviderDashboard
    {
        [Test, MoqAutoData]
        public async Task Then_If_Apprenticeship_Then_Gets_Provider_Apprentice_Dashboard(
            long ukprn,
            ProviderDashboard providerDashboard,
            [Frozen] Mock<IQueryStore> queryStore)
        {
            var client = new QueryStoreClient(queryStore.Object, Mock.Of<ITimeProvider>(),
                new ServiceParameters("Apprenticeship"));
            queryStore
                .Setup(x => x.GetAsync<ProviderDashboard>(
                    QueryViewType.ProviderDashboard.TypeName, 
                    QueryViewType.ProviderDashboard.GetIdValue(ukprn))).ReturnsAsync(providerDashboard);

            var dashboard = await client.GetProviderDashboardAsync(ukprn);
            
            dashboard.Should().BeEquivalentTo(providerDashboard);
        }
        [Test, MoqAutoData]
        public async Task Then_If_Traineeship_Then_Gets_Provider_Trainee_Dashboard(
            long ukprn,
            ProviderDashboard providerDashboard,
            [Frozen] Mock<IQueryStore> queryStore)
        {
            var client = new QueryStoreClient(queryStore.Object, Mock.Of<ITimeProvider>(),
                new ServiceParameters("Traineeship"));
            queryStore
                .Setup(x => x.GetAsync<ProviderDashboard>(
                    QueryViewType.ProviderTraineeshipDashboard.TypeName, 
                    QueryViewType.ProviderTraineeshipDashboard.GetIdValue(ukprn))).ReturnsAsync(providerDashboard);

            var dashboard = await client.GetProviderDashboardAsync(ukprn);
            
            dashboard.Should().BeEquivalentTo(providerDashboard);
        }
    }
}