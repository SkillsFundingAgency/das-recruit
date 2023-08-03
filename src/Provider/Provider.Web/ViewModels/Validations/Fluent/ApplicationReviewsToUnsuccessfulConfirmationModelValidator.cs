using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent
{
    public class ApplicationReviewsToUnsuccessfulConfirmationModelValidator : AbstractValidator<ApplicationReviewsToUnsuccessfulConfirmationViewModel>
    {
        public ApplicationReviewsToUnsuccessfulConfirmationModelValidator(IProfanityListProvider profanityListProvider)
        {
            RuleFor(x => x.ApplicationsToUnsuccessfulConfirmed)
                .NotNull()
                .WithMessage(ApplicationReviewValidator.ApplicationsToUnsuccessfulConfirmationRequired);
        }
    }
}