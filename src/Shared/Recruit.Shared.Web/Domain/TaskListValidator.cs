using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Shared.Web.Domain;

/// <summary>
/// Should not be persisted as an integer anywhere, consider the enum integer values ephemeral
/// </summary>
[Flags]
public enum TaskListItemFlags: ulong
{
    // Contains Items for both Employer & Provider - importantly in order as they appear in the task list
    None = 0UL,
    
    // Section One
    NameOfEmployer = 1UL, // Provider only
    AdvertTitle = NameOfEmployer << 1,
    OrganisationName = AdvertTitle << 1, // Employer only
    TrainingCourse = OrganisationName << 1,
    TrainingProvider = TrainingCourse << 1, // Employer only
    SummaryDescription = TrainingProvider << 1,
    WhatWillTheyDoAtWork = SummaryDescription << 1,
    HowTheyWillTrain = WhatWillTheyDoAtWork << 1,
    
    // Section Two
    ClosingAndStartDates = 1UL << 10,
    DurationAndWorkHours = ClosingAndStartDates << 1,
    PayRateAndBenefits = DurationAndWorkHours << 1,
    NumberOfPositions = PayRateAndBenefits << 1,
    Locations = NumberOfPositions << 1,
    
    // Section Three
    Skills = 1UL << 20,
    Qualifications = Skills << 1,
    FutureProspects = Qualifications << 1,
    OtherThingsToConsider = FutureProspects << 1,
    
    // Section Four
    NameOfEmployerOnAdvert = 1UL << 30,
    EmployerInformation = NameOfEmployerOnAdvert << 1,
    ContactDetails = EmployerInformation << 1,
    WebsiteForApplications = ContactDetails << 1,
    
    // Section Five
    AdditionalQuestions = 1UL << 40,
    
    // All
    All = ulong.MaxValue,
}

/// <summary>
/// Should not be persisted as an integer anywhere, consider the enum integer values ephemeral
/// </summary>
[Flags]
public enum EmployerTaskListSectionFlags: ulong
{
    One = TaskListItemFlags.AdvertTitle 
          | TaskListItemFlags.OrganisationName
          | TaskListItemFlags.TrainingCourse
          | TaskListItemFlags.TrainingProvider
          | TaskListItemFlags.SummaryDescription
          | TaskListItemFlags.WhatWillTheyDoAtWork
          | TaskListItemFlags.HowTheyWillTrain,
    Two = TaskListItemFlags.ClosingAndStartDates 
          | TaskListItemFlags.DurationAndWorkHours
          | TaskListItemFlags.PayRateAndBenefits
          | TaskListItemFlags.NumberOfPositions
          | TaskListItemFlags.Locations,
    Three = TaskListItemFlags.Skills
            | TaskListItemFlags.Qualifications
            | TaskListItemFlags.FutureProspects
            | TaskListItemFlags.OtherThingsToConsider,
    Four = TaskListItemFlags.NameOfEmployerOnAdvert
           | TaskListItemFlags.EmployerInformation
           | TaskListItemFlags.ContactDetails
           | TaskListItemFlags.WebsiteForApplications,
    Five = TaskListItemFlags.AdditionalQuestions,
    All = One | Two | Three | Four | Five
}

/// <summary>
/// Should not be persisted as an integer anywhere, consider the enum integer values ephemeral
/// </summary>
[Flags]
public enum ProviderTaskListSectionFlags: ulong
{
    One = TaskListItemFlags.NameOfEmployer
          | TaskListItemFlags.AdvertTitle
          | TaskListItemFlags.TrainingCourse
          | TaskListItemFlags.SummaryDescription
          | TaskListItemFlags.WhatWillTheyDoAtWork
          | TaskListItemFlags.HowTheyWillTrain,
    Two = EmployerTaskListSectionFlags.Two,
    Three = EmployerTaskListSectionFlags.Three,
    Four = EmployerTaskListSectionFlags.Four,
    Five = EmployerTaskListSectionFlags.Five,
    All = One | Two | Three | Four | Five
}

internal static class IRuleBuilderOptionsExtensions
{
    internal static IRuleBuilderOptions<Vacancy, T> RunCondition<T>(this IRuleBuilderOptions<Vacancy, T> context, TaskListItemFlags item)
    {
        return context
            .WithErrorCode(Enum.GetName(item))
            .Configure(c=>c.ApplyCondition(x => x.CanRunItemValidators(item)));
    }
        
