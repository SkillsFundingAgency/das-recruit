using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.QueryStore
{
    public class WhenUpdatingProviderDashboard
    {
        [Test, MoqAutoData]
        public async Task Then_If_Apprenticeship_Then_Updates_Provider_Apprentice_Dashboard(
            long ukprn,
            ProviderDashboard providerDashboard,
            List<VacancySummary> vacancySummaries,
            List<ProviderDashboardTransferredVacancy> providerDashboardTransferredVacancies,
            [Frozen] Mock<IQueryStore> queryStore,
            QueryStoreClient client)
        {
            await client.UpdateProviderDashboardAsync(ukprn, vacancySummaries, providerDashboardTransferredVacancies);

            queryStore
                .Verify(x =>
                    x.UpsertAsync(It.Is<ProviderDashboard>(c =>
                        c.Id.Equals(QueryViewType.ProviderDashboard.GetIdValue(ukprn))
                        && c.Vacancies.Equals(vacancySummaries)
                        && c.TransferredVacancies.Equals(providerDashboardTransferredVacancies)
                    )), Times.Once);
        }
        
    }
}