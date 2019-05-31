using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.TrainingProvider
{
    public class SelectTrainingProviderEditModelValidator : AbstractValidator<SelectTrainingProviderEditModel>
    {
        public SelectTrainingProviderEditModelValidator()
        {
            When(m => m.SelectionType == TrainingProviderSelectionType.Ukprn, () =>
            {
                RuleFor(m => m.Ukprn)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage("The UKPRN field is required")
                    .Matches(ValidationConstants.UkprnRegex.ToString())
                    .WithMessage("UKPRN is not recognised");
            });

            When(m => m.SelectionType == TrainingProviderSelectionType.TrainingProviderSearch, () =>
            {
                RuleFor(m => m.TrainingProviderSearch)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage("Please select a training provider");
            });
        }
    }
}
