using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class WageEditModelValidator : AbstractValidator<WageEditModel>
    {
        public const string WageTypeRequired = "Select how much the apprentice will be paid";
        public WageEditModelValidator()
        {
            RuleFor(x => x.WageType)
                .NotNull()
                .WithMessage(WageTypeRequired);
        }
    }
}
