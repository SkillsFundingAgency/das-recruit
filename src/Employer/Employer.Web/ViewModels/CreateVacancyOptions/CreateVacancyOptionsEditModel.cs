using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancyOptions
{
    public class CreateVacancyOptionsEditModel
    {
        [Required(ErrorMessage = ValidationMessages.CreateVacancyOptionsConfirmationMessages.SelectionRequired)]
        public Guid? VacancyId { get; set; }
    }
}
