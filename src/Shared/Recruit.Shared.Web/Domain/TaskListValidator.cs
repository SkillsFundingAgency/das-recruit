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
public enum TaskListSectionFlags: ulong
{
    One = 1UL << 10,
    Two = 1UL << 20,
    Three = 1UL << 30,
    Four = 1UL << 40,
    Five = 1UL << 50,
    All = ulong.MaxValue,
}

/// <summary>
/// Should not be persisted as an integer anywhere, consider the enum integer values ephemeral
/// </summary>
[Flags]
public enum TaskListItemFlags: ulong
{
    // Section One
    Title = 1UL,
    OrganisationName = 1UL << 2,
    TrainingCourse = 1UL << 3,
    TrainingProvider = 1UL << 4,
    SummaryDescription = 1UL << 5,
    WhatWillTheyDoAtWork = 1UL << 6,
    HowTheyWillTrain = 1UL << 7,
    
    // Section Two
    ClosingAndStartDates = 1UL << 8,
    DurationAndWorkHours = 1UL << 9,
    PayRateAndBenefits = 1UL << 10,
    NumberOfPositions = 1UL << 11,
    Locations = 1UL << 12,
    
    // Section Three
    Skills = 1UL << 13,
    Qualifications = 1UL << 14,
    FutureProspects = 1UL << 15,
    OtherThingsToConsider = 1UL << 16,
    
    // Section Four
    NameOfEmployerOnAdvert = 1UL << 17,
    EmployerInformation = 1UL << 18,
    ContactDetails = 1UL << 19,
    WebsiteForApplications = 1UL << 20,
    
    // Section Five
    AdditionalQuestions = 1UL << 21,
    
    // All
    All = ulong.MaxValue,
}

internal static class IRuleBuilderOptionsExtensions
{
    internal static IRuleBuilderOptions<Vacancy, T> RunCondition<T>(this IRuleBuilderOptions<Vacancy, T> context, TaskListSectionFlags section, TaskListItemFlags item)
    {
        return context
            .WithErrorCode(Enum.GetName(item))
            .Configure(c=>c.ApplyCondition(x => x.CanRunSectionValidators(section) || x.CanRunItemValidators(item)));
    }
        
    private static bool CanRunSectionValidators<T>(this ValidationContext<T> context, TaskListSectionFlags sections)
    {
        var sectionValidatorsToRun = (TaskListSectionFlags)context.RootContextData[TaskListValidator.SectionRulesetKey];
        return (sectionValidatorsToRun & sections) > 0;
    }
    
    private static bool CanRunItemValidators<T>(this ValidationContext<T> context, TaskListItemFlags items)
    {
        var itemValidatorsToRun = (TaskListItemFlags)context.RootContextData[TaskListValidator.ItemRulesetKey];
        return (itemValidatorsToRun & items) > 0;
    }
}

public interface ITaskListValidator
{
    Task<bool> IsCompleteAsync(Vacancy vacancy);
    Task<bool> IsSectionCompleteAsync(Vacancy vacancy, TaskListSectionFlags section);
    Task<bool> IsItemCompleteAsync(Vacancy vacancy, TaskListItemFlags item);
    Task<Dictionary<TaskListItemFlags, bool>> GetAllItemStatesAsync(Vacancy vacancy);
}

public class TaskListValidator : AbstractValidator<Vacancy>, ITaskListValidator
{
    public const string SectionRulesetKey = "SectionRuleSets";
    public const string ItemRulesetKey = "ItemRuleSets";
    
    public TaskListValidator()
    {
        // ========================
        // Section One
        // ========================
        RuleFor(x => x.Title).NotEmpty().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.Title);
        RuleFor(x => x.AccountLegalEntityPublicHashedId).NotEmpty().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.OrganisationName);
        RuleFor(x => x.ProgrammeId).NotEmpty().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.TrainingCourse);
        RuleFor(x => x.TrainingProvider).NotNull().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.TrainingProvider);
        When(x => x.TrainingProvider is not null, () =>
        {
            RuleFor(x => x.TrainingProvider.Ukprn).NotEmpty().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.TrainingProvider);
        });
        RuleFor(x => x.ShortDescription).NotEmpty().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.SummaryDescription);
        RuleFor(x => x.Description).NotEmpty().RunCondition(TaskListSectionFlags.One, TaskListItemFlags.WhatWillTheyDoAtWork);
        // How the apprenticeship will train - optional so omitted, will always be true
        
        // ========================
        // Section Two
        // ========================
        RuleFor(x => x.StartDate).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.ClosingAndStartDates);
        RuleFor(x => x.ClosingDate).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.ClosingAndStartDates);
        RuleFor(x => x.Wage).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.DurationAndWorkHours);
        When(x => x.Wage is not null, () =>
        {
            RuleFor(x => x.Wage.Duration).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.DurationAndWorkHours);
            RuleFor(x => x.Wage.WorkingWeekDescription).NotEmpty().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.DurationAndWorkHours);
            RuleFor(x => x.Wage.WeeklyHours).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.DurationAndWorkHours);
        });
        RuleFor(x => x.Wage).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.PayRateAndBenefits);
        When(x => x.Wage is not null, () =>
        {
            RuleFor(x => x.Wage.WageType).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.PayRateAndBenefits);
        });
        RuleFor(x => x.NumberOfPositions).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.NumberOfPositions);
        When(x => x.EmployerLocationOption is AvailableWhere.AcrossEngland, () =>
        {
            RuleFor(x => x.EmployerLocationInformation).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.Locations);
        });
        When(x => x.EmployerLocationOption is AvailableWhere.MultipleLocations or AvailableWhere.OneLocation, () =>
        {
            RuleFor(x => x.EmployerLocations).Must(x => x is { Count: >0 }).RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.Locations);
        });
        When(x => x.EmployerLocationOption is null, () =>
        {
            RuleFor(x => x.EmployerLocation).NotNull().RunCondition(TaskListSectionFlags.Two, TaskListItemFlags.Locations);
        });
        
        // ========================
        // Section Three
        // ========================
        When(x => x.ApprenticeshipType is not ApprenticeshipTypes.Foundation, () =>
        {
            RuleFor(x => x.Skills).Must(x => x is { Count: >0 }).RunCondition(TaskListSectionFlags.Three, TaskListItemFlags.Skills);
        });
        When(x => x.HasOptedToAddQualifications ?? false, () =>
        {
            RuleFor(x => x.Qualifications).Must(x => x is { Count: >0 }).RunCondition(TaskListSectionFlags.Three, TaskListItemFlags.Qualifications);
        });
        RuleFor(x => x.OutcomeDescription).NotEmpty().RunCondition(TaskListSectionFlags.Three, TaskListItemFlags.FutureProspects);
        // Other things to consider - optional so omitted, will always be true
        
        // ========================
        // Section Four
        // ========================
        RuleFor(x => x.EmployerNameOption).NotNull().RunCondition(TaskListSectionFlags.Four, TaskListItemFlags.NameOfEmployerOnAdvert);
        RuleFor(x => x.EmployerDescription).NotEmpty().RunCondition(TaskListSectionFlags.Four, TaskListItemFlags.EmployerInformation);
        // Contact details - optional so omitted, will always be true
        RuleFor(x => x.ApplicationMethod).NotNull().RunCondition(TaskListSectionFlags.Four, TaskListItemFlags.WebsiteForApplications);
        
        // ========================
        // Section Five
        // ========================
        RuleFor(x => x.HasSubmittedAdditionalQuestions).Must(x => x).RunCondition(TaskListSectionFlags.Five, TaskListItemFlags.AdditionalQuestions);
    }

    public async Task<bool> IsCompleteAsync(Vacancy vacancy)
    {
        var context = new ValidationContext<Vacancy>(vacancy);
        context.RootContextData.Add(SectionRulesetKey, ulong.MaxValue);
        context.RootContextData.Add(ItemRulesetKey, ulong.MaxValue);
        var result = await ValidateAsync(context);
        return result.IsValid;
    }
    
    public async Task<bool> IsSectionCompleteAsync(Vacancy vacancy, TaskListSectionFlags section)
    {
        var context = new ValidationContext<Vacancy>(vacancy);
        context.RootContextData.Add(SectionRulesetKey, section);
        context.RootContextData.Add(ItemRulesetKey, ulong.MinValue);
        var result = await ValidateAsync(context);
        return result.IsValid;
    }
    
    public async Task<bool> IsItemCompleteAsync(Vacancy vacancy, TaskListItemFlags item)
    {
        var context = new ValidationContext<Vacancy>(vacancy);
        context.RootContextData.Add(SectionRulesetKey, ulong.MinValue);
        context.RootContextData.Add(ItemRulesetKey, item);
        var result = await ValidateAsync(context);
        return result.IsValid;
    }

    public async Task<Dictionary<TaskListItemFlags, bool>> GetAllItemStatesAsync(Vacancy vacancy)
    {
        var result = new Dictionary<TaskListItemFlags, bool>();
        var type = typeof(TaskListItemFlags);

        var context = new ValidationContext<Vacancy>(vacancy);
        context.RootContextData.Add(SectionRulesetKey, ulong.MinValue);
        context.RootContextData.Add(ItemRulesetKey, ulong.MaxValue);
        var validationResults = await ValidateAsync(context);
        
        foreach (ulong item in Enum.GetValuesAsUnderlyingType(type))
        {
            string name = Enum.GetName(type, item)!;
            if (item == ulong.MaxValue)
            {
                result.Add((TaskListItemFlags) item, validationResults.IsValid);
            }
            else
            {
                bool invalid = validationResults.Errors.Any(x => x.ErrorCode == name);
                result.Add((TaskListItemFlags) item, !invalid);
            }
        }

        return result;
    }
}