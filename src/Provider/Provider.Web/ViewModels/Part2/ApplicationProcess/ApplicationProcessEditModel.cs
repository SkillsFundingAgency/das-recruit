using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess
{
    public class ApplicationProcessEditModel : VacancyRouteModel
    {
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ApplicationInstructions { get; set; }
        public string ApplicationUrl { get; set; }
    }
}
