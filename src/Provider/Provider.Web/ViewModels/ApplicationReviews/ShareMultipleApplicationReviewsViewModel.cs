using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsViewModel : ShareMultipleApplicationsPostRequest
    {
        public Guid VacancyId { get; set; }
        public long VacancyReference { get; set; }
        public long Ukprn { get; set; }
        public bool SelectedAllApplications { get; set; }
        public List<VacancyApplication> VacancyApplications { get; set; }
    }
}
