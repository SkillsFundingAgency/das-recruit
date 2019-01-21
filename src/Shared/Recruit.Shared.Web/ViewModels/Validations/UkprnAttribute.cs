namespace Esfa.Recruit.Shared.Web.ViewModels.Validations
{
    using Esfa.Recruit.Vacancies.Client.Application.Validation;
    using System.ComponentModel.DataAnnotations;

    public class UkprnAttribute : RegularExpressionAttribute
    {
        public UkprnAttribute() : base(ValidationConstants.UkprnRegex.ToString())
        {
        }
    }
}
