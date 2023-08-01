using System.Linq;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent
{
    public class ApplicationReviewsToUnsuccessfulModelValidator : AbstractValidator<ApplicationReviewsToUnsuccessfulRequest>
    {
        public ApplicationReviewsToUnsuccessfulModelValidator()
        {
            RuleFor(x => x.ApplicationsToUnsuccessful)
                .Must(apps => apps != null && apps.Any())
                .WithMessage(ApplicationReviewValidator.ApplicationReviewsToUnsuccessful);
        }
    }
}