using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using SFA.DAS.VacancyServices.Wage;
using WageType = SFA.DAS.VacancyServices.Wage.WageType;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators.VacancyValidators
{
    internal static class VacancyFluentValidationExtensions
    {
        internal static IRuleBuilderInitial<Vacancy, Vacancy> ClosingDateMustBeLessThanStartDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder)
        {
            return ruleBuilder.Custom((vacancy, context) =>
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
            return ruleBuilder.Custom((vacancy, context) =>
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

        internal static IRuleBuilderInitial<Vacancy, Vacancy> TrainingMustBeActiveForStartDate(this IRuleBuilder<Vacancy, Vacancy> ruleBuilder, IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider)
        {
            return ruleBuilder.CustomAsync(async (vacancy, context, cancellationToken) =>
            {
                var allProgrammes = await apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync();

                var matchingProgramme = allProgrammes.SingleOrDefault(x => x.Id.Equals(vacancy.ProgrammeId, StringComparison.InvariantCultureIgnoreCase));

                if (matchingProgramme.EffectiveTo != null && matchingProgramme.EffectiveTo < vacancy.StartDate)
                {
                    var message = $"The start date must be before {matchingProgramme.EffectiveTo.Value.AsGdsDate()} when the apprenticeship training closes to new starters.";
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
            return ruleBuilder.CustomAsync(async (trainingProvider, context, cancellationToken) =>
            {
                if (trainingProvider.Ukprn.HasValue && 
                    (await trainingProviderSummaryProvider.GetAsync(trainingProvider.Ukprn.Value)) != null)
                return;

                var failure = new ValidationFailure(nameof(Vacancy.TrainingProvider), "The UKPRN is not valid or the associated provider is not active")
                {
                    ErrorCode = ErrorCodes.TrainingProviderMustExist,
                    CustomState = VacancyRuleSet.TrainingProvider
                };
                context.AddFailure(failure);
            });
        }

        internal static IRuleBuilderInitial<TrainingProvider, TrainingProvider> TrainingProviderMustNotBeBlocked(this IRuleBuilder<TrainingProvider, TrainingProvider> ruleBuilder, IBlockedOrganisationQuery blockedOrganisationRep)
        {
            return ruleBuilder.CustomAsync(async (trainingProvider, context, cancellationToken) =>
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
            return ruleBuilder.CustomAsync(async (vacancy, context, cancellationToken) =>
            {
                if (vacancy.OwnerType != OwnerType.Provider)
                    return;

                var hasPermission = await providerRelationshipService.HasProviderGotEmployersPermissionAsync(vacancy.TrainingProvider.Ukprn.Value, vacancy.EmployerAccountId, vacancy.LegalEntityId);

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

        internal static IRuleBuilderOptions<Vacancy, TElement> RunCondition<TElement>(this IConfigurable<PropertyRule, IRuleBuilderOptions<Vacancy, TElement>> ruleBuilder, VacancyRuleSet condition)
        {
            return ruleBuilder.Configure(c => c.ApplyCondition(context => context.CanRunValidator(condition), ApplyConditionTo.AllValidators));
        }

        internal static IRuleBuilderInitial<Vacancy, TElement> RunCondition<TElement>(this IConfigurable<PropertyRule, IRuleBuilderInitial<Vacancy, TElement>> ruleBuilder, VacancyRuleSet condition)
        {
            return ruleBuilder.Configure(c => c.ApplyCondition(context => context.CanRunValidator(condition), ApplyConditionTo.AllValidators));
        }

        internal static IRuleBuilderOptions<Vacancy, TElement> WithRuleId<TElement>(this IConfigurable<PropertyRule, IRuleBuilderOptions<Vacancy, TElement>> ruleBuilder, VacancyRuleSet ruleId)
        {
            return ruleBuilder.Configure(c =>
            {
                // Set rule type in context so it can be returned in error object
                foreach (var validator in c.Validators)
                {
                    validator.CustomStateProvider = s => ruleId;
                }
            });
        }

        internal static IRuleBuilderInitial<Vacancy, TElement> WithRuleId<TElement>(this IConfigurable<PropertyRule, IRuleBuilderInitial<Vacancy, TElement>> ruleBuilder, VacancyRuleSet ruleId)
        {
            return ruleBuilder.Configure(c =>
            {
                // Set rule type in context so it can be returned in error object
                foreach (var validator in c.Validators)
                {
                    validator.CustomStateProvider = s => ruleId;
                }
            });
        }

        private static bool CanRunValidator(this ValidationContext context, VacancyRuleSet validationToCheck)
        {
            var validationsToRun = (VacancyRuleSet)context.RootContextData[ValidationConstants.ValidationsRulesKey];

            return (validationsToRun & validationToCheck) > 0;
        }
    }
}