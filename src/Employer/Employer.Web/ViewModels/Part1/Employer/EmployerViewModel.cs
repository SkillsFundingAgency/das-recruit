using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerViewModel : VacancyRouteModel
    {
        public IEnumerable<OrganisationViewModel> Organisations { get; set; }

        public bool HasOnlyOneOrganisation => Organisations.Count() == 1;
        
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public PartOnePageInfoViewModel PageInfo { get; set; }

        public long? SelectedOrganisationId { get; set; }
    }

    public class OrganisationViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
