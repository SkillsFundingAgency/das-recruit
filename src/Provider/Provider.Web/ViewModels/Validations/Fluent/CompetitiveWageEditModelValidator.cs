using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent
{
    public class CompetitiveWageEditModelValidator : AbstractValidator<CompetitiveWageEditModel>
    {
        public const string IsSalaryAboveNationalMinimumWageRequired = "Select whether the salary will be competitive or not.";
        public CompetitiveWageEditModelValidator()
        {
            RuleFor(x => x.IsSalaryAboveNationalMinimumWage)
                .NotNull()
                .WithMessage(IsSalaryAboveNationalMinimumWageRequired);
        }
    }
}
