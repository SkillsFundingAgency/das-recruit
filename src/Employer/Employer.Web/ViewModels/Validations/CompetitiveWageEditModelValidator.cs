using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class CompetitiveWageEditModelValidator : AbstractValidator<CompetitiveWageEditModel>
    {
        public const string CompetitiveWageTypeRequired = "Select how much the apprentice will be paid";
        public CompetitiveWageEditModelValidator()
        {
            RuleFor(x => x.CompetitiveSalaryType)
                .NotNull()
                .WithMessage(CompetitiveWageTypeRequired);
        }
    }
}
