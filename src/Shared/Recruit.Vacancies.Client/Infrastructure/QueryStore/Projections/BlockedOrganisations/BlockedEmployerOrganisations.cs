using System.Collections.Generic;
using Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations
{
    public class BlockedEmployerOrganisations : QueryProjectionBase
    {
        public BlockedEmployerOrganisations() : base(QueryViewType.BlockedEmployerOrganisations.TypeName)
        {}

        public IEnumerable<BlockedOrganisationSummary> Data { get; set; }
    }
}