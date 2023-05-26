using Microsoft.AspNetCore.Mvc;
using System;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsViewModel
    {
        // TODO
        [FromRoute]
        public Guid? VacancyId { get; set; }
    }
}
