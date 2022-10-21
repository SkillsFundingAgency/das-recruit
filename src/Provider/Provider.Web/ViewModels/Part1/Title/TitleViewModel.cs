using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Title
{
    public class TitleViewModel : VacancyRouteModel
    {
        public string EmployerAccountId { get; set; }
        public string Title { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Title)
        };
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.Title_Post : RouteNames.CreateVacancy_Post;
        public bool HasAnyVacancies { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public string BackLink =>
            HasAnyVacancies ? RouteNames.Vacancies_Get : RouteNames.CreateVacancy_Get;
    }
}