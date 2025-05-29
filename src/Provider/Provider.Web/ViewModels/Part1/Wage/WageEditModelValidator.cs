using FluentValidation;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage
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

