﻿using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingFirstVacancyViewModel : VacancyRouteModel
    {
        public string VacancyTitle { get; init; }
        [Required(ErrorMessage = ValidationMessages.TrainingFirstVacancyValidationMessages.HasFoundTraining)]
        public bool? HasFoundTraining { get; set; }
    }
}
