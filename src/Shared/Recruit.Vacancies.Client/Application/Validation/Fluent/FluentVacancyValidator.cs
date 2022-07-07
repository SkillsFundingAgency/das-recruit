using System;
using System.Diagnostics;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public sealed class FluentVacancyValidator : AbstractValidator<Vacancy>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IMinimumWageProvider _minimumWageService;
        private readonly IQualificationsProvider _qualificationsProvider;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly ITrainingProviderSummaryProvider _trainingProviderSummaryProvider;
        private readonly IBlockedOrganisationQuery _blockedOrganisationRepo;
        private readonly IProfanityListProvider _profanityListProvider;
        private readonly IProviderRelationshipsService _providerRelationshipService;
        private readonly ServiceParameters _serviceParameters;

        public FluentVacancyValidator(
            ITimeProvider timeProvider,
            IMinimumWageProvider minimumWageService,
            IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
            IQualificationsProvider qualificationsProvider,
            IHtmlSanitizerService htmlSanitizerService,
            ITrainingProviderSummaryProvider trainingProviderSummaryProvider,
            IBlockedOrganisationQuery blockedOrganisationRepo,
            IProfanityListProvider profanityListProvider,
            IProviderRelationshipsService providerRelationshipService,
            ServiceParameters serviceParameters)
        {
            _timeProvider = timeProvider;
            _minimumWageService = minimumWageService;
            _qualificationsProvider = qualificationsProvider;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _htmlSanitizerService = htmlSanitizerService;
            _trainingProviderSummaryProvider = trainingProviderSummaryProvider;
            _blockedOrganisationRepo = blockedOrganisationRepo;
            _profanityListProvider = profanityListProvider;
            _providerRelationshipService = providerRelationshipService;
            _serviceParameters = serviceParameters;

            SingleFieldValidations();

            CrossFieldValidations();
        }

        private bool IsApprenticeshipVacancy
        {
            get
            {
                return _serviceParameters.VacancyType == VacancyType.Apprenticeship;
            }
        }

        private string VacancyContext
        {
            get
            {
                return IsApprenticeshipVacancy
                    ? "apprenticeship"
                    : "traineeship";
            }
        }

        private void SingleFieldValidations()
        {
            if (IsApprenticeshipVacancy)
            {
                ValidateApprenticeshipTitle();
                ValidateApprenticeshipDuration();
            }
            else
            {
                ValidateTraineeshipTitle();
                ValidateTraineeshipDuration();
            }


            ValidateOrganisation();

            ValidateNumberOfPositions();

            ValidateShortDescription();

            ValidateClosingDate();

            ValidateStartDate();

            if (IsApprenticeshipVacancy)
            {
                ValidateTrainingProgramme();
            }
            else
            {
                ValidateRoute();
            }

            if (IsApprenticeshipVacancy)
            {
                ValidateWorkingWeek();
                ValidateWeeklyHours();
                ValidateWage();
            }
            else
            {
                ValidateTraineeshipWorkingWeek();
            }

            ValidateSkills();

            if (IsApprenticeshipVacancy)
            {
                ValidateQualifications();
                ValidateDescription();
            }

            ValidateTrainingDescription();

            ValidateOutcomeDescription();

            ValidateApplicationMethod();

            ValidateEmployerContactDetails();

            ValidateProviderContactDetails();

            ValidateThingsToConsider();

            ValidateEmployerInformation();

            ValidateTrainingProvider();

            if (!IsApprenticeshipVacancy)
            {
                ValidateWorkExperience();
            }

        }



        private void CrossFieldValidations()
        {
            ValidateStartDateClosingDate();

            MinimumWageValidation();

            TrainingExpiryDateValidation();
        }

        private void ValidateApprenticeshipTitle()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.StopOnFirstFailure)
                   .NotEmpty()
                       .WithMessage("Enter a title for this apprenticeship")
                       .WithErrorCode("1")
                   .MaximumLength(100)
                       .WithMessage("Title must not exceed {MaxLength} characters")
                       .WithErrorCode("2")
                   .ValidFreeTextCharacters()
                       .WithMessage("Title contains some invalid characters")
                       .WithErrorCode("3")
                   .Matches(ValidationConstants.ContainsApprenticeOrApprenticeshipRegex)
                       .WithMessage("Enter a title which includes the word 'apprentice' or 'apprenticeship'")
                       .WithErrorCode("200")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Title must not contain a banned word or phrase.")
                .WithErrorCode("601")
                .RunCondition(VacancyRuleSet.Title)
                   .WithRuleId(VacancyRuleSet.Title);
        }
        private void ValidateTraineeshipTitle()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Enter a title for this traineeship")
                .WithErrorCode("1")
                .MaximumLength(100)
                .WithMessage("Title must not exceed {MaxLength} characters")
                .WithErrorCode("2")
                .ValidFreeTextCharacters()
                .WithMessage("Title contains some invalid characters")
                .WithErrorCode("3")
                .Matches(ValidationConstants.ContainsTraineeOrTraineeshipRegex)
                .WithMessage("Enter a title which includes the word 'trainee' or 'traineeship'")
                .WithErrorCode("200")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Title must not contain a banned word or phrase.")
                .WithErrorCode("601")
                .RunCondition(VacancyRuleSet.Title)
                .WithRuleId(VacancyRuleSet.Title);
        }

        private void ValidateOrganisation()
        {
            RuleFor(x => x.EmployerName)
                .NotEmpty()
                    .WithMessage("Select the employer name you want on your advert")
                    .WithErrorCode("4")
                .RunCondition(VacancyRuleSet.EmployerName)
                .WithRuleId(VacancyRuleSet.EmployerName);

            RuleFor(x => x.LegalEntityName)
                .NotEmpty()
                    .WithMessage("You must select one organisation")
                    .WithErrorCode("400")
                .RunCondition(VacancyRuleSet.LegalEntityName)
                .WithRuleId(VacancyRuleSet.LegalEntityName);

            When(v => v.EmployerNameOption == EmployerNameOption.TradingName, () =>
                RuleFor(x => x.EmployerName)
                    .NotEmpty()
                        .WithMessage("Enter the trading name")
                        .WithErrorCode("401")
                    .MaximumLength(100)
                        .WithMessage("The trading name must not exceed {MaxLength} characters")
                        .WithErrorCode("402")
                    .ValidFreeTextCharacters()
                        .WithMessage("The trading name contains some invalid characters")
                        .WithErrorCode("403")
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Trading name must not contain a banned word or phrase.")
                    .WithErrorCode("602")
                    .RunCondition(VacancyRuleSet.TradingName)
                    .WithRuleId(VacancyRuleSet.TradingName)
            );

            When(v => v.EmployerNameOption == EmployerNameOption.Anonymous, () =>
                RuleFor(x => x.EmployerName)
                    .NotEmpty()
                    .WithMessage("Enter a brief description of what the employer does")
                    .WithErrorCode("405")
                    .MaximumLength(100)
                    .WithMessage("The description must not be more than {MaxLength} characters")
                    .WithErrorCode("406")
                    .ValidFreeTextCharacters()
                    .WithMessage("The description contains some invalid characters")
                    .WithErrorCode("407")
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Description must not contain a banned word or phrase.")
                    .WithErrorCode("603")
                    .RunCondition(VacancyRuleSet.EmployerNameOption)
                    .WithRuleId(VacancyRuleSet.EmployerNameOption)
            );

            When(v => v.EmployerNameOption == EmployerNameOption.Anonymous, () =>
                RuleFor(x => x.AnonymousReason)
                    .NotEmpty()
                    .WithMessage("Enter why you want your advert to be anonymous")
                    .WithErrorCode("408")
                    .MaximumLength(200)
                    .WithMessage("The reason must not be more than {MaxLength} characters")
                    .WithErrorCode("409")
                    .ValidFreeTextCharacters()
                    .WithMessage("The reason contains some invalid characters")
                    .WithErrorCode("410")
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Reason must not contain a banned word or phrase.")
                    .WithErrorCode("604")
                    .RunCondition(VacancyRuleSet.EmployerNameOption)
                    .WithRuleId(VacancyRuleSet.EmployerNameOption)
            );

            RuleFor(x => x.EmployerNameOption)
                .NotEmpty()
                    .WithMessage("Select the employer name you want on your advert")
                    .WithErrorCode("404")
                .RunCondition(VacancyRuleSet.EmployerNameOption)
                .WithRuleId(VacancyRuleSet.EmployerNameOption);

            RuleFor(x => x.EmployerLocation)
                .NotNull()
                    .WithMessage("You must provide an employer location")
                    .WithErrorCode("98")
                .SetValidator(new AddressValidator((long)VacancyRuleSet.EmployerAddress, IsApprenticeshipVacancy))
                .RunCondition(VacancyRuleSet.EmployerAddress)
                .WithRuleId(VacancyRuleSet.EmployerAddress);
        }

        private void ValidateNumberOfPositions()
        {
            RuleFor(x => x.NumberOfPositions)
                .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage($"Enter the number of positions for this {VacancyContext}")
                    .WithErrorCode("10")
                .RunCondition(VacancyRuleSet.NumberOfPositions)
                .WithRuleId(VacancyRuleSet.NumberOfPositions);
        }

        private void ValidateShortDescription()
        {
            RuleFor(x => x.ShortDescription)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage($"Enter a short description of the {VacancyContext}")
                    .WithErrorCode("12")
                .MaximumLength(350)
                    .WithMessage($"Summary of the {VacancyContext} must not exceed {{MaxLength}} characters")
                    .WithErrorCode("13")
                .MinimumLength(50)
                    .WithMessage($"Summary of the {VacancyContext} must be at least {{MinLength}} characters")
                    .WithErrorCode("14")
                .ValidFreeTextCharacters()
                    .WithMessage($"Short description of the {VacancyContext} contains some invalid characters")
                    .WithErrorCode("15")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage($"Short description of the {VacancyContext} must not contain a banned word or phrase.")
                .WithErrorCode("605")
                .RunCondition(VacancyRuleSet.ShortDescription)
                .WithRuleId(VacancyRuleSet.ShortDescription);
        }

        private void ValidateClosingDate()
        {
            RuleFor(x => x.ClosingDate)
                .NotNull()
                    .WithMessage("Enter an application closing date")
                    .WithErrorCode("16")
                .GreaterThan(v => _timeProvider.Now.Date.AddDays(14).AddTicks(-1))
                    .WithMessage("Closing date should be at least 14 days in the future.")
                    .WithErrorCode("18")
                .RunCondition(VacancyRuleSet.ClosingDate)
                .WithRuleId(VacancyRuleSet.ClosingDate);
        }

        private void ValidateStartDate()
        {
            RuleFor(x => x.StartDate)
                .NotNull()
                .WithMessage($"Enter when you expect the {(IsApprenticeshipVacancy? "apprentice" :"trainee")} to start")
                    .WithErrorCode("20")
                .GreaterThan(v => v.ClosingDate)
                .WithMessage("Start date cannot be before the closing date. We advise using a date more than 14 days from now.")
                    .WithErrorCode("22")
                .RunCondition(VacancyRuleSet.StartDate)
                .WithRuleId(VacancyRuleSet.StartDate);
        }

        private void ValidateTrainingProgramme()
        {
            RuleFor(x => x.ProgrammeId)
                .NotEmpty()
                    .WithMessage($"You must select {VacancyContext} training")
                    .WithErrorCode("25")
                .WithRuleId(VacancyRuleSet.TrainingProgramme)
                .RunCondition(VacancyRuleSet.TrainingProgramme);
        }

        private void ValidateRoute()
        {
            RuleFor(x => x.RouteId)
                .NotEmpty()
                .WithMessage($"You must select trainee sector")
                .WithErrorCode("25")
                .WithRuleId(VacancyRuleSet.RouteId)
                .RunCondition(VacancyRuleSet.RouteId);
        }

        private void ValidateApprenticeshipDuration()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.DurationUnit)
                    .NotEmpty()
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .IsInEnum()
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .RunCondition(VacancyRuleSet.Duration)
                    .WithRuleId(VacancyRuleSet.Duration);

                RuleFor(x => x.Wage.Duration)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .GreaterThan(0)
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .Must((vacancy, value) =>
                    {
                        if (vacancy.Wage.DurationUnit == DurationUnit.Month && value < 12)
                        {
                            return false;
                        }

                        return true;
                    })
                    .WithMessage("Expected duration must be at least 12 months")
                    .WithErrorCode("36")
                    .Must((vacancy, value) =>
                    {
                        if (( vacancy.Wage.DurationUnit == DurationUnit.Month && value >= 12 
                             || vacancy.Wage.DurationUnit == DurationUnit.Year && value >= 1)
                            && vacancy.Wage.WeeklyHours.HasValue
                            && vacancy.Wage.WeeklyHours < 30m)
                        {

                            var numberOfMonths = (int) Math.Ceiling(30 / vacancy.Wage.WeeklyHours.GetValueOrDefault() * 12);

                            if (vacancy.Wage.DurationUnit == DurationUnit.Year)
                            {
                                value *= 12;
                            }
                            
                            if (numberOfMonths > value)
                            {
                                return false;    
                            }
                        }

                        return true;
                    })
                    .WithMessage((vacancy, value) =>
                    {
                        int numberOfMonths = (int)Math.Ceiling(30 / vacancy.Wage.WeeklyHours.GetValueOrDefault() * 12);
                        return $"Duration of apprenticeship must be {numberOfMonths} months based on the number of hours per week entered";
                    })
                    .WithErrorCode("36")
                    .RunCondition(VacancyRuleSet.Duration)
                    .WithRuleId(VacancyRuleSet.Duration);
            });
        }

        private void ValidateTraineeshipDuration()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.DurationUnit)
                    .NotEmpty()
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .IsInEnum()
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .RunCondition(VacancyRuleSet.Duration)
                    .WithRuleId(VacancyRuleSet.Duration);

                RuleFor(x => x.Wage.Duration)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .GreaterThan(0)
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .Must((vacancy, value) =>
                    {
                        if (vacancy.Wage.DurationUnit == DurationUnit.Week && value < 6)
                        {
                            return false;
                        }

                        if (vacancy.Wage.DurationUnit == DurationUnit.Month && value <= 1)
                        {
                            return false;
                        }
                        return true;
                    })
                    .WithMessage("Expected duration must be at least 6 weeks")
                    .WithErrorCode("36")
                    .Must((vacancy, value) =>
                    {
                        if (vacancy.Wage.DurationUnit == DurationUnit.Month && value > 12)
                        {
                            return false;
                        }
                        if (vacancy.Wage.DurationUnit == DurationUnit.Week && value > 52)
                        {
                            return false;
                        }

                        return true;
                    })
                    .WithMessage("Expected duration must be 12 months or under")
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
                        .WithMessage("Enter details about the working week")
                        .WithErrorCode("37")
                    .ValidFreeTextCharacters()
                        .WithMessage("Working week details contains some invalid characters")
                        .WithErrorCode("38")
                    .MaximumLength(250)
                        .WithMessage("Details about the working week must not exceed {MaxLength} characters")
                        .WithErrorCode("39")
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Working week details must not contain a banned word or phrase.")
                    .WithErrorCode("606")
                    .RunCondition(VacancyRuleSet.WorkingWeekDescription)
                    .WithRuleId(VacancyRuleSet.WorkingWeekDescription);
            });
        }

        private void ValidateTraineeshipWorkingWeek()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WorkingWeekDescription)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .NotEmpty()
                        .WithMessage("Enter weekly hours on the traineeship")
                        .WithErrorCode("37")
                    .ValidFreeTextCharacters()
                        .WithMessage("Weekly hours on the traineeship contain some invalid characters")
                        .WithErrorCode("38")
                    .MaximumLength(250)
                        .WithMessage("Weekly hours on the traineeship must not exceed {MaxLength} characters")
                        .WithErrorCode("39")
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Weekly hours on the traineeship must not contain a banned word or phrase")
                    .WithErrorCode("606")
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
                        .WithMessage($"Enter how many hours the {(IsApprenticeshipVacancy? "apprentice" :"trainee")} will work each week, including training")
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
                    .WithMessage("Select how much you'd like to pay the apprentice")
                    .WithErrorCode("46")
                .RunCondition(VacancyRuleSet.Wage)
                .WithRuleId(VacancyRuleSet.Wage);

            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WageType)
                    .NotEmpty()
                        .WithMessage("Select how much the apprentice will be paid")
                        .WithErrorCode("46")
                    .IsInEnum()
                        .WithMessage("Select how much the apprentice will be paid")
                        .WithErrorCode("46")
                    .RunCondition(VacancyRuleSet.Wage)
                    .WithRuleId(VacancyRuleSet.Wage);

                RuleFor(x => x.Wage.WageAdditionalInformation)
                    .MaximumLength(250)
                        .WithMessage("Extra information about pay must not exceed {MaxLength} characters")
                        .WithErrorCode("44")
                    .ValidFreeTextCharacters()
                        .WithMessage("Additional pay information contains some invalid characters")
                        .WithErrorCode("45")
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Additional pay information must not contain a banned word or phrase.")
                    .WithErrorCode("607")
                    .RunCondition(VacancyRuleSet.Wage)
                    .WithRuleId(VacancyRuleSet.Wage);
            });
        }

        private void ValidateSkills()
        {
            RuleFor(x => x.Skills)
                .Must(s => s != null && s.Count > 0)
                    .WithMessage("Select the skills and personal qualities you'd like the applicant to have")
                    .WithErrorCode("51")
                .RunCondition(VacancyRuleSet.Skills)
                .WithRuleId(VacancyRuleSet.Skills);

            RuleForEach(x => x.Skills)
                .NotEmpty()
                    .WithMessage("You must include a skill or quality")
                    .WithErrorCode("99")
                .ValidFreeTextCharacters()
                    .WithMessage("Skill contains some invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(30)
                    .WithMessage("Skill or quality must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Skill or quality must not contain a banned word or phrase.")
                .WithErrorCode("608")
                .WithRuleId(VacancyRuleSet.Skills);
        }

        private void ValidateQualifications()
        {
            RuleFor(x => x.Qualifications)
                .Must(q => q != null && q.Count > 0)
                    .WithMessage("You must add a qualification")
                    .WithErrorCode("52")
                .RunCondition(VacancyRuleSet.Qualifications)
                .WithRuleId(VacancyRuleSet.Qualifications);
            RuleForEach(x => x.Qualifications)
                .NotEmpty()
                .SetValidator(new VacancyQualificationsValidator((long)VacancyRuleSet.Qualifications,
                    _qualificationsProvider, _profanityListProvider))
                .RunCondition(VacancyRuleSet.Qualifications)
                .WithRuleId(VacancyRuleSet.Qualifications);
        }

        private void ValidateDescription()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage($"Enter what the {(IsApprenticeshipVacancy? "apprentice" :"trainee")} will be doing")
                    .WithErrorCode("53")
                .MaximumLength(4000)
                    .WithMessage($"What the {VacancyContext} involves must not exceed {{MaxLength}} characters")
                    .WithErrorCode("7")
                .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage($"What the {VacancyContext} involves contains some invalid characters")
                    .WithErrorCode("6")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage($"What the {VacancyContext} involves must not contain a banned word or phrase.")
                .WithErrorCode("609")
                .RunCondition(VacancyRuleSet.Description)
                .WithRuleId(VacancyRuleSet.Description);
        }

        private void ValidateTrainingDescription()
        {
            RuleFor(x => x.TrainingDescription)
                .NotEmpty()
                    .WithMessage(IsApprenticeshipVacancy
                                            ? "Enter the training the apprentice will take and the qualification the apprentice will get"
                                            : "Enter what training you will give the trainee" )
                    .WithErrorCode("54")
                .MaximumLength(4000)
                    .WithMessage(IsApprenticeshipVacancy
                    ? "Training to be provided description must not exceed {MaxLength} characters"
                    : "Training provided must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage(IsApprenticeshipVacancy
                    ? "Training to be provided description contains some invalid characters"
                    : "Training provided contains some invalid characters")
                    .WithErrorCode("6")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage(IsApprenticeshipVacancy
                    ? "Training to be provided description must not contain a banned word or phrase"
                    : "Training provided must not contain a banned word or phrase")
                .WithErrorCode("610")
                .RunCondition(VacancyRuleSet.TrainingDescription)
                .WithRuleId(VacancyRuleSet.TrainingDescription);
        }

        private void ValidateOutcomeDescription()
        {
            RuleFor(x => x.OutcomeDescription)
                .NotEmpty()
                    .WithMessage($"Enter the expected career progression after this {(IsApprenticeshipVacancy? "apprenticeship" : "traineeship")}")
                    .WithErrorCode("55")
                .MaximumLength(4000)
                    .WithMessage("Expected career progression must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage("What is the expected career progression after this apprenticeship description contains some invalid characters")
                    .WithErrorCode("6")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("What is the expected career progression after this apprenticeship description must not contain a banned word or phrase.")
                .WithErrorCode("611")
                .RunCondition(VacancyRuleSet.OutcomeDescription)
                .WithRuleId(VacancyRuleSet.OutcomeDescription);
        }

        private void ValidateApplicationMethod()
        {
            RuleFor(x => x.ApplicationMethod)
                    .NotEmpty()
                        .WithMessage("Select a website for applications")
                        .WithErrorCode("85")
                    .IsInEnum()
                        .WithMessage("Select a website for applications")
                        .WithErrorCode("85")
                    .RunCondition(VacancyRuleSet.ApplicationMethod)
                    .WithRuleId(VacancyRuleSet.ApplicationMethod);

            When(x => x.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship, () =>
            {
                RuleFor(x => x.ApplicationUrl)
                    .Empty()
                        .WithMessage($"Application url must be empty when apply through {(IsApprenticeshipVacancy? "Find an apprenticeship" :"Find a traineeship")} service option is specified")
                        .WithErrorCode("86")
                    .RunCondition(VacancyRuleSet.ApplicationMethod)
                    .WithRuleId(VacancyRuleSet.ApplicationMethod);

                RuleFor(x => x.ApplicationInstructions)
                    .Empty()
                        .WithMessage($"Application process must be empty when apply through {(IsApprenticeshipVacancy? "Find an apprenticeship" :"Find a traineeship")} service option is specified")
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
                    .WithMessage("Enter a link where candidates can apply")
                    .WithErrorCode("85")
                .MaximumLength(200)
                    .WithMessage("The website address must not exceed {MaxLength} characters")
                    .WithErrorCode("84")
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Website address must be a valid link")
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
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Application process must not contain a banned word or phrase.")
                .WithErrorCode("612")
                .RunCondition(VacancyRuleSet.ApplicationMethod)
                .WithRuleId(VacancyRuleSet.ApplicationMethod);
        }

        private void ValidateEmployerContactDetails()
        {
            When(x => x.EmployerContact != null, () =>
            {
                RuleFor(x => x.EmployerContact)
                    .SetValidator(new ContactDetailValidator((long)VacancyRuleSet.EmployerContactDetails,_profanityListProvider))
                .RunCondition(VacancyRuleSet.EmployerContactDetails);
            });
        }

        private void ValidateProviderContactDetails()
        {
            When(x => x.ProviderContact != null, () =>
            {
                RuleFor(x => x.ProviderContact)
                .SetValidator(new ContactDetailValidator((long)VacancyRuleSet.ProviderContactDetails,_profanityListProvider))
                .RunCondition(VacancyRuleSet.ProviderContactDetails);
            });
        }

        private void ValidateThingsToConsider()
        {
            RuleFor(x => x.ThingsToConsider)
                .MaximumLength(4000)
                    .WithMessage("Things to consider must not exceed {MaxLength} characters")
                    .WithErrorCode("75")
                .ValidFreeTextCharacters()
                    .WithMessage("Things to consider contains some invalid characters")
                    .WithErrorCode("76")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Things to consider must not contain a banned word or phrase.")
                .WithErrorCode("613")
                .RunCondition(VacancyRuleSet.ThingsToConsider)
                .WithRuleId(VacancyRuleSet.ThingsToConsider);
        }

        private void ValidateWorkExperience()
        {
            RuleFor(x => x.WorkExperience)
                .NotEmpty()
                    .WithMessage("What work experience will the employer give the trainee?")
                    .WithErrorCode("83")
                .MaximumLength(4000)
                    .WithMessage("What work experience will the employer give the trainee must not exceed {MaxLength} characters")
                    .WithErrorCode("81")
                .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage("What work experience will the employer give the trainee contains some invalid characters")
                    .WithErrorCode("82")
                .ProfanityCheck(_profanityListProvider)
                    .WithMessage("What work experience will the employer give the trainee must not contain a banned word or phrase.")
                    .WithErrorCode("615")
                .RunCondition(VacancyRuleSet.WorkExperience)
                .WithRuleId(VacancyRuleSet.WorkExperience);
        }

        private void ValidateEmployerInformation()
        {
            RuleFor(x => x.EmployerDescription)
                .NotEmpty()
                    .WithMessage("Enter details about the employer")
                    .WithErrorCode("80")
                .MaximumLength(4000)
                    .WithMessage("Information about the employer must not exceed {MaxLength} characters")
                    .WithErrorCode("77")
                .ValidFreeTextCharacters()
                    .WithMessage("Information about the employer contains some invalid characters")
                    .WithErrorCode("78")
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Information about the employer must not contain a banned word or phrase.")
                .WithErrorCode("614")
                .RunCondition(VacancyRuleSet.EmployerDescription)
                .WithRuleId(VacancyRuleSet.EmployerDescription);

            RuleFor(x => x.EmployerWebsiteUrl)
                .MaximumLength(100)
                    .WithMessage("Website address must not exceed {MaxLength} characters")
                    .WithErrorCode("84")
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Website address must be a valid link")
                    .WithErrorCode("82")
                    .When(v => !string.IsNullOrEmpty(v.EmployerWebsiteUrl))
                .RunCondition(VacancyRuleSet.EmployerWebsiteUrl)
                .WithRuleId(VacancyRuleSet.EmployerWebsiteUrl);
        }

        private void ValidateTrainingProvider()
        {
            var trainingProviderValidator = new TrainingProviderValidator((long)VacancyRuleSet.TrainingProvider, _trainingProviderSummaryProvider, _blockedOrganisationRepo);

            RuleFor(x => x.TrainingProvider)
                .NotNull()
                    .WithMessage("You must enter a training provider or UKPRN to continue")
                    .WithErrorCode(ErrorCodes.TrainingProviderUkprnNotEmpty)
                .SetValidator(trainingProviderValidator)
                .RunCondition(VacancyRuleSet.TrainingProvider)
                .WithRuleId(VacancyRuleSet.TrainingProvider);

            RuleFor(x => x)
                .TrainingProviderVacancyMustHaveEmployerPermission(_providerRelationshipService)
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
            When(x => !string.IsNullOrWhiteSpace(x.ProgrammeId), () =>
            {
                RuleFor(x => x)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .TrainingMustExist(_apprenticeshipProgrammesProvider)
                    .TrainingMustBeActiveForStartDate(_apprenticeshipProgrammesProvider)
                .RunCondition(VacancyRuleSet.TrainingExpiryDate)
                .WithRuleId(VacancyRuleSet.TrainingExpiryDate);
            });
        }
    }
}
