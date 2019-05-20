using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;

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
        public bool HasCloneableVacancies { get; set; }
        public string BackLink =>
            HasCloneableVacancies ? RouteNames.Vacancies_Get : RouteNames.CreateVacancyOptions_Get;

        public bool ShowReturnToMALink { get; set; }
    }
}
