using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public sealed class FluentVacancyValidator : AbstractValidator<Vacancy>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IGetApprenticeshipNationalMinimumWages _minimumWageService;
        private readonly Lazy<IEnumerable<ApprenticeshipProgramme>> _trainingProgrammes;

        public FluentVacancyValidator(ITimeProvider timeProvider, IGetApprenticeshipNationalMinimumWages minimumWageService, IQueryStoreReader queryStoreReader)
        {
            _timeProvider = timeProvider;
            _minimumWageService = minimumWageService;
            _trainingProgrammes = new Lazy<IEnumerable<ApprenticeshipProgramme>>(() => queryStoreReader.GetApprenticeshipProgrammesAsync().Result.Programmes);

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

            ValidateDuration();

            ValidateWorkingWeek();

            ValidateWeeklyHours();

            ValidateWage();
        }

        private void CrossFieldValidations()
        {
            ValidateStartDateClosingDate();

            MinimumWageValidation();

            TrainingExpiryDateValidation();
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
            RuleFor(x => x.Programme.Id)
                .NotEmpty()
                    .WithMessage("Select  apprenticeship training")
                    .WithErrorCode("25")
                .WithRuleId(VacancyRuleSet.TrainingProgramme)
                .RunCondition(VacancyRuleSet.TrainingProgramme);
        }

        private void ValidateDuration()
        {
            RuleFor(x => x.Wage.DurationUnit)
                .NotEmpty()
                    .WithMessage("Enter the expected duaration")
                    .WithErrorCode("34")
                .IsInEnum()
                    .WithMessage("Enter the expected duaration")
                    .WithErrorCode("34")
                .RunCondition(VacancyRuleSet.Duration)
                .WithRuleId(VacancyRuleSet.Duration);

            RuleFor(x => x.Wage.Duration)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                    .WithMessage("Enter the expected duaration")
                    .WithErrorCode("34")
                .GreaterThan(0)
                    .WithMessage("Enter the expected duaration")
                    .WithErrorCode("34")
                .Must((vacancy, value) => 
                {
                    if (vacancy.Wage.DurationUnit == DurationUnit.Month && value < 12)
                    {
                        return false;
                    }

                    return true;
                })
                    .WithMessage("The expected duration must be at least 12 months (52 weeks)")
                    .WithErrorCode("36")
                .RunCondition(VacancyRuleSet.Duration)
                .WithRuleId(VacancyRuleSet.Duration);
        }

        private void ValidateWorkingWeek()
        {
            RuleFor(x => x.Wage.WorkingWeekDescription)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                    .WithMessage("Enter the working week")
                    .WithErrorCode("37")
                .ValidFreeTextCharacters()
                    .WithMessage("The working week contains some invalid characters")
                    .WithErrorCode("38")
                .MaximumLength(250)
                    .WithMessage("The working week must not be more than {MaxLength} characters")
                    .WithErrorCode("39")
                .RunCondition(VacancyRuleSet.WorkingWeekDescription)
                .WithRuleId(VacancyRuleSet.WorkingWeekDescription);
        }

        private void ValidateWeeklyHours()
        {
            RuleFor(x => x.Wage.WeeklyHours)
                .NotEmpty()
                    .WithMessage("Enter the hours per week")
                    .WithErrorCode("40")
                .GreaterThan(16)
                    .WithMessage("The total hours a week must be more than {ComparisonValue}")
                    .WithErrorCode("42")
                .LessThan(48)
                    .WithMessage("  The paid hours a week must be less than {ComparisonValue}")
                    .WithErrorCode("43")
                .RunCondition(VacancyRuleSet.WeeklyHours)
                .WithRuleId(VacancyRuleSet.WeeklyHours);
        }

        private void ValidateWage()
        {
            RuleFor(x => x.Wage.WageType)
                .NotEmpty()
                    .WithMessage("Select a wage")
                    .WithErrorCode("46")
                .IsInEnum()
                    .WithMessage("Select a wage")
                    .WithErrorCode("46")
                .RunCondition(VacancyRuleSet.Wage)
                .WithRuleId(VacancyRuleSet.Wage);

            RuleFor(x => x.Wage.WageAdditionalInformation)
                .MaximumLength(241)
                    .WithMessage("Additional salary information must not be more than {MaxLength} characters")
                    .WithErrorCode("44")
                .ValidFreeTextCharacters()
                    .WithMessage("Additional salary information contains some invalid characters")
                    .WithErrorCode("45")
                .RunCondition(VacancyRuleSet.Wage)
                .WithRuleId(VacancyRuleSet.Wage);

            When(x => x.Wage != null && x.Wage.WageType == WageType.Unspecified, () =>
            {
                RuleFor(x => x.Wage.WageAdditionalInformation)
                    .NotEmpty()
                        .WithMessage("Enter a reason why you need to use Unspecified")
                        .WithErrorCode("50")
                    .RunCondition(VacancyRuleSet.Wage)
                    .WithRuleId(VacancyRuleSet.Wage);
            });
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

        private void MinimumWageValidation()
        {
            When(x => x.Wage != null && x.Wage.WageType == WageType.FixedWage, () =>
            {
                RuleFor(x => x)
                    .FixedWageMustBeGreaterThanApprenticeshipMinimumWage(_minimumWageService)
                .RunCondition(VacancyRuleSet.MinimumWage)
                .WithRuleId(VacancyRuleSet.MinimumWage);
            });
        }

        private void TrainingExpiryDateValidation()
        {
            When(x => x.Programme != null && !string.IsNullOrWhiteSpace(x.Programme.Id) && x.StartDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .TrainingMustBeActiveForStartDate(_trainingProgrammes)
                .RunCondition(VacancyRuleSet.TrainingExpiryDate)
                .WithRuleId(VacancyRuleSet.TrainingExpiryDate);
            });
        }
    }
}