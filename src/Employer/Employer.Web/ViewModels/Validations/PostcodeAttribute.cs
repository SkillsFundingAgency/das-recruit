namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using Esfa.Recruit.Vacancies.Client.Domain;
    using System.ComponentModel.DataAnnotations;

    public class PostcodeAttribute : RegularExpressionAttribute
    {
        public PostcodeAttribute() : base(ValidationRegexes.PostcodeRegex.ToString())
        {
        }
    }
}
