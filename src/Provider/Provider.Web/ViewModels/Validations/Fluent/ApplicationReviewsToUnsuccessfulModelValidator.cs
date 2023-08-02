using System.Linq;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent
{
    public class ApplicationReviewsToUnsuccessfulModelValidator : AbstractValidator<ApplicationReviewsToUnSuccessfulRequest>
    {
        public ApplicationReviewsToUnsuccessfulModelValidator()
        {
            RuleFor(x => x.ApplicationsToUnSuccessful)
                .Must(apps => apps != null && apps.Any())
                .WithMessage(ApplicationReviewValidator.ApplicationReviewsToUnsuccessful);
        }
    }
}