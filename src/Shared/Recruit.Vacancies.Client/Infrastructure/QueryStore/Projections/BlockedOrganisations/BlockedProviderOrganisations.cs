using System.Collections.Generic;
using Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations
{
    public class BlockedProviderOrganisations : QueryProjectionBase
    {
        public BlockedProviderOrganisations() : base(QueryViewType.BlockedProviderOrganisations.TypeName)
        {}

        public IEnumerable<BlockedOrganisationSummary> Data { get; set; }
    }
}