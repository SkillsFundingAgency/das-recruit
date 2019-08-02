using FluentValidation;

namespace Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy
{
    public class ConsentEditModelValidator : AbstractValidator<ConsentEditModel>
    {
        public ConsentEditModelValidator()
        {
            RuleFor(x => x.Acknowledged)
                .Equal(true)
                .WithMessage("Please tick the box to continue");
        }
    }
}
