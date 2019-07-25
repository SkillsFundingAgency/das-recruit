using FluentValidation;

namespace Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy
{
    public class AcknowledgeEditModelValidator : AbstractValidator<AcknowledgeEditModel>
    {
        public AcknowledgeEditModelValidator()
        {
            RuleFor(x => x.Acknowledged)
                .Equal(true)
                .WithMessage("Please tick the box to continue");
        }
    }
}
