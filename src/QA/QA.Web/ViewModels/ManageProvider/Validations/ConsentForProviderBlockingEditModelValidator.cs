using FluentValidation;

namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider.Validations
{
    public class ConsentForProviderBlockingEditModelValidator : AbstractValidator<ConsentForProviderBlockingEditModel>
    {
        public ConsentForProviderBlockingEditModelValidator()
        {
            RuleFor(m => m.HasConsent)
                .Equal(true)
                .WithMessage("Please tick the box to continue");
            RuleFor(m => m.Reason)
                .NotEmpty()
                .WithMessage("Please enter a reason");
        }
    }
}