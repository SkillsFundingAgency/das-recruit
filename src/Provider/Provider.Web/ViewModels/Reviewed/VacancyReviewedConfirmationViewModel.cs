﻿using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Reviewed
{
    public class VacancyReviewedConfirmationViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }

        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
        public string EmployerName { get; set; }
        public bool IsResubmit { get; set; }
        public bool IsVacancyRejectedByEmployerNotificationSelected { get; set; }
    }
}