    private static bool CanRunItemValidators<T>(this ValidationContext<T> context, TaskListItemFlags items)
    {
        var itemValidatorsToRun = (TaskListItemFlags)context.RootContextData[TaskListValidator.ItemRulesetKey];
        return (itemValidatorsToRun & items) > 0;
    }
}

public interface ITaskListValidator
{
    Task<bool> IsCompleteAsync(Vacancy vacancy, EmployerTaskListSectionFlags flags);
    Task<bool> IsCompleteAsync(Vacancy vacancy, ProviderTaskListSectionFlags flags);
    Task<bool> IsCompleteAsync(Vacancy vacancy, TaskListItemFlags flags);
    Task<Dictionary<TaskListItemFlags, bool>> GetItemStatesAsync(Vacancy vacancy, EmployerTaskListSectionFlags flags);
    Task<Dictionary<TaskListItemFlags, bool>> GetItemStatesAsync(Vacancy vacancy, ProviderTaskListSectionFlags flags);
    Task<Dictionary<TaskListItemFlags, bool>> GetItemStatesAsync(Vacancy vacancy, TaskListItemFlags flags);
}

public class TaskListValidator : AbstractValidator<Vacancy>, ITaskListValidator
{
    public const string ItemRulesetKey = "ItemRuleSets";

    // these prevent bad things happening with AutoFixture
    public new CascadeMode ClassLevelCascadeMode => CascadeMode.Continue; 
    public new CascadeMode RuleLevelCascadeMode => CascadeMode.Stop; 
    
    public TaskListValidator()
    {
        // ========================
        // Section One
        // ========================
        RuleFor(x => x.Title).NotEmpty().RunCondition(TaskListItemFlags.AdvertTitle);
        RuleFor(x => x.ProgrammeId).NotEmpty().RunCondition(TaskListItemFlags.TrainingCourse);
        RuleFor(x => x.ShortDescription).NotEmpty().RunCondition(TaskListItemFlags.SummaryDescription);
        RuleFor(x => x.Description).NotEmpty().RunCondition(TaskListItemFlags.WhatWillTheyDoAtWork);
        // How the apprenticeship will train - optional so omitted, will always be true
        
        // Employer only
        RuleFor(x => x.AccountLegalEntityPublicHashedId).NotEmpty().RunCondition(TaskListItemFlags.OrganisationName);
        RuleFor(x => x.TrainingProvider).NotNull().RunCondition(TaskListItemFlags.TrainingProvider);
        When(x => x.TrainingProvider is not null, () =>
        {
            RuleFor(x => x.TrainingProvider.Ukprn).NotEmpty().RunCondition(TaskListItemFlags.TrainingProvider);
        });

        // Provider only
        RuleFor(x => x.EmployerAccountId).NotEmpty().RunCondition(TaskListItemFlags.NameOfEmployer);
        RuleFor(x => x.AccountLegalEntityPublicHashedId).NotEmpty().RunCondition(TaskListItemFlags.NameOfEmployer);
        
        // ========================
        // Section Two
        // ========================
        RuleFor(x => x.StartDate).NotNull().RunCondition(TaskListItemFlags.ClosingAndStartDates);
        RuleFor(x => x.ClosingDate).NotNull().RunCondition(TaskListItemFlags.ClosingAndStartDates);
        RuleFor(x => x.Wage).NotNull().RunCondition(TaskListItemFlags.DurationAndWorkHours);
        When(x => x.Wage is not null, () =>
        {
            RuleFor(x => x.Wage.Duration).NotNull().RunCondition(TaskListItemFlags.DurationAndWorkHours);
            RuleFor(x => x.Wage.WorkingWeekDescription).NotEmpty().RunCondition(TaskListItemFlags.DurationAndWorkHours);
            RuleFor(x => x.Wage.WeeklyHours).NotNull().RunCondition(TaskListItemFlags.DurationAndWorkHours);
        });
        RuleFor(x => x.Wage).NotNull().RunCondition(TaskListItemFlags.PayRateAndBenefits);
        When(x => x.Wage is not null, () =>
        {
            RuleFor(x => x.Wage.WageType).NotNull().RunCondition(TaskListItemFlags.PayRateAndBenefits);
        });
        RuleFor(x => x.NumberOfPositions).NotNull().RunCondition(TaskListItemFlags.NumberOfPositions);
        When(x => x.EmployerLocationOption is AvailableWhere.AcrossEngland, () =>
        {
            RuleFor(x => x.EmployerLocationInformation).NotNull().RunCondition(TaskListItemFlags.Locations);
        });
        When(x => x.EmployerLocationOption is AvailableWhere.MultipleLocations or AvailableWhere.OneLocation, () =>
        {
            RuleFor(x => x.EmployerLocations).Must(x => x is { Count: >0 }).RunCondition(TaskListItemFlags.Locations);
        });
        When(x => x.EmployerLocationOption is null, () =>
        {
            RuleFor(x => x.EmployerLocation).NotNull().RunCondition(TaskListItemFlags.Locations);
        });
        
        // ========================
        // Section Three
        // ========================
        When(x => x.ApprenticeshipType is not ApprenticeshipTypes.Foundation, () =>
        {
            RuleFor(x => x.Skills).Must(x => x is { Count: >0 }).RunCondition(TaskListItemFlags.Skills);
            RuleFor(x => x.HasOptedToAddQualifications).NotNull().RunCondition(TaskListItemFlags.Qualifications);
        });
        When(x => x.HasOptedToAddQualifications ?? false, () =>
        {
            RuleFor(x => x.Qualifications).Must(x => x is { Count: >0 }).RunCondition(TaskListItemFlags.Qualifications);
        });
        RuleFor(x => x.OutcomeDescription).NotEmpty().RunCondition(TaskListItemFlags.FutureProspects);
        // Other things to consider - optional so omitted, will always be true
        
        // ========================
        // Section Four
        // ========================
        RuleFor(x => x.EmployerNameOption).NotNull().RunCondition(TaskListItemFlags.NameOfEmployerOnAdvert);
        RuleFor(x => x.EmployerDescription).NotEmpty().RunCondition(TaskListItemFlags.EmployerInformation);
        // Contact details - optional so omitted, will always be true
        RuleFor(x => x.ApplicationMethod).NotNull().RunCondition(TaskListItemFlags.WebsiteForApplications);
        
        // ========================
        // Section Five
        // ========================
        When(x => x.ApplicationMethod is not ApplicationMethod.ThroughExternalApplicationSite, () =>
        {
            RuleFor(x => x.HasSubmittedAdditionalQuestions).Must(x => x).RunCondition(TaskListItemFlags.AdditionalQuestions);
        });
    }
    
