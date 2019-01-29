using System;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer
{
    public class EmployersEditModel : VacancyRouteModel
    {
        public string SelectedEmployerId { get; set; }
    }
}