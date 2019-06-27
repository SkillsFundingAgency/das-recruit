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
                    .WithMessage("You must provide a UKPRN")
                    .Matches(ValidationConstants.UkprnRegex.ToString())
                    .WithMessage("You must provide a valid UKPRN");
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
