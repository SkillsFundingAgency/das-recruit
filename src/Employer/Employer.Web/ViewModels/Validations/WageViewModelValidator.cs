using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class WageViewModelValidator : AbstractValidator<WageEditModel>
    {
        public const string WageTypeRequired = "Select how much the apprentice will be paid";
        public WageViewModelValidator()
        {
            RuleFor(x => x.WageType)
                .NotNull()
                .WithMessage(WageTypeRequired);
        }
    }
}
