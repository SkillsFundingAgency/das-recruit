using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public class ShareApplicationReviewsPostRequestValidator : AbstractValidator<ShareApplicationReviewsPostRequest>
    {
        public ShareApplicationReviewsPostRequestValidator()
        {
            RuleFor(x => x.ShareApplicationsConfirmed)
                .NotNull()
                .WithMessage(ApplicationReviewValidator.ShareConfirmationRequired);
        }
    }
}
