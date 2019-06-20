﻿using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider
{
    public class SelectTrainingProviderEditModelValidator : AbstractValidator<SelectTrainingProviderEditModel>
    {
        public SelectTrainingProviderEditModelValidator()
        {
            RuleFor(m => m.IsTrainingProviderSelected)
                .NotNull()
                .WithMessage(ValidationMessages.TrainingProviderValidationMessages.IsTrainingProviderSelectedNotNull);

            When(m => m.IsTrainingProviderSelected == true && 
                      m.SelectionType == TrainingProviderSelectionType.Ukprn, () =>
            {
                RuleFor(m => m.Ukprn)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage(ValidationMessages.TrainingProviderValidationMessages.UkprnNotEmpty)
                    .Matches(ValidationConstants.UkprnRegex.ToString())
                    .WithMessage(ValidationMessages.TrainingProviderValidationMessages.UkprnIsValid);
            });

            When(m => m.IsTrainingProviderSelected == true && 
                      m.SelectionType == TrainingProviderSelectionType.TrainingProviderSearch, () =>
            {
                RuleFor(m => m.TrainingProviderSearch)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage(ValidationMessages.TrainingProviderValidationMessages.TrainingProviderSearchNotEmpty);
            });
        }
    }
}
