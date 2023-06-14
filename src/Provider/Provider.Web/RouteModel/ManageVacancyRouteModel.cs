using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ManageVacancyRouteModel : VacancyRouteModel
    {
        public bool SharedApplicationsBanner { get; set; }
    }
}
