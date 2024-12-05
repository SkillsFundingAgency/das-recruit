using System;
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

        private string VacancyContext
        {
            get
            {
                return "apprenticeship";
            }
        }

        private string ApplicantContext
        {
            get
            {
                return "apprentice";
            }
        }

        private void SingleFieldValidations()
        {
        
            ValidateApprenticeshipTitle();
            ValidateApprenticeshipDuration();
            
            ValidateOrganisation();
            ValidateNumberOfPositions();
            ValidateShortDescription();
            ValidateClosingDate();
            ValidateStartDate();

            ValidateTrainingProgramme();
            
            ValidateWorkingWeek();
            ValidateWeeklyHours();
            ValidateWage();

            ValidateSkills();

        
            ValidateQualifications();
            ValidateDescription();
            ValidateAdditionalQuestions();
            

            ValidateHowTheApprenticeWillTrain();
            
            ValidateOutcomeDescription();
            ValidateApplicationMethod();
            ValidateEmployerContactDetails();
            ValidateProviderContactDetails();
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

        private void ValidateApprenticeshipTitle()
        {
            RuleFor(x => x.Title)

                .Cascade(CascadeMode.Stop)

                   .NotEmpty()
                       .WithMessage("Enter a title for this apprenticeship")
                       .WithErrorCode("1")
                       .WithState(_ => VacancyRuleSet.Title)
                   .MaximumLength(100)
                       .WithMessage("Title must not exceed {MaxLength} characters")
                       .WithErrorCode("2")
                       .WithState(_ => VacancyRuleSet.Title)
                   .ValidFreeTextCharacters()
                       .WithMessage("Title contains some invalid characters")
                       .WithErrorCode("3")
                       .WithState(_ => VacancyRuleSet.Title)
                   .Matches(ValidationConstants.ContainsApprenticeOrApprenticeshipRegex)
                       .WithMessage("Enter a title which includes the word 'apprentice' or 'apprenticeship'")
                       .WithErrorCode("200")
                       .WithState(_ => VacancyRuleSet.Title)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Title must not contain a banned word or phrase.")
                .WithErrorCode("601")
                .WithState(_ => VacancyRuleSet.Title)
                .RunCondition(VacancyRuleSet.Title);
        }
        private void ValidateTraineeshipTitle()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Enter a title for this traineeship")
                .WithErrorCode("1")
                .WithState(_ => VacancyRuleSet.Title)
                .MaximumLength(100)
                .WithMessage("Title must not exceed {MaxLength} characters")
                .WithErrorCode("2")
                .WithState(_ => VacancyRuleSet.Title)
                .ValidFreeTextCharacters()
                .WithMessage("Title contains some invalid characters")
                .WithErrorCode("3")
                .WithState(_ => VacancyRuleSet.Title)
                .Matches(ValidationConstants.ContainsTraineeOrTraineeshipRegex)
                .WithMessage("Enter a title which includes the word 'trainee' or 'traineeship'")
                .WithErrorCode("200")
                .WithState(_ => VacancyRuleSet.Title)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Title must not contain a banned word or phrase.")
                .WithErrorCode("601")
                .WithState(_ => VacancyRuleSet.Title)
                .RunCondition(VacancyRuleSet.Title);
        }

        private void ValidateOrganisation()
        {
            RuleFor(x => x.EmployerName)
                .NotEmpty()
                    .WithMessage((vacancy, value) => $"Select the employer name you want on your {(vacancy.OwnerType == OwnerType.Employer ? "advert" : "vacancy")}")
                    .WithErrorCode("4")
                    .WithState(_ => VacancyRuleSet.EmployerName)
                .RunCondition(VacancyRuleSet.EmployerName);

            RuleFor(x => x.LegalEntityName)
                .NotEmpty()
                .WithMessage("You must select one organisation")
                .WithErrorCode("400")
                .WithState(_ => VacancyRuleSet.LegalEntityName)
                .RunCondition(VacancyRuleSet.LegalEntityName);

            When(v => v.EmployerNameOption == EmployerNameOption.TradingName, () =>
                RuleFor(x => x.EmployerName)
                    .NotEmpty()
                        .WithMessage("Enter the trading name")
                        .WithErrorCode("401")
                        .WithState(_ => VacancyRuleSet.TradingName)
                    .MaximumLength(100)
                        .WithMessage("The trading name must not exceed {MaxLength} characters")
                        .WithErrorCode("402")
                        .WithState(_ => VacancyRuleSet.TradingName)
                    .ValidFreeTextCharacters()
                        .WithMessage("The trading name contains some invalid characters")
                        .WithErrorCode("403")
                        .WithState(_ => VacancyRuleSet.TradingName)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Trading name must not contain a banned word or phrase.")
                    .WithErrorCode("602")
                    .WithState(_ => VacancyRuleSet.TradingName)
                    .RunCondition(VacancyRuleSet.TradingName)

            );

            When(v => v.EmployerNameOption == EmployerNameOption.Anonymous, () =>
                RuleFor(x => x.EmployerName)
                    .NotEmpty()
                    .WithMessage("Enter a brief description of what the employer does")
                    .WithErrorCode("405")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .MaximumLength(100)
                    .WithMessage("The description must not be more than {MaxLength} characters")
                    .WithErrorCode("406")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .ValidFreeTextCharacters()
                    .WithMessage("The description contains some invalid characters")
                    .WithErrorCode("407")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Description must not contain a banned word or phrase.")
                    .WithErrorCode("603")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .RunCondition(VacancyRuleSet.EmployerNameOption)
            );

            When(v => v.EmployerNameOption == EmployerNameOption.Anonymous, () =>
                RuleFor(x => x.AnonymousReason)
                    .NotEmpty()
                    .WithMessage((vacancy, value) => $"Enter why you want your {(vacancy.OwnerType == OwnerType.Employer ? "advert" : "vacancy")} to be anonymous")
                    .WithErrorCode("408")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .MaximumLength(200)
                    .WithMessage("The reason must not be more than {MaxLength} characters")
                    .WithErrorCode("409")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .ValidFreeTextCharacters()
                    .WithMessage("The reason contains some invalid characters")
                    .WithErrorCode("410")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Reason must not contain a banned word or phrase.")
                    .WithErrorCode("604")
                    .WithState(_ => VacancyRuleSet.EmployerNameOption)
                    .RunCondition(VacancyRuleSet.EmployerNameOption)

            );

            RuleFor(x => x.EmployerNameOption)
                .NotEmpty()
                    .WithMessage((vacancy, value) => $"Select the employer name you want on your {(vacancy.OwnerType == OwnerType.Employer ? "advert" : "vacancy")}")
                    .WithErrorCode("404")
                .WithState(_ => VacancyRuleSet.EmployerNameOption)
                .RunCondition(VacancyRuleSet.EmployerNameOption);

            When(v => v.EmployerLocationOption is null, () =>
            {
                RuleFor(x => x.EmployerLocation)
                    .NotNull()
                    .WithMessage("You must provide an employer location")
                    .WithErrorCode("98")
                    .WithState(_ => VacancyRuleSet.EmployerAddress)
                    .SetValidator(new AddressValidator((long)VacancyRuleSet.EmployerAddress))
                    .RunCondition(VacancyRuleSet.EmployerAddress);
            });

            // TODO: this will be required for the other validation scenarios
            // When(v => v.EmployerLocationOption == AvailableWhere.OneLocation, () =>
            // {
            //     RuleFor(x => x.EmployerLocations)
            //         .NotNull()
            //         .Must(x => x.Count == 1)
            //         .WithMessage("Select a location")
            //         .WithState(_ => VacancyRuleSet.EmployerAddress)
            //         .RunCondition(VacancyRuleSet.EmployerAddress);
            //     
            //     RuleForEach(x => x.EmployerLocations)
            //         .SetValidator(new AddressValidator((long)VacancyRuleSet.EmployerAddress))
            //         .RunCondition(VacancyRuleSet.EmployerAddress);
            // });
            
            When(v => v.EmployerLocationOption == AvailableWhere.MultipleLocations, () =>
            {
                RuleFor(x => x.EmployerLocations)
                    .NotNull()
                    .Must(x => x.Count is >=2 and <=10)
                    .WithMessage("Select between 2 and 10 locations")
                    .WithState(_ => VacancyRuleSet.EmployerAddress)
                    .RunCondition(VacancyRuleSet.EmployerAddress);
                
                RuleForEach(x => x.EmployerLocations)
                    .SetValidator(new AddressValidator((long)VacancyRuleSet.EmployerAddress))
                    .RunCondition(VacancyRuleSet.EmployerAddress);
            });
        }

        private void ValidateNumberOfPositions()
        {
            RuleFor(x => x.NumberOfPositions)
                .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage($"Enter the number of positions for this {VacancyContext}")
                    .WithErrorCode("10")
                    .WithState(_ => VacancyRuleSet.NumberOfPositions)
                .RunCondition(VacancyRuleSet.NumberOfPositions);
        }

        private void ValidateShortDescription()
        {
            RuleFor(x => x.ShortDescription)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage($"Enter a short description of the {VacancyContext}")
                    .WithErrorCode("12")
                .WithState(_ => VacancyRuleSet.ShortDescription)
                .MaximumLength(350)
                    .WithMessage($"Summary of the {VacancyContext} must not exceed {{MaxLength}} characters")
                    .WithErrorCode("13")
                .WithState(_ => VacancyRuleSet.ShortDescription)
                .MinimumLength(50)
                    .WithMessage($"Summary of the {VacancyContext} must be at least {{MinLength}} characters")
                    .WithErrorCode("14")
                .WithState(_ => VacancyRuleSet.ShortDescription)
                .ValidFreeTextCharacters()
                    .WithMessage($"Short description of the {VacancyContext} contains some invalid characters")
                    .WithErrorCode("15")
                .WithState(_ => VacancyRuleSet.ShortDescription)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage($"Short description of the {VacancyContext} must not contain a banned word or phrase.")
                .WithErrorCode("605")
                .WithState(_ => VacancyRuleSet.ShortDescription)
                .RunCondition(VacancyRuleSet.ShortDescription);
        }


        private void ValidateClosingDate()
        {
            RuleFor(x => x.ClosingDate)
                .NotNull()
                    .WithMessage("Enter an application closing date")
                    .WithErrorCode("16")
                .WithState(_ => VacancyRuleSet.ClosingDate)
                .GreaterThan(v => _timeProvider.Now.Date.AddDays(14).AddTicks(-1))
                    .WithMessage("Closing date should be at least 14 days in the future.")
                    .WithErrorCode("18")
                .WithState(_ => VacancyRuleSet.ClosingDate)
                .RunCondition(VacancyRuleSet.ClosingDate);
        }

        private void ValidateStartDate()
        {
            RuleFor(x => x.StartDate)
                .NotNull()
                .WithMessage($"Enter when you expect the {ApplicantContext} to start")
                    .WithErrorCode("20")
                    .WithState(_ => VacancyRuleSet.StartDate)
                .GreaterThan(v => v.ClosingDate)
                .WithMessage("Start date cannot be before the closing date. We advise using a date more than 14 days from now.")
                    .WithErrorCode("22")
                    .WithState(_ => VacancyRuleSet.StartDate)
                .RunCondition(VacancyRuleSet.StartDate);
        }

        private void ValidateTrainingProgramme()
        {
            RuleFor(x => x.ProgrammeId)
                .NotEmpty()
                    .WithMessage($"You must select {VacancyContext} training")
                    .WithErrorCode("25")
                    .WithState(_ => VacancyRuleSet.TrainingProgramme)
                .RunCondition(VacancyRuleSet.TrainingProgramme);
        }

        private void ValidateApprenticeshipDuration()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.DurationUnit)
                    .NotEmpty()
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
                    .IsInEnum()
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
                    .RunCondition(VacancyRuleSet.Duration);

                RuleFor(x => x.Wage.Duration)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
                    .GreaterThan(0)
                    .WithMessage($"Enter how long the whole {VacancyContext} is, including work and training")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
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
                    .WithState(_ => VacancyRuleSet.Duration)
                    .Must((vacancy, value) =>
                    {
                        if ((vacancy.Wage.DurationUnit == DurationUnit.Month && value >= 12
                             || vacancy.Wage.DurationUnit == DurationUnit.Year && value >= 1)
                            && vacancy.Wage.WeeklyHours.HasValue
                            && vacancy.Wage.WeeklyHours < 30m)
                        {

                            var numberOfMonths = (int)Math.Ceiling(30 / vacancy.Wage.WeeklyHours.GetValueOrDefault() * 12);

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
                    .WithState(_ => VacancyRuleSet.Duration)
                    .RunCondition(VacancyRuleSet.Duration);
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
                    .WithState(_ => VacancyRuleSet.Duration)
                    .IsInEnum()
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
                    .RunCondition(VacancyRuleSet.Duration);

                RuleFor(x => x.Wage.Duration)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
                    .GreaterThan(0)
                    .WithMessage($"Enter {VacancyContext} duration")
                    .WithErrorCode("34")
                    .WithState(_ => VacancyRuleSet.Duration)
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
                    .WithState(_ => VacancyRuleSet.Duration)
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
                    .WithState(_ => VacancyRuleSet.Duration)
                    .RunCondition(VacancyRuleSet.Duration);
            });
        }
        private void ValidateWorkingWeek()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WorkingWeekDescription)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("Enter details about the working week")
                        .WithErrorCode("37")
                    .WithState(_ => VacancyRuleSet.WorkingWeekDescription)
                    .ValidFreeTextCharacters()
                        .WithMessage("Working week details contains some invalid characters")
                        .WithErrorCode("38")
                    .WithState(_ => VacancyRuleSet.WorkingWeekDescription)
                    .MaximumLength(250)
                        .WithMessage("Details about the working week must not exceed {MaxLength} characters")
                        .WithErrorCode("39")
                    .WithState(_ => VacancyRuleSet.WorkingWeekDescription)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Working week details must not contain a banned word or phrase.")
                    .WithErrorCode("606")
                    .WithState(_ => VacancyRuleSet.WorkingWeekDescription)
                    .RunCondition(VacancyRuleSet.WorkingWeekDescription);
            });
        }


        private void ValidateWeeklyHours()
        {
            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WeeklyHours)
                    .NotEmpty()
                        .WithMessage($"Enter how many hours the {ApplicantContext} will work each week, including training")
                        .WithErrorCode("40")
                    .WithState(_ => VacancyRuleSet.WeeklyHours)
                    .GreaterThanOrEqualTo(16)
                        .WithMessage("The total hours a week must be at least {ComparisonValue}")
                        .WithErrorCode("42")
                    .WithState(_ => VacancyRuleSet.WeeklyHours)
                    .LessThanOrEqualTo(48)
                        .WithMessage("The total hours a week must not be more than {ComparisonValue}")
                        .WithErrorCode("43")
                    .WithState(_ => VacancyRuleSet.WeeklyHours)
                    .RunCondition(VacancyRuleSet.WeeklyHours);
            });
        }

        private void ValidateWage()
        {
            RuleFor(x => x.Wage)
                .NotNull()
                    .WithMessage("Select how much you'd like to pay the apprentice")
                    .WithErrorCode("46")
                .WithState(_ => VacancyRuleSet.Wage)
                .RunCondition(VacancyRuleSet.Wage);

            When(x => x.Wage != null, () =>
            {
                RuleFor(x => x.Wage.WageType)
                    .NotEmpty()
                        .WithMessage("Select how much the apprentice will be paid")
                        .WithErrorCode("46")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .IsInEnum()
                        .WithMessage("Select how much the apprentice will be paid")
                        .WithErrorCode("46")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .RunCondition(VacancyRuleSet.Wage);

                RuleFor(x => x.Wage.WageAdditionalInformation)
                    .MaximumLength(250)
                        .WithMessage("Information about pay must be {MaxLength} characters or less")
                        .WithErrorCode("44")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .ValidFreeTextCharacters()
                        .WithMessage("Information about pay contains some invalid characters")
                        .WithErrorCode("45")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Information about pay must not contain a banned word or phrase")
                    .WithErrorCode("607")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .RunCondition(VacancyRuleSet.Wage);
               
                RuleFor(x => x.Wage.CompanyBenefitsInformation)
                    .MaximumLength(250)
                    .WithMessage("Company benefits must be {MaxLength} characters or less")
                    .WithErrorCode("44")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .ValidFreeTextCharacters()
                    .WithMessage("Company benefits contains some invalid characters")
                    .WithErrorCode("45")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Company benefits must not contain a banned word or phrase")
                    .WithErrorCode("607")
                    .WithState(_ => VacancyRuleSet.Wage)
                    .RunCondition(VacancyRuleSet.Wage);
                
            });
        }

        private void ValidateSkills()
        {
            RuleFor(x => x.Skills)
                .Must(s => s != null && s.Count > 0)
                    .WithMessage("Select the skills and personal qualities you'd like the applicant to have")
                    .WithErrorCode("51")
                .WithState(_ => VacancyRuleSet.Skills)
                .RunCondition(VacancyRuleSet.Skills);

            RuleForEach(x => x.Skills)
                .NotEmpty()
                    .WithMessage("You must include a skill or quality")
                    .WithErrorCode("99")
                .WithState(_ => VacancyRuleSet.Skills)
                .ValidFreeTextCharacters()
                    .WithMessage("Skill contains some invalid characters")
                    .WithErrorCode("6")
                .WithState(_ => VacancyRuleSet.Skills)
                .MaximumLength(30)
                    .WithMessage("Skill or quality must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .WithState(_ => VacancyRuleSet.Skills)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Skill or quality must not contain a banned word or phrase.")
                .WithErrorCode("608")
                .WithState(_ => VacancyRuleSet.Skills);
        }

        private void ValidateQualifications()
        {
            When(c => c.HasOptedToAddQualifications is true, ValidateListOfQualifications);    
        }

        private void ValidateListOfQualifications()
        {
            RuleFor(x => x.Qualifications)
                .Must(q => q != null && q.Count > 0)
                .WithMessage("You must add a qualification")
                .WithErrorCode("52")
                .WithState(_ => VacancyRuleSet.Qualifications)
                .RunCondition(VacancyRuleSet.Qualifications);
            RuleForEach(x => x.Qualifications)
                .NotEmpty()
                .SetValidator(new VacancyQualificationsValidator((long)VacancyRuleSet.Qualifications,
                    _qualificationsProvider, _profanityListProvider))
                .RunCondition(VacancyRuleSet.Qualifications)
                .WithState(_ => VacancyRuleSet.Qualifications);
        }

        private void ValidateDescription()
        {
            const string messageText = "will do at work";

            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage($"Enter what the {ApplicantContext} {messageText}")
                    .WithErrorCode("53")
                .WithState(_ => VacancyRuleSet.Description)
                .MaximumLength(4000)
                    .WithMessage($"What the {ApplicantContext} {messageText} must not exceed {{MaxLength}} characters")
                    .WithErrorCode("7")
                .WithState(_ => VacancyRuleSet.Description)
                .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage($"What the {ApplicantContext} {messageText} contains some invalid characters")
                    .WithErrorCode("6")
                .WithState(_ => VacancyRuleSet.Description)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage($"What the {ApplicantContext} {messageText} must not contain a banned word or phrase")
                .WithErrorCode("609")
                .WithState(_ => VacancyRuleSet.Description)
                .RunCondition(VacancyRuleSet.Description);
        }

        private void ValidateAdditionalQuestions()
        {
            When(x => !string.IsNullOrEmpty(x.AdditionalQuestion1), () =>
            {
                RuleFor(x => x.AdditionalQuestion1)
                    .MaximumLength(250)
                    .WithMessage("Question 1 must not exceed 250 characters")
                    .WithErrorCode("321")
                    .WithState(_ => VacancyRuleSet.AdditionalQuestion1)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Questions must not contain a restricted word")
                    .WithErrorCode("322")
                    .WithState(_ => VacancyRuleSet.AdditionalQuestion1)
                    .Must(s => !string.IsNullOrEmpty(s) && s.Contains('?'))
                    .WithMessage("Question 1 must include a question mark ('?')")
                    .WithErrorCode("340")
                    .WithState(_ => VacancyRuleSet.AdditionalQuestion1)
                    .RunCondition(VacancyRuleSet.AdditionalQuestion1);
            });


            When(x => !string.IsNullOrEmpty(x.AdditionalQuestion2), () =>
            {
                RuleFor(x => x.AdditionalQuestion2)
                    .MaximumLength(250)
                    .WithMessage("Question 2 must not exceed 250 characters")
                    .WithErrorCode("331")
                    .WithState(_ => VacancyRuleSet.AdditionalQuestion2)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Questions must not contain a restricted word")
                    .WithErrorCode("332")
                    .WithState(_ => VacancyRuleSet.AdditionalQuestion2)
                    .Must(s => !string.IsNullOrEmpty(s) && s.Contains('?'))
                    .WithMessage("Question 2 must include a question mark ('?')")
                    .WithErrorCode("340")
                    .WithState(_ => VacancyRuleSet.AdditionalQuestion2)
                    .RunCondition(VacancyRuleSet.AdditionalQuestion2);
            });

        }
        
        private void ValidateHowTheApprenticeWillTrain()
        {
            When(x => !string.IsNullOrEmpty(x.TrainingDescription), () =>
            {
                RuleFor(x => x.TrainingDescription)
                    .MaximumLength(4000)
                    .WithMessage("The apprentice’s training schedule must not exceed 4000 characters")
                    .WithErrorCode("321")
                    .WithState(_ => VacancyRuleSet.TrainingDescription)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("The apprentice’s training schedule must not contain a restricted word")
                    .WithErrorCode("322")
                    .WithState(_ => VacancyRuleSet.TrainingDescription)
                    .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage("The apprentice’s training schedule contains some invalid characters")
                    .WithErrorCode("346")
                    .WithState(_ => VacancyRuleSet.TrainingDescription)
                    .RunCondition(VacancyRuleSet.TrainingDescription);
            });


            When(x => !string.IsNullOrEmpty(x.AdditionalTrainingDescription), () =>
            {
                RuleFor(x => x.AdditionalTrainingDescription)
                    .MaximumLength(4000)
                    .WithMessage("Any additional training information must not exceed 4000 characters")
                    .WithErrorCode("341")
                    .WithState(_ => VacancyRuleSet.AdditionalTrainingDescription)
                    .ProfanityCheck(_profanityListProvider)
                    .WithMessage("Any additional training information must not contain a restricted word")
                    .WithErrorCode("342")
                    .WithState(_ => VacancyRuleSet.AdditionalTrainingDescription)
                    .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage("Any additional training information contains some invalid characters")
                    .WithErrorCode("344")
                    .WithState(_ => VacancyRuleSet.AdditionalTrainingDescription)
                    .RunCondition(VacancyRuleSet.AdditionalTrainingDescription);
            });
        }

        private void ValidateOutcomeDescription()
        {
            RuleFor(x => x.OutcomeDescription)
                .NotEmpty()
                    .WithMessage($"Enter the expected career progression after this {VacancyContext}")
                    .WithErrorCode("55")
                .WithState(_ => VacancyRuleSet.OutcomeDescription)
                .MaximumLength(4000)
                    .WithMessage("Expected career progression must not exceed {MaxLength} characters")
                    .WithErrorCode("7")
                .WithState(_ => VacancyRuleSet.OutcomeDescription)
                .ValidHtmlCharacters(_htmlSanitizerService)
                    .WithMessage("What is the expected career progression after this apprenticeship description contains some invalid characters")
                    .WithErrorCode("6")
                .WithState(_ => VacancyRuleSet.OutcomeDescription)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("What is the expected career progression after this apprenticeship description must not contain a banned word or phrase.")
                .WithErrorCode("611")
                .WithState(_ => VacancyRuleSet.OutcomeDescription)
                .RunCondition(VacancyRuleSet.OutcomeDescription);
        }

        private void ValidateApplicationMethod()
        {
            RuleFor(x => x.ApplicationMethod)
                    .NotEmpty()
                        .WithMessage("Select a website for applications")
                        .WithErrorCode("85")
                    .WithState(_ => VacancyRuleSet.ApplicationMethod)
                    .IsInEnum()
                        .WithMessage("Select a website for applications")
                        .WithErrorCode("85")
                    .WithState(_ => VacancyRuleSet.ApplicationMethod)
                    .RunCondition(VacancyRuleSet.ApplicationMethod);

            When(x => x.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship, () =>
            {
                RuleFor(x => x.ApplicationUrl)
                    .Empty()
                        .WithMessage($"Application url must be empty when apply through Find an apprenticeship service option is specified")
                        .WithErrorCode("86")
                    .WithState(_ => VacancyRuleSet.ApplicationMethod)
                    .RunCondition(VacancyRuleSet.ApplicationMethod);

                RuleFor(x => x.ApplicationInstructions)
                    .Empty()
                        .WithMessage($"Application process must be empty when apply through Find an apprenticeship service option is specified")
                        .WithErrorCode("89")
                    .WithState(_ => VacancyRuleSet.ApplicationMethod)
                    .RunCondition(VacancyRuleSet.ApplicationMethod);
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
                .WithState(_ => VacancyRuleSet.ApplicationMethod)
                .MaximumLength(2000)
                    .WithMessage("The website address must not exceed {MaxLength} characters")
                    .WithErrorCode("84")
                .WithState(_ => VacancyRuleSet.ApplicationMethod)
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Website address must be a valid link")
                    .WithErrorCode("86")
                .WithState(_ => VacancyRuleSet.ApplicationMethod)
                .RunCondition(VacancyRuleSet.ApplicationMethod);
        }

        private void ValidateApplicationInstructions()
        {
            RuleFor(x => x.ApplicationInstructions)
                .MaximumLength(500)
                    .WithMessage("Application process must not exceed {MaxLength} characters")
                    .WithErrorCode("88")
                .WithState(_ => VacancyRuleSet.ApplicationMethod)
                .ValidFreeTextCharacters()
                    .WithMessage("Application process contains some invalid characters")
                    .WithErrorCode("89")
                .WithState(_ => VacancyRuleSet.ApplicationMethod)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Application process must not contain a banned word or phrase.")
                .WithErrorCode("612")
                .WithState(_ => VacancyRuleSet.ApplicationMethod)
                .RunCondition(VacancyRuleSet.ApplicationMethod);
        }

        private void ValidateEmployerContactDetails()
        {
            When(x => x.EmployerContact != null, () =>
            {
                RuleFor(x => x.EmployerContact)
                    .SetValidator(new ContactDetailValidator((long)VacancyRuleSet.EmployerContactDetails, _profanityListProvider))
                .RunCondition(VacancyRuleSet.EmployerContactDetails);
            });
        }

        private void ValidateProviderContactDetails()
        {
            When(x => x.ProviderContact != null, () =>
            {
                RuleFor(x => x.ProviderContact)
                .SetValidator(new ContactDetailValidator((long)VacancyRuleSet.ProviderContactDetails, _profanityListProvider))
                .RunCondition(VacancyRuleSet.ProviderContactDetails);
            });
        }

        private void ValidateThingsToConsider()
        {
            RuleFor(x => x.ThingsToConsider)
                .MaximumLength(4000)
                    .WithMessage("Things to consider must not exceed {MaxLength} characters")
                    .WithErrorCode("75")
                .WithState(_ => VacancyRuleSet.ThingsToConsider)
                .ValidFreeTextCharacters()
                    .WithMessage("Things to consider contains some invalid characters")
                    .WithErrorCode("76")
                .WithState(_ => VacancyRuleSet.ThingsToConsider)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Things to consider must not contain a banned word or phrase.")
                .WithErrorCode("613")
                .WithState(_ => VacancyRuleSet.ThingsToConsider)
                .RunCondition(VacancyRuleSet.ThingsToConsider);
        }

        private void ValidateEmployerInformation()
        {
            RuleFor(x => x.EmployerDescription)
                .NotEmpty()
                    .WithMessage("Enter details about the employer")
                    .WithErrorCode("80")
                .WithState(_ => VacancyRuleSet.EmployerDescription)
                .MaximumLength(4000)
                    .WithMessage("Information about the employer must not exceed {MaxLength} characters")
                    .WithErrorCode("77")
                .WithState(_ => VacancyRuleSet.EmployerDescription)
                .ValidFreeTextCharacters()
                    .WithMessage("Information about the employer contains some invalid characters")
                    .WithErrorCode("78")
                .WithState(_ => VacancyRuleSet.EmployerDescription)
                .ProfanityCheck(_profanityListProvider)
                .WithMessage("Information about the employer must not contain a banned word or phrase.")
                .WithErrorCode("614")
                .WithState(_ => VacancyRuleSet.EmployerDescription)
                .RunCondition(VacancyRuleSet.EmployerDescription);

            RuleFor(x => x.EmployerWebsiteUrl)
                .MaximumLength(100)
                    .WithMessage("Website address must not exceed {MaxLength} characters")
                    .WithErrorCode("84")
                .WithState(_ => VacancyRuleSet.EmployerWebsiteUrl)
                .Must(FluentExtensions.BeValidWebUrl)
                    .WithMessage("Website address must be a valid link")
                    .WithErrorCode("82")
                .WithState(_ => VacancyRuleSet.EmployerWebsiteUrl)
                    .When(v => !string.IsNullOrEmpty(v.EmployerWebsiteUrl))
                .RunCondition(VacancyRuleSet.EmployerWebsiteUrl);
        }

        private void ValidateTrainingProvider()
        {
            var trainingProviderValidator = new TrainingProviderValidator((long)VacancyRuleSet.TrainingProvider, _trainingProviderSummaryProvider, _blockedOrganisationRepo);

            RuleFor(x => x.TrainingProvider)
                .NotNull()
                    .WithMessage("You must enter a training provider or UKPRN to continue")
                    .WithErrorCode(ErrorCodes.TrainingProviderUkprnNotEmpty)
                .WithState(_ => VacancyRuleSet.TrainingProvider)
                .SetValidator(trainingProviderValidator)
                .RunCondition(VacancyRuleSet.TrainingProvider)
                .WithState(_ => VacancyRuleSet.TrainingProvider);

            RuleFor(x => x)
                .TrainingProviderVacancyMustHaveEmployerPermission(_providerRelationshipService)
                .RunCondition(VacancyRuleSet.TrainingProvider);
        }

        private void ValidateStartDateClosingDate()
        {
            When(x => x.StartDate.HasValue && x.ClosingDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .ClosingDateMustBeLessThanStartDate()
                    .RunCondition(VacancyRuleSet.StartDateEndDate);
            });
        }

        private void MinimumWageValidation()
        {
            When(x => x.Wage != null && x.Wage.WageType == WageType.FixedWage, () =>
            {
                RuleFor(x => x)
                    .FixedWageMustBeGreaterThanApprenticeshipMinimumWage(_minimumWageService)
                .RunCondition(VacancyRuleSet.MinimumWage);
            });
        }

        private void TrainingExpiryDateValidation()
        {
            When(x => !string.IsNullOrWhiteSpace(x.ProgrammeId), () =>
            {
                RuleFor(x => x)
                    .Cascade(CascadeMode.Stop)
                    .TrainingMustExist(_apprenticeshipProgrammesProvider)
                    .TrainingMustBeActiveForCurrentDate(_apprenticeshipProgrammesProvider, _timeProvider)
                    .TrainingMustBeActiveForStartDate(_apprenticeshipProgrammesProvider)
                .RunCondition(VacancyRuleSet.TrainingExpiryDate);
            });
        }
    }
}
