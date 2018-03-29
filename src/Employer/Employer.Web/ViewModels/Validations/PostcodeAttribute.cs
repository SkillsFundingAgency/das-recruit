namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using Esfa.Recruit.Vacancies.Client.Application.Validation;
    using Esfa.Recruit.Vacancies.Client.Domain;
    using System.ComponentModel.DataAnnotations;

    public class PostcodeAttribute : RegularExpressionAttribute
    {
        public PostcodeAttribute() : base(ValidationConstants.PostcodeRegex.ToString())
        {
        }
    }
}
