using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations
{
    public class BlockedProviderOrganisations : QueryProjectionBase
    {
        public BlockedProviderOrganisations() : base(QueryViewType.BlockedProviderOrganisations.TypeName)
        {}

        public IEnumerable<long> Data { get; set; }
    }
}