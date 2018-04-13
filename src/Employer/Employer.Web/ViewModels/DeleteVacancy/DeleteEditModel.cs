using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy
{
    public class DeleteEditModel
    {
        [Required]
        [FromRoute]
        public Guid VacancyId { get; set; }

        [Required(ErrorMessage = ValidationMessages.DeleteVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmDeletion { get; set; }
    }
}
