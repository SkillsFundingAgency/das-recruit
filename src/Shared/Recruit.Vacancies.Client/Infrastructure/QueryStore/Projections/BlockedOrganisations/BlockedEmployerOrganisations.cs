using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations
{
    public class BlockedEmployerOrganisations : QueryProjectionBase
    {
        public BlockedEmployerOrganisations() : base(QueryViewType.BlockedEmployerOrganisations.TypeName)
        {}

        public IEnumerable<string> Data { get; set; }
    }
}