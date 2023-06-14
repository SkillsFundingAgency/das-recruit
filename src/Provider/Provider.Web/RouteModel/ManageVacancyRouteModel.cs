using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ManageVacancyRouteModel : VacancyRouteModel
    {
        public List<VacancyApplication> SharedApplications { get; set; }
    }
}
