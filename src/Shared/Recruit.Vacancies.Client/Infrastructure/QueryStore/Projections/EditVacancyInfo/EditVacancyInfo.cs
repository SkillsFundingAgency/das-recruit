using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class EditVacancyInfo : QueryProjectionBase
    {
        public EditVacancyInfo()  : base(QueryViewType.EditVacancyInfo.TypeName)
        {
        }

        public IEnumerable<LegalEntity> LegalEntities { get; set; }
    }
}