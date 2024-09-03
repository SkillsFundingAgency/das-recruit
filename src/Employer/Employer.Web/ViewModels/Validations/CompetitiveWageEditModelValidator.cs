using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class CompetitiveWageEditModelValidator : AbstractValidator<CompetitiveWageEditModel>
    {
        public const string IsSalaryAboveNationalMinimumWageRequired = "Select if the salary is above National Minimum Wage";
        public CompetitiveWageEditModelValidator()
        {
            RuleFor(x => x.IsSalaryAboveNationalMinimumWage)
                .NotNull()
                .WithMessage(IsSalaryAboveNationalMinimumWageRequired);
        }
    }
}
