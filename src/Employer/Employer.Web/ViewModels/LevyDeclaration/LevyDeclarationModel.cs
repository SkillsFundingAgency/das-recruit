using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration
{
    public class LevyDeclarationModel
    {
        [Required(ErrorMessage = ValidationMessages.LevyDeclarationConfirmationMessages.SelectionRequired)]
        public bool? ConfirmAsLevyPayer { get; set; }
    }
}