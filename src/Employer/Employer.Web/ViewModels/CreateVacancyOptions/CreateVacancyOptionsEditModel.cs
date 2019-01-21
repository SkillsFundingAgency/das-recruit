using System;
using System.ComponentModel.DataAnnotations;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancyOptions
{
    public class CreateVacancyOptionsEditModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.CreateVacancyOptionsConfirmationMessages.SelectionRequired)]
        public Guid? VacancyId { get; set; }
    }
}
