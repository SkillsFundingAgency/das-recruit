using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public sealed class FluentVacancyValidator : AbstractValidator<Vacancy>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IGetMinimumWages _minimumWageService;
        private readonly Lazy<IEnumerable<ApprenticeshipProgramme>> _trainingProgrammes;
        private readonly QualificationsConfiguration _qualificationsConfiguration;

        public FluentVacancyValidator(ITimeProvider timeProvider, IGetMinimumWages minimumWageService, IQueryStoreReader queryStoreReader, IOptions<QualificationsConfiguration> qualificationsConfiguration)
        {
            _timeProvider = timeProvider;
            _minimumWageService = minimumWageService;
            _trainingProgrammes = new Lazy<IEnumerable<ApprenticeshipProgramme>>(() => queryStoreReader.GetApprenticeshipProgrammesAsync().Result.Programmes);
            _qualificationsConfiguration = qualificationsConfiguration.Value;

            SingleFieldValidations();

            CrossFieldValidations();
        }

        private void SingleFieldValidations()
        {
            ValidateTitle();

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

            ValidateSkills();

            ValidateQualifications();

            ValidateDescription();

            ValidateTrainingDescription();

            ValidateOutcomeDescription();

            ValidateApplicationUrl();

            ValidateApplicationInstructions();

            ValidateEmployerContactDetails();
        }

        private void CrossFieldValidations()
        {
            ValidateStartDateClosingDate();

            MinimumWageValidation();

            TrainingExpiryDateValidation();
        }

        private void ValidateTitle()
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
            RuleFor(x => x.EmployerName)
                .NotEmpty()
                    .WithMessage("You must select one organisation")
                    .WithErrorCode("4")
                .RunCondition(VacancyRuleSet.EmployerName)
                .WithRuleId(VacancyRuleSet.EmployerName);

            RuleFor(x => x.EmployerLocation)
                .SetValidator(new AddressValidator((long)VacancyRuleSet.EmployerAddress))
                .RunCondition(VacancyRuleSet.EmployerAddress)
                .WithRuleId(VacancyRuleSet.EmployerAddress);
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
                .GreaterThan(_timeProvider.Now.Date.AddDays(1).AddTicks(-1))
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
                .GreaterThan(_timeProvider.Now.Date.AddDays(1).AddTicks(-1))
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
                .GreaterThanOrEqualTo(16)
                    .WithMessage("The total hours a week must be at least {ComparisonValue}")
                    .WithErrorCode("42")
                .LessThanOrEqualTo(48)
                    .WithMessage("The paid hours a week must not be more than {ComparisonValue}")
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

        private void ValidateSkills()
        {
            RuleFor(x => x.Skills)
                .Must(s => s != null && s.Count > 0)
                    .WithMessage("You must include a skill or quality")
                    .WithErrorCode("51")
                .RunCondition(VacancyRuleSet.Skills)
                .WithRuleId(VacancyRuleSet.Skills);

            RuleForEach(x => x.Skills)
                .NotEmpty()
                    .WithMessage("You must include a skill or quality")
                    .WithErrorCode("51")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(100)
                    .WithMessage("The skill or quality must be less than {MaxLength} characters")
                    .WithErrorCode("7")
                .WithRuleId(VacancyRuleSet.Skills);
        }

        private void ValidateQualifications()
        {
            RuleFor(x => x.Qualifications)
                .Must(q => q != null && q.Count > 0)
                    .WithMessage("You must have at least one qualification")
                    .WithErrorCode("52")
                .SetCollectionValidator(new QualificationValidator((long)VacancyRuleSet.Qualifications, _qualificationsConfiguration))
                .RunCondition(VacancyRuleSet.Qualifications)
                .WithRuleId(VacancyRuleSet.Qualifications);
        }

        private void ValidateDescription()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage("You must give information on what the apprenticeship will involve")
                    .WithErrorCode("53")
                .MaximumLength(500)
                    .WithMessage("This section must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .RunCondition(VacancyRuleSet.Description)
                .WithRuleId(VacancyRuleSet.Description);
        }

        private void ValidateTrainingDescription()
        {
            RuleFor(x => x.TrainingDescription)
                .NotEmpty()
                    .WithMessage("You must give information on the training to be provided")
                    .WithErrorCode("54")
                .MaximumLength(500)
                    .WithMessage("This section must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .RunCondition(VacancyRuleSet.TrainingDescription)
                .WithRuleId(VacancyRuleSet.TrainingDescription);
        }

        private void ValidateOutcomeDescription()
        {
            RuleFor(x => x.OutcomeDescription)
                .NotEmpty()
                    .WithMessage("You must give information on what to expect at the end of the apprenticeship")
                    .WithErrorCode("55")
                .MaximumLength(500)
                    .WithMessage("This section must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .RunCondition(VacancyRuleSet.OutcomeDescription)
                .WithRuleId(VacancyRuleSet.OutcomeDescription);
        }

        private void ValidateApplicationUrl()
        {
            RuleFor(x => x.ApplicationUrl)
                .NotEmpty()
                    .WithMessage("Enter a valid website address")
                    .WithErrorCode("85")
                .MaximumLength(200)
                    .WithMessage("The website address must not be more than {MaxLength} characters")
                    .WithErrorCode("84")
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Enter a valid website address")
                    .WithErrorCode("86")
                .RunCondition(VacancyRuleSet.ApplicationUrl)
                .WithRuleId(VacancyRuleSet.ApplicationUrl);
        }

        private void ValidateApplicationInstructions()
        {
            RuleFor(x => x.ApplicationInstructions)
                .MaximumLength(500)
                    .WithMessage("The application process should be less than {MaxLength} characters")
                    .WithErrorCode("88")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("89")
                .RunCondition(VacancyRuleSet.ApplicationInstructions)
                .WithRuleId(VacancyRuleSet.ApplicationInstructions);
        }

        private void ValidateEmployerContactDetails()
        {
            RuleFor(x => x.EmployerContactName)
                .MaximumLength(100)
                    .WithMessage("Contact details should be less than {MaxLength} characters")
                    .WithErrorCode("90")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("91")
                .RunCondition(VacancyRuleSet.EmployerContactDetails)
                .WithRuleId(VacancyRuleSet.EmployerContactDetails);

            RuleFor(x => x.EmployerContactEmail)
                .MaximumLength(100)
                    .WithMessage("Email address must not be more than {MaxLength} characters")
                    .WithErrorCode("92")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("93")
                .Matches(ValidationConstants.EmailAddressRegex)
                    .WithMessage("Enter a valid email address")
                    .WithErrorCode("94")
                    .When(v => !string.IsNullOrEmpty(v.EmployerContactEmail))
                .RunCondition(VacancyRuleSet.EmployerContactDetails)
                .WithRuleId(VacancyRuleSet.EmployerContactDetails);

            RuleFor(x => x.EmployerContactPhone)
                .MaximumLength(16)
                    .WithMessage("Contact number must be less than {MaxLength} digits")
                    .WithErrorCode("95")
                .MinimumLength(8)
                    .WithMessage("Contact number must be more than {MinLength} digits")
                    .WithErrorCode("96")
                .Matches(ValidationConstants.PhoneNumberRegex)
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("97")
                    .When(v => !string.IsNullOrEmpty(v.EmployerContactPhone))
                .RunCondition(VacancyRuleSet.EmployerContactDetails)
                .WithRuleId(VacancyRuleSet.EmployerContactDetails);
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