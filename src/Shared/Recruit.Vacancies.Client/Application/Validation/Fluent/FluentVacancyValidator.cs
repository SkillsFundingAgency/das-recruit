using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public sealed class FluentVacancyValidator : AbstractValidator<Vacancy>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IMinimumWageProvider _minimumWageService;
        private readonly Lazy<IEnumerable<IApprenticeshipProgramme>> _trainingProgrammes;
        private readonly IList<string> _qualifications;

        public FluentVacancyValidator(ITimeProvider timeProvider, IMinimumWageProvider minimumWageService, IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider, IQualificationsProvider qualificationsProvider)
        {
            _timeProvider = timeProvider;
            _minimumWageService = minimumWageService;
            _trainingProgrammes = new Lazy<IEnumerable<IApprenticeshipProgramme>>(() => apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync().Result);
            _qualifications = qualificationsProvider.GetQualificationsAsync().Result;

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

            ValidateApplicationMethod();

            ValidateEmployerContactDetails();

            ValidateThingsToConsider();

            ValidateEmployerInformation();

            ValidateTrainingProvider();
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
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                    .WithMessage("You must provide a vacancy title")
                    .WithErrorCode("1")
                .MaximumLength(100)
                    .WithMessage("Title must not exceed {MaxLength} characters")
                    .WithErrorCode("2")
                .ValidFreeTextCharacters()
                    .WithMessage("Title contains some invalid characters")
                    .WithErrorCode("3")
                .Matches(ValidationConstants.ContainsApprenticeOrApprenticeshipRegex)
                    .WithMessage("Title must contain the word 'apprentice' or 'apprenticeship'")
                    .WithErrorCode("200")
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
                .NotNull()
                    .WithMessage("You must provide an employer location")
                    .WithErrorCode("98")
                .SetValidator(new AddressValidator((long)VacancyRuleSet.EmployerAddress))
                .RunCondition(VacancyRuleSet.EmployerAddress)
                .WithRuleId(VacancyRuleSet.EmployerAddress);
        }

        private void ValidateNumberOfPositions()
        {
            RuleFor(x => x.NumberOfPositions)
                .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage("You must state the number of positions for this vacancy")
                    .WithErrorCode("10")
                .RunCondition(VacancyRuleSet.NumberOfPositions)
                .WithRuleId(VacancyRuleSet.NumberOfPositions);
        }

        private void ValidateShortDescription()
        {
            RuleFor(x => x.ShortDescription)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Enter the brief overview of the vacancy")
                    .WithErrorCode("12")
                .MaximumLength(350)
                    .WithMessage("The overview of the vacancy must not exceed {MaxLength} characters")
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
                    .WithMessage("You must provide the closing date for applications")
                    .WithErrorCode("16")
                .GreaterThan(_timeProvider.Now.Date.AddDays(1).AddTicks(-1))
                    .WithMessage("Application closing date can't be today or earlier. We advise using a date more than two weeks from now.")
                    .WithErrorCode("18")
                .RunCondition(VacancyRuleSet.ClosingDate)
                .WithRuleId(VacancyRuleSet.ClosingDate);
        }

        private void ValidateStartDate()
        {
            RuleFor(x => x.StartDate)
                .NotNull()
                .WithMessage("You must provide a possible apprenticeship start date")
                    .WithErrorCode("20")
                .GreaterThan(_timeProvider.Now.Date.AddDays(1).AddTicks(-1))
                .WithMessage("Possible apprenticeship start date can't be today or earlier. We advise using a date more than two weeks from now.")
                    .WithErrorCode("22")
                .RunCondition(VacancyRuleSet.StartDate)
                .WithRuleId(VacancyRuleSet.StartDate);
        }

        private void ValidateTrainingProgramme()
        {
            RuleFor(x => x.ProgrammeId)
                .NotEmpty()
                    .WithMessage("You must select an apprenticeship training programme")
                    .WithErrorCode("25")
                .WithRuleId(VacancyRuleSet.TrainingProgramme)
                .RunCondition(VacancyRuleSet.TrainingProgramme);
        }

        private void ValidateDuration()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.DurationUnit)
                    .NotEmpty()
                    .WithMessage("You must state the expected duration")
                    .WithErrorCode("34")
                    .IsInEnum()
                    .WithMessage("You must state the expected duration")
                    .WithErrorCode("34")
                    .RunCondition(VacancyRuleSet.Duration)
                    .WithRuleId(VacancyRuleSet.Duration);

                RuleFor(x => x.Wage.Duration)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage("You must state the expected duration")
                    .WithErrorCode("34")
                    .GreaterThan(0)
                    .WithMessage("You must state the expected duration")
                    .WithErrorCode("34")
                    .Must((vacancy, value) =>
                    {
                        if (vacancy.Wage.DurationUnit == DurationUnit.Month && value < 12)
                        {
                            return false;
                        }

                        return true;
                    })
                    .WithMessage("Expected duration must be at least 12 months (52 weeks)")
                    .WithErrorCode("36")
                    .RunCondition(VacancyRuleSet.Duration)
                    .WithRuleId(VacancyRuleSet.Duration);
            });
        }

        private void ValidateWorkingWeek()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WorkingWeekDescription)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                        .WithMessage("You must include details of the working week")
                        .WithErrorCode("37")
                    .ValidFreeTextCharacters()
                        .WithMessage("Working week details contains some invalid characters")
                        .WithErrorCode("38")
                    .MaximumLength(250)
                        .WithMessage("Working week details must not exceed {MaxLength} characters")
                        .WithErrorCode("39")
                    .RunCondition(VacancyRuleSet.WorkingWeekDescription)
                    .WithRuleId(VacancyRuleSet.WorkingWeekDescription);
            });
        }

        private void ValidateWeeklyHours()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WeeklyHours)
                    .NotEmpty()
                        .WithMessage("You must state the total working hours per week")
                        .WithErrorCode("40")
                    .GreaterThanOrEqualTo(16)
                        .WithMessage("The total hours a week must be at least {ComparisonValue}")
                        .WithErrorCode("42")
                    .LessThanOrEqualTo(48)
                        .WithMessage("The total hours a week must not be more than {ComparisonValue}")
                        .WithErrorCode("43")
                    .RunCondition(VacancyRuleSet.WeeklyHours)
                    .WithRuleId(VacancyRuleSet.WeeklyHours);
            });
        }

        private void ValidateWage()
        {
            RuleFor(x => x.Wage)
                .NotNull()
                    .WithMessage("You must select a wage")
                    .WithErrorCode("46")
                .RunCondition(VacancyRuleSet.Wage)
                .WithRuleId(VacancyRuleSet.Wage);

            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WageType)
                    .NotEmpty()
                        .WithMessage("You must select a wage")
                        .WithErrorCode("46")
                    .IsInEnum()
                        .WithMessage("You must select a wage")
                        .WithErrorCode("46")
                    .RunCondition(VacancyRuleSet.Wage)
                    .WithRuleId(VacancyRuleSet.Wage);

                RuleFor(x => x.Wage.WageAdditionalInformation)
                    .MaximumLength(250)
                        .WithMessage("Additional salary information must not exceed {MaxLength} characters")
                        .WithErrorCode("44")
                    .ValidFreeTextCharacters()
                        .WithMessage("Additional salary information contains some invalid characters")
                        .WithErrorCode("45")
                    .RunCondition(VacancyRuleSet.Wage)
                    .WithRuleId(VacancyRuleSet.Wage);

                When(x => x.Wage.WageType == WageType.Unspecified, () =>
                {
                    RuleFor(x => x.Wage.WageAdditionalInformation)
                        .NotEmpty()
                        .WithMessage("You must provide a reason why you need to use Unspecified")
                        .WithErrorCode("50")
                        .RunCondition(VacancyRuleSet.Wage)
                        .WithRuleId(VacancyRuleSet.Wage);
                });
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
                    .WithErrorCode("99")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(100)
                    .WithMessage("Skill or quality must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .WithRuleId(VacancyRuleSet.Skills);
        }

        private void ValidateQualifications()
        {
            RuleFor(x => x.Qualifications)
                .Must(q => q != null && q.Count > 0)
                    .WithMessage("You must have at least one qualification")
                    .WithErrorCode("52")
                .SetCollectionValidator(new QualificationValidator((long)VacancyRuleSet.Qualifications, _qualifications))
                .RunCondition(VacancyRuleSet.Qualifications)
                .WithRuleId(VacancyRuleSet.Qualifications);
        }

        private void ValidateDescription()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage("You must provide information on what the apprenticeship will involve")
                    .WithErrorCode("53")
                .MaximumLength(500)
                    .WithMessage("This section must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("Typical working day description contains some invalid characters")
                    .WithErrorCode("6")
                .RunCondition(VacancyRuleSet.Description)
                .WithRuleId(VacancyRuleSet.Description);
        }

        private void ValidateTrainingDescription()
        {
            RuleFor(x => x.TrainingDescription)
                .NotEmpty()
                    .WithMessage("You must provide information on the training to be provided")
                    .WithErrorCode("54")
                .MaximumLength(500)
                    .WithMessage("Training description must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("Training description contains some invalid characters")
                    .WithErrorCode("6")
                .RunCondition(VacancyRuleSet.TrainingDescription)
                .WithRuleId(VacancyRuleSet.TrainingDescription);
        }

        private void ValidateOutcomeDescription()
        {
            RuleFor(x => x.OutcomeDescription)
                .NotEmpty()
                    .WithMessage("You must provide information on what to expect at the end of the apprenticeship")
                    .WithErrorCode("55")
                .MaximumLength(500)
                    .WithMessage("This section must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .RunCondition(VacancyRuleSet.OutcomeDescription)
                .WithRuleId(VacancyRuleSet.OutcomeDescription);
        }

        private void ValidateApplicationMethod()
        {
            RuleFor(x => x.ApplicationMethod)
                    .NotEmpty()
                        .WithMessage("You must select an application method")
                        .WithErrorCode("85")
                    .IsInEnum()
                        .WithMessage("You must select an application method")
                        .WithErrorCode("85")
                    .RunCondition(VacancyRuleSet.ApplicationMethod)
                    .WithRuleId(VacancyRuleSet.ApplicationMethod);

            When(x => x.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship, () =>
            {
                RuleFor(x => x.ApplicationUrl)
                    .Empty()
                        .WithMessage("Application url must be empty when apply through Find an apprenticeship service option is specified")
                        .WithErrorCode("86")
                    .RunCondition(VacancyRuleSet.ApplicationMethod)
                    .WithRuleId(VacancyRuleSet.ApplicationMethod);

                RuleFor(x => x.ApplicationInstructions)
                    .Empty()
                        .WithMessage("Application process must be empty when apply through Find an apprenticeship service option is specified")
                        .WithErrorCode("89")
                    .RunCondition(VacancyRuleSet.ApplicationMethod)
                    .WithRuleId(VacancyRuleSet.ApplicationMethod);
            });

            When(x => x.ApplicationMethod == ApplicationMethod.ThroughExternalApplicationSite, () =>
            {
                ValidateApplicationUrl();
                ValidateApplicationInstructions();
            });
        }

        private void ValidateApplicationUrl()
        {
            RuleFor(x => x.ApplicationUrl)
                .NotEmpty()
                    .WithMessage("Enter a valid website address")
                    .WithErrorCode("85")
                .MaximumLength(200)
                    .WithMessage("The website address must not exceed {MaxLength} characters")
                    .WithErrorCode("84")
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Enter a valid website address")
                    .WithErrorCode("86")
                .RunCondition(VacancyRuleSet.ApplicationMethod)
                .WithRuleId(VacancyRuleSet.ApplicationMethod);
        }

        private void ValidateApplicationInstructions()
        {
            RuleFor(x => x.ApplicationInstructions)
                .MaximumLength(500)
                    .WithMessage("Application process must not exceed {MaxLength} characters")
                    .WithErrorCode("88")
                .ValidFreeTextCharacters()
                    .WithMessage("Application process contains some invalid characters")
                    .WithErrorCode("89")
                .RunCondition(VacancyRuleSet.ApplicationMethod)
                .WithRuleId(VacancyRuleSet.ApplicationMethod);
        }

        private void ValidateEmployerContactDetails()
        {
            RuleFor(x => x.EmployerContactName)
                .MaximumLength(100)
                    .WithMessage("Contact name must not exceed {MaxLength} characters")
                    .WithErrorCode("90")
                .ValidFreeTextCharacters()
                    .WithMessage("Contact name contains some invalid characters")
                    .WithErrorCode("91")
                .RunCondition(VacancyRuleSet.EmployerContactDetails)
                .WithRuleId(VacancyRuleSet.EmployerContactDetails);

            RuleFor(x => x.EmployerContactEmail)
                .MaximumLength(100)
                    .WithMessage("Email address must not exceed {MaxLength} characters")
                    .WithErrorCode("92")
                .ValidFreeTextCharacters()
                    .WithMessage("Email address contains some invalid characters")
                    .WithErrorCode("93")
                .Matches(ValidationConstants.EmailAddressRegex)
                    .WithMessage("Email address must be in a valid format")
                    .WithErrorCode("94")
                    .When(v => !string.IsNullOrEmpty(v.EmployerContactEmail))
                .RunCondition(VacancyRuleSet.EmployerContactDetails)
                .WithRuleId(VacancyRuleSet.EmployerContactDetails);

            RuleFor(x => x.EmployerContactPhone)
                .MaximumLength(16)
                    .WithMessage("Contact number must not exceed {MaxLength} digits")
                    .WithErrorCode("95")
                .MinimumLength(8)
                    .WithMessage("Contact number must be more than {MinLength} digits")
                    .WithErrorCode("96")
                .Matches(ValidationConstants.PhoneNumberRegex)
                    .WithMessage("Contact number contains some invalid characters")
                    .WithErrorCode("97")
                    .When(v => !string.IsNullOrEmpty(v.EmployerContactPhone))
                .RunCondition(VacancyRuleSet.EmployerContactDetails)
                .WithRuleId(VacancyRuleSet.EmployerContactDetails);
        }

        private void ValidateThingsToConsider()
        {
            RuleFor(x => x.ThingsToConsider)
                .MaximumLength(350)
                    .WithMessage("Things to consider must not exceed {MaxLength} characters")
                    .WithErrorCode("75")
                .ValidFreeTextCharacters()
                    .WithMessage("Things to consider contains some invalid characters")
                    .WithErrorCode("76")
                .RunCondition(VacancyRuleSet.ThingsToConsider)
                .WithRuleId(VacancyRuleSet.ThingsToConsider);
        }

        private void ValidateEmployerInformation()
        {
            RuleFor(x => x.EmployerDescription)
                .NotEmpty()
                    .WithMessage("You must provide an employer description")
                    .WithErrorCode("80")
                .MaximumLength(500)
                    .WithMessage("Employer description must not exceed {MaxLength} characters")
                    .WithErrorCode("77")
                .ValidFreeTextCharacters()
                    .WithMessage("Employer description contains some invalid characters")
                    .WithErrorCode("78")
                .RunCondition(VacancyRuleSet.EmployerDescription)
                .WithRuleId(VacancyRuleSet.EmployerDescription);

            RuleFor(x => x.EmployerWebsiteUrl)
                .MaximumLength(100)
                    .WithMessage("Employers website address must not exceed {MaxLength} characters")
                    .WithErrorCode("84")
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Employers website address must be a valid website address")
                    .WithErrorCode("82")
                    .When(v => !string.IsNullOrEmpty(v.EmployerWebsiteUrl))
                .RunCondition(VacancyRuleSet.EmployerWebsiteUrl)
                .WithRuleId(VacancyRuleSet.EmployerWebsiteUrl);
        }

        private void ValidateTrainingProvider()
        {
            RuleFor(x => x.TrainingProvider)
                .NotNull()
                    .WithMessage("You must enter a training provider")
                    .WithErrorCode("101")
                .SetValidator(new TrainingProviderValidator((long)VacancyRuleSet.TrainingProvider))
                .RunCondition(VacancyRuleSet.TrainingProvider)
                .WithRuleId(VacancyRuleSet.TrainingProvider);
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
            When(x => x.ProgrammeId != null && !string.IsNullOrWhiteSpace(x.ProgrammeId) && x.StartDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .TrainingMustBeActiveForStartDate(_trainingProgrammes)
                .RunCondition(VacancyRuleSet.TrainingExpiryDate)
                .WithRuleId(VacancyRuleSet.TrainingExpiryDate);
            });
        }
    }
}