    public Task<bool> IsCompleteAsync(Vacancy vacancy, EmployerTaskListSectionFlags flags)
    {
        return IsCompleteAsync(vacancy, (TaskListItemFlags)flags);
    }

    public Task<bool> IsCompleteAsync(Vacancy vacancy, ProviderTaskListSectionFlags flags)
    {
        return IsCompleteAsync(vacancy, (TaskListItemFlags)flags);
    }

    public async Task<bool> IsCompleteAsync(Vacancy vacancy, TaskListItemFlags flags)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        var context = new ValidationContext<Vacancy>(vacancy);
        context.RootContextData.Add(ItemRulesetKey, flags);
        var result = await ValidateAsync(context);
        return result.IsValid;
    }

    public Task<Dictionary<TaskListItemFlags, bool>> GetItemStatesAsync(Vacancy vacancy, EmployerTaskListSectionFlags flags)
    {
        return GetItemStatesAsync(vacancy, (TaskListItemFlags)flags);
    }

    public Task<Dictionary<TaskListItemFlags, bool>> GetItemStatesAsync(Vacancy vacancy, ProviderTaskListSectionFlags flags)
    {
        return GetItemStatesAsync(vacancy, (TaskListItemFlags)flags);
    }

    public async Task<Dictionary<TaskListItemFlags, bool>> GetItemStatesAsync(Vacancy vacancy, TaskListItemFlags flags)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        
        var result = new Dictionary<TaskListItemFlags, bool>();
        var type = typeof(TaskListItemFlags);

        var context = new ValidationContext<Vacancy>(vacancy);
        context.RootContextData.Add(ItemRulesetKey, flags);
        var validationResults = await ValidateAsync(context);
        
        foreach (ulong item in Enum.GetValuesAsUnderlyingType(type))
        {
            if (!flags.HasFlag((TaskListItemFlags)item))
            {
                continue;
            }

            string name = Enum.GetName(type, item)!;
            switch ((TaskListItemFlags)item)
            {
                case TaskListItemFlags.All:
                case TaskListItemFlags.None: break;
                default:
                    bool invalid = validationResults.Errors.Any(x => x.ErrorCode == name);
                    result.Add((TaskListItemFlags)item, !invalid);
                    break;
            }
        }

        return result;
    }
}