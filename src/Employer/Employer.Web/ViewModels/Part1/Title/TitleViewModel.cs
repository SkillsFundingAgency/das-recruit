using System;
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleViewModel
    {
        [FromRoute]
        public string EmployerAccountId { get; set; }
        [FromRoute]
        public Guid? VacancyId { get; set; }
        public string Title { get; set; }
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
        public bool ShowReturnToDashboardLink { get; set; }
        public string ReturnToMALinkText { get; set; }
        public string ReturnToMALink { get; set; }
        public string BackLinkText { get; set; }
        public string BackLinkRoute { get; set; }
        public bool ReferredFromMa { get; set; }
        public string ReferredUkprn { get; set; }
        public string ReferredProgrammeId { get; set; }
        public bool ReferredFromSavedFavourites => ReferredFromMa &
                                                   (!string.IsNullOrEmpty(ReferredUkprn) ||
                                                    !string.IsNullOrEmpty(ReferredProgrammeId));

        public string TrainingTitle { get; set; }
    }
}
