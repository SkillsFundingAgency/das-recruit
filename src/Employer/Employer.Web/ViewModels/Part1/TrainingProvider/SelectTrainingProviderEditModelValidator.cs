using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider
{
    public class SelectTrainingProviderEditModelValidator : AbstractValidator<SelectTrainingProviderEditModel>
    {
        public SelectTrainingProviderEditModelValidator()
        {
            When(m=>
                      m.SelectionType == TrainingProviderSelectionType.Ukprn, () =>
            {
                RuleFor(m => m.Ukprn)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage(ValidationMessages.TrainingProviderValidationMessages.UkprnNotEmpty)
                    .Matches(ValidationConstants.UkprnRegex.ToString())
                    .WithMessage(ValidationMessages.TrainingProviderValidationMessages.UkprnIsValid);
            });

            When(m => m.SelectionType == TrainingProviderSelectionType.TrainingProviderSearch, () =>
            {
                RuleFor(m => m.TrainingProviderSearch)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage(ValidationMessages.TrainingProviderValidationMessages.TrainingProviderSearchNotEmpty);
            });
        }
    }
}
