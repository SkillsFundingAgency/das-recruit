using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent
{
    public class ShareApplicationReviewsPostRequestValidator : AbstractValidator<IApplicationReviewsShareModel>
    {
        public ShareApplicationReviewsPostRequestValidator()
        {
            RuleFor(x => x.ShareApplicationsConfirmed)
                .NotNull()
                .WithMessage(ApplicationReviewValidator.ShareConfirmationRequired);
        }
    }
}
