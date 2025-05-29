using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using FluentValidation;


namespace Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent
{
    public class ApplicationReviewsToShareModelValidator : AbstractValidator<ApplicationReviewsToShareRouteModel>
    {
        public ApplicationReviewsToShareModelValidator()
        {
            RuleFor(x => x.ApplicationsToShare)
                .Must(apps => apps != null && apps.Any())
                .WithMessage(ApplicationReviewValidator.ApplicationReviewsToShare);
        }
    }
}
