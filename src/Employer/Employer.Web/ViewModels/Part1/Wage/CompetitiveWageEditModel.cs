﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage
{
    public class CompetitiveWageEditModel : WageEditModel
    {
        public bool? IsSalaryAboveNationalMinimumWage { get; set; }
    }
}
