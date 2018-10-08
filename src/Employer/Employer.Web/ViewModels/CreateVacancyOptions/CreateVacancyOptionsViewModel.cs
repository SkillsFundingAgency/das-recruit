using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancyOptions
{
    public class CreateVacancyOptionsViewModel 
    {
        [Required(ErrorMessage = ValidationMessages.CreateVacancyOptionsConfirmationMessages.SelectionRequired)]
        public Guid? VacancyId { get; set; }

        public IEnumerable<ClonableVacancy> Vacancies { get; set; }
    }

    public class ClonableVacancy
    {
        public Guid Id { get; set; }
        public long VacancyReference { get; set; }
        public string Summary { get; set; }
    }
}
