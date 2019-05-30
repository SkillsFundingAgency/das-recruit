using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleViewModel : TitleEditModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Title)
        };
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.Title_Post : RouteNames.CreateVacancy_Post;
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public List<VacancySummary> Vacancies { get; set; } = new List<VacancySummary>();
        public string BackLink =>
            Vacancies.Any() ? RouteNames.CreateVacancyOptions_Get : RouteNames.Dashboard_Index_Get;
    }
}
