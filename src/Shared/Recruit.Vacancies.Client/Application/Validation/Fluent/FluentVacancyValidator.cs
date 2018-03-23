using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public class FluentVacancyValidator : AbstractValidator<Vacancy>
    {
        private readonly ITimeProvider _timeProvider;

        public FluentVacancyValidator(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;

            SingleFieldValidations();

            CrossFieldValidations();
        }

        private void SingleFieldValidations()
        {
            ValidateDescription();

            ValidateOrganisation();

            ValidateNumberOfPositions();

            ValidateShortDescription();

            ValidateClosingDate();

            ValidateStartDate();

            ValidateTrainingProgramme();
        }

        private void CrossFieldValidations()
        {
            ValidateStartDateClosingDate();
        }

        private void ValidateDescription()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Enter the title of the vacancy")
                    .WithErrorCode("1")
                .MaximumLength(100)
                    .WithMessage("The title must not be more than {MaxLength}")
                    .WithErrorCode("2")
                .ValidFreeTextCharacters()
                    .WithMessage("The title contains some invalid characters")
                    .WithErrorCode("3")
                .RunCondition(VacancyRuleSet.Title)
                .WithRuleId(VacancyRuleSet.Title);
        }

        private void ValidateOrganisation()
        {
            RuleFor(x => x.OrganisationId)
                .NotEmpty()
                    .WithMessage("You must select one organisation")
                    .WithErrorCode("4")
                .RunCondition(VacancyRuleSet.OrganisationId)
                .WithRuleId(VacancyRuleSet.OrganisationId);

            RuleFor(x => x.Location)
                .SetValidator(new AddressValidator((long)VacancyRuleSet.OrganisationAddress))
                .RunCondition(VacancyRuleSet.OrganisationAddress)
                .WithRuleId(VacancyRuleSet.OrganisationAddress);
        }

        private void ValidateNumberOfPositions()
        {
            RuleFor(x => x.NumberOfPositions)
                .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage("Enter the number of positions for this vacancy")
                    .WithErrorCode("10")
                .RunCondition(VacancyRuleSet.NumberOfPostions)
                .WithRuleId(VacancyRuleSet.NumberOfPostions);
        }

        private void ValidateShortDescription()
        {
            RuleFor(x => x.ShortDescription)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Enter the brief overview of the vacancy")
                    .WithErrorCode("12")
                .MaximumLength(350)
                    .WithMessage("The overview of the vacancy must not be more than {MaxLength} characters")
                    .WithErrorCode("13")
                .MinimumLength(50)
                    .WithMessage("The overview of the vacancy must be more than {MinLength} characters")
                    .WithErrorCode("14")
                .ValidFreeTextCharacters()
                    .WithMessage("The overview of the vacancy contains some invalid characters")
                    .WithErrorCode("15")
                .RunCondition(VacancyRuleSet.ShortDescription)
                .WithRuleId(VacancyRuleSet.ShortDescription);

        }

        private void ValidateClosingDate()
        {
            RuleFor(x => x.ClosingDate)
                .NotNull()
                    .WithMessage("Enter the closing date for applications")
                    .WithErrorCode("16")
                .GreaterThan(_timeProvider.Now.AddDays(1))
                    .WithMessage("The closing date can't be today or earlier. We advise using a date more than two weeks from now")
                    .WithErrorCode("18")
                .RunCondition(VacancyRuleSet.ClosingDate)
                .WithRuleId(VacancyRuleSet.ClosingDate);
        }

        private void ValidateStartDate()
        {
            RuleFor(x => x.StartDate)
                .NotNull()
                .WithMessage("Enter the possible start date")
                    .WithErrorCode("20")
                .GreaterThan(_timeProvider.Now.AddDays(1))
                .WithMessage("The possible start date can't be today or earlier. We advise using a date more than two weeks from now")
                    .WithErrorCode("22")
                .RunCondition(VacancyRuleSet.StartDate)
                .WithRuleId(VacancyRuleSet.StartDate);
        }

        private void ValidateTrainingProgramme()
        {
            RuleFor(x => x.Programme)
                .SetValidator(new TrainingProgrammeValidator((long)VacancyRuleSet.TrainingProgramme))
                    .RunCondition(VacancyRuleSet.TrainingProgramme)
                .WithRuleId(VacancyRuleSet.TrainingProgramme);
        }

        private void ValidateStartDateClosingDate()
        {
            When(x => x.StartDate.HasValue && x.ClosingDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .ClosingDateMustBeLessThanStartDate()
                .RunCondition(VacancyRuleSet.StartDateEndDate)
                .WithRuleId(VacancyRuleSet.StartDateEndDate);
            });
        }
    }
}