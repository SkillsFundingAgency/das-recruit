using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class ApplicationReviewsToUnsuccessfulRouteModelValidator : AbstractValidator<ApplicationReviewsToUnsuccessfulRouteModel>
    {
        public ApplicationReviewsToUnsuccessfulRouteModelValidator()
        {
            RuleFor(x => x.ApplicationsToUnsuccessful)
                .Must(apps => apps != null && apps.Any())
                .WithMessage(ApplicationReviewValidator.ApplicationReviewsToUnsuccessful);
        }
    }
}
