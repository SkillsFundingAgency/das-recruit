using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentValidation;
using FluentValidation.Results;
using SFA.DAS.VacancyServices.Wage;
using WageType = SFA.DAS.VacancyServices.Wage.WageType;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators
{
    internal static class VacancyFluentValidationExtensions
    {
        internal static IRuleBuilderInitial<Vacancy, Vacancy> ClosingDateMustBeLessThanStartDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder)
        {
            return (IRuleBuilderInitial<Vacancy, Vacancy>)ruleBuilder.Custom((vacancy, context) =>
            {
                if (vacancy.StartDate.Value.Date <= vacancy.ClosingDate.Value.Date)
                {
                    var failure = new ValidationFailure(string.Empty, "The possible start date should be after the closing date")
                    {
                        ErrorCode = "24",
                        CustomState = VacancyRuleSet.StartDateEndDate
                    };
                    context.AddFailure(failure);
                }
            });
        }

        internal static IRuleBuilderInitial<Vacancy, Vacancy> FixedWageMustBeGreaterThanApprenticeshipMinimumWage(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, IMinimumWageProvider minimumWageService)
        {
            return (IRuleBuilderInitial<Vacancy, Vacancy>)ruleBuilder.Custom((vacancy, context) =>
            {
                var wagePeriod = minimumWageService.GetWagePeriod(vacancy.StartDate.Value);

                if (vacancy.Wage.FixedWageYearlyAmount == null || vacancy.Wage.FixedWageYearlyAmount / 52 / vacancy.Wage.WeeklyHours < wagePeriod.ApprenticeshipMinimumWage)
                {
                    var minimumYearlyWageForApprentices = WagePresenter.GetDisplayText(WageType.ApprenticeshipMinimum, WageUnit.Annually, new WageDetails
                    {
                        HoursPerWeek = vacancy.Wage.WeeklyHours,
                        StartDate = vacancy.StartDate.Value
                    }).AsWholeMoneyAmount();

                    var errorMessage = (vacancy.Status == VacancyStatus.Live) ?
                        $"National Minimum Wage is changing from {wagePeriod.ValidFrom:d MMM yyyy}. So the fixed wage you entered before will no longer be valid. Change the date to before {wagePeriod.ValidFrom:d MMM yyyy} or to change the wage, create a new vacancy." :
                        $"Yearly wage must be at least {minimumYearlyWageForApprentices}";

                    var failure = new ValidationFailure(string.Empty, errorMessage)
                    {
                        ErrorCode = "49",
                        CustomState = VacancyRuleSet.MinimumWage,
                        PropertyName = $"{nameof(Wage)}.{nameof(Wage.FixedWageYearlyAmount)}"  
                    };
                    context.AddFailure(failure);
                }
            });
        }

        internal static IRuleBuilderInitial<Vacancy, Vacancy> TrainingMustExist(
            this IRuleBuilder<Vacancy, Vacancy> ruleBuilder,
            IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider)
        {
            return (IRuleBuilderInitial<Vacancy, Vacancy>)ruleBuilder.CustomAsync(async (vacancy, context, cancellationToken) =>
            {
                var programme =
                    await apprenticeshipProgrammeProvider.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

                if (programme == null)
                {
                    var message = $"Training programme {vacancy.ProgrammeId} does not exist.";
                    var failure = new ValidationFailure(string.Empty, message)
                    {
                        ErrorCode = ErrorCodes.TrainingNotExist,
                        CustomState = VacancyRuleSet.TrainingProgramme,
                        PropertyName = nameof(Vacancy.ProgrammeId)
                    };
                    context.AddFailure(failure);
                }
            });
        }
        internal static IRuleBuilderInitial<Vacancy, Vacancy> TrainingMustBeActiveForCurrentDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider, ITimeProvider timeProvider)
        {
            return (IRuleBuilderInitial<Vacancy, Vacancy>)ruleBuilder.CustomAsync(async (vacancy, context, cancellationToken) =>
            {
                var allProgrammes = await apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync();

                var matchingProgramme = allProgrammes.SingleOrDefault(x => x.Id.Equals(vacancy.ProgrammeId, StringComparison.InvariantCultureIgnoreCase));

                if (matchingProgramme == null || (matchingProgramme.LastDateStarts != null && matchingProgramme.LastDateStarts < timeProvider.Now.Date) ||
                    (matchingProgramme.EffectiveTo != null && matchingProgramme.EffectiveTo < timeProvider.Now.Date))
                {
                    var failure = new ValidationFailure(string.Empty, "The training course you have selected is no longer available. You can select a new course or create a new advert.")
                    {
                        ErrorCode = ErrorCodes.TrainingNotExist,
                        CustomState = VacancyRuleSet.TrainingProgramme,
                        PropertyName = nameof(Vacancy.ProgrammeId),
                    };
                    context.AddFailure(failure);
                    
                }
            });
        }

        internal static IRuleBuilderInitial<Vacancy, Vacancy> TrainingMustBeActiveForStartDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider)
        {
            return (IRuleBuilderInitial<Vacancy, Vacancy>)ruleBuilder.CustomAsync(async (vacancy, context, cancellationToken) =>
            {
                if (!vacancy.StartDate.HasValue)
                {
                    var message = $"The start date must have a value.";
                    var failure = new ValidationFailure(string.Empty, message)
                    {
                        ErrorCode = ErrorCodes.TrainingExpiryDate,
                        CustomState = VacancyRuleSet.TrainingExpiryDate,
                        PropertyName = nameof(Vacancy.StartDate)
                    };
                    context.AddFailure(failure);
                }
                
                var allProgrammes = await apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync();

                var matchingProgramme = allProgrammes.SingleOrDefault(x => x.Id.Equals(vacancy.ProgrammeId, StringComparison.InvariantCultureIgnoreCase));

                if ((matchingProgramme.LastDateStarts != null && matchingProgramme.LastDateStarts < vacancy.StartDate) ||
                    (matchingProgramme.EffectiveTo !=null && matchingProgramme.EffectiveTo < vacancy.StartDate))
                {
                    var dateToDisplay = matchingProgramme.LastDateStarts.HasValue 
                        ? matchingProgramme.LastDateStarts.Value.AsGdsDate()
                        : matchingProgramme.EffectiveTo.Value.AsGdsDate();
                    
                    var message = $"Start date must be on or before {dateToDisplay} as this is the last day for new starters for the training course you have selected. If you don’t want to change the start date, you can change the training course.";
                    var failure = new ValidationFailure(string.Empty, message)
                    {
                        ErrorCode = ErrorCodes.TrainingExpiryDate,
                        CustomState = VacancyRuleSet.TrainingExpiryDate,
                        PropertyName = nameof(Vacancy.StartDate)
                    };
                    context.AddFailure(failure);
                }
            });
        }

        internal static IRuleBuilderInitial<TrainingProvider, TrainingProvider> TrainingProviderMustExistInRoatp(this IRuleBuilder<TrainingProvider, TrainingProvider> ruleBuilder, ITrainingProviderSummaryProvider trainingProviderSummaryProvider)
        {
            return (IRuleBuilderInitial<TrainingProvider, TrainingProvider>)ruleBuilder.CustomAsync(async (trainingProvider, context, cancellationToken) =>
            {
                if (trainingProvider.Ukprn.HasValue && (await trainingProviderSummaryProvider.GetAsync(trainingProvider.Ukprn.Value)) != null)
                {
                    return;
                }

                var failure = new ValidationFailure(nameof(Vacancy.TrainingProvider), "The UKPRN is not valid or the associated provider is not active")
                {
                    ErrorCode = ErrorCodes.TrainingProviderMustExist,
                    CustomState = VacancyRuleSet.TrainingProvider
                };
                context.AddFailure(failure);
            });
        }

        internal static IRuleBuilderInitial<TrainingProvider, TrainingProvider> TrainingProviderMustBeMainOrEmployerProfile(this IRuleBuilder<TrainingProvider, TrainingProvider> ruleBuilder, ITrainingProviderSummaryProvider trainingProviderSummaryProvider)
        {
            return (IRuleBuilderInitial<TrainingProvider, TrainingProvider>)ruleBuilder.CustomAsync(async (trainingProvider, context, _) =>
            {
                if (trainingProvider.Ukprn.HasValue && await trainingProviderSummaryProvider.IsTrainingProviderMainOrEmployerProfile(trainingProvider.Ukprn.Value))
                {
                    return;
                }

                var failure = new ValidationFailure(nameof(Vacancy.TrainingProvider), "UKPRN of a training provider must be registered to deliver apprenticeship training")
                {
                    ErrorCode = ErrorCodes.TrainingProviderMustBeMainOrEmployerProfile,
                    CustomState = VacancyRuleSet.TrainingProvider
                };
                context.AddFailure(failure);
            });
        }

        internal static IRuleBuilderInitial<TrainingProvider, TrainingProvider> TrainingProviderMustNotBeBlocked(this IRuleBuilder<TrainingProvider, TrainingProvider> ruleBuilder, IBlockedOrganisationQuery blockedOrganisationRep)
        {
            return (IRuleBuilderInitial<TrainingProvider, TrainingProvider>)ruleBuilder.CustomAsync(async (trainingProvider, context, cancellationToken) =>
            {
                if (trainingProvider.Ukprn != null)
                {
                    var organisation = await blockedOrganisationRep.GetByOrganisationIdAsync(trainingProvider.Ukprn.Value.ToString());
                    if(organisation == null || organisation.BlockedStatus != BlockedStatus.Blocked)
                        return;
                }
                
                var failure = new ValidationFailure(nameof(Vacancy.TrainingProvider), $"{trainingProvider.Name} can no longer be used as a training provider")
                {
                    ErrorCode = ErrorCodes.TrainingProviderMustNotBeBlocked,
                    CustomState = VacancyRuleSet.TrainingProvider
                };
                context.AddFailure(failure);
            });
        }

        internal static IRuleBuilderInitial<Vacancy, Vacancy> TrainingProviderVacancyMustHaveEmployerPermission(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, IProviderRelationshipsService providerRelationshipService)
        {
            return (IRuleBuilderInitial<Vacancy, Vacancy>)ruleBuilder.CustomAsync(async (vacancy, context, cancellationToken) =>
            {
                if (vacancy.OwnerType != OwnerType.Provider)
                    return;

                var hasPermission = await providerRelationshipService.HasProviderGotEmployersPermissionAsync(vacancy.TrainingProvider.Ukprn.Value, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.Recruitment);

                if (hasPermission)
                    return;
                
                var failure = new ValidationFailure(string.Empty, "Training provider does not have permission to create vacancies for this employer")
                {
                    ErrorCode = ErrorCodes.TrainingProviderMustHaveEmployerPermission,
                    CustomState = VacancyRuleSet.TrainingProvider
                };
                context.AddFailure(failure);
            });
        }


        internal static IRuleBuilderOptions<Vacancy, T> RunCondition<T>(this IRuleBuilderOptions<Vacancy, T> context, VacancyRuleSet condition)
        {
            return context.Configure(c=>c.ApplyCondition(x => x.CanRunValidator(condition)));
        }
        
        internal static IRuleBuilderInitial<Vacancy, T> RunCondition<T>(this IRuleBuilderInitial<Vacancy, T> context, VacancyRuleSet condition)
        {
            return context.Configure(c=>c.ApplyCondition(x => x.CanRunValidator(condition)));
        }

        private static bool CanRunValidator<T>(this ValidationContext<T> context, VacancyRuleSet validationToCheck)
        {
            var validationsToRun = (VacancyRuleSet)context.RootContextData[ValidationConstants.ValidationsRulesKey];

            return (validationsToRun & validationToCheck) > 0;
        }
    }
}