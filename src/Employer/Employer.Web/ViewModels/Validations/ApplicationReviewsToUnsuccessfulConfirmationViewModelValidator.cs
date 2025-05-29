using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class ApplicationReviewsToUnsuccessfulConfirmationViewModelValidator : AbstractValidator<ApplicationReviewsToUnsuccessfulConfirmationViewModel>
    {
        public ApplicationReviewsToUnsuccessfulConfirmationViewModelValidator()
        {
            RuleFor(x => x.ApplicationsUnsuccessfulConfirmed)
                .NotNull()
                .WithMessage(ApplicationReviewValidator.ApplicationsToUnsuccessfulConfirmationRequired);
        }
    }
}
