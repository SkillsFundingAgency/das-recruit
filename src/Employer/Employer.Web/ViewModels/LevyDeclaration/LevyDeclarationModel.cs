using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration
{
    public class LevyDeclarationModel
    {
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [Required(ErrorMessage = ValidationMessages.LevyDeclarationConfirmationMessages.SelectionRequired)]
        public bool? ConfirmAsLevyPayer { get; set; }
    }
}