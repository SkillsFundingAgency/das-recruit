using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider
{
    public class ConfirmTrainingProviderEditModelValidator : AbstractValidator<ConfirmTrainingProviderEditModel>
    {
        public ConfirmTrainingProviderEditModelValidator()
        {
            RuleFor(m => m.Ukprn)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("The UKPRN field is required")
                .Matches(ValidationConstants.UkprnRegex.ToString())
                .WithMessage("UKPRN is not recognised");
        }
    }
}
