﻿using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Views;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class AboutEmployerEditModel : VacancyRouteModel
    {
        public string EmployerDescription { get; set; }
        public string EmployerWebsiteUrl { get; set; }
    }
}
