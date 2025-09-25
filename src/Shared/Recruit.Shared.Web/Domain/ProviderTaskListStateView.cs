using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Domain;

public sealed class ProviderTaskListStateView : TaskListStateViewBase
{
    private ProviderTaskListStateView()
    {
        SectionOneState = VacancyTaskListSectionState.NotStarted;
        SectionTwoState = VacancyTaskListSectionState.NotStarted;
        SectionThreeState = VacancyTaskListSectionState.NotStarted;
        SectionFourState = VacancyTaskListSectionState.NotStarted;
        SectionFiveState = VacancyTaskListSectionState.NotStarted;
        SectionSixState = VacancyTaskListSectionState.NotStarted;

        CompleteStates = new Dictionary<TaskListItemFlags, bool>
        {
            { TaskListItemFlags.AdvertTitle, false }
        };
    }

    public static ProviderTaskListStateView CreateEmpty()
    {
        return new ProviderTaskListStateView();
    }

    public ProviderTaskListStateView(Dictionary<TaskListItemFlags, bool> completeStates, Vacancy vacancy)
    {
        ArgumentNullException.ThrowIfNull(completeStates);
        ArgumentNullException.ThrowIfNull(vacancy);
        
        CompleteStates = completeStates;
        var apprenticeshipType = vacancy.ApprenticeshipType.GetValueOrDefault();

        SectionOneState = GetSectionState(ProviderTaskListSectionFlags.One, TaskListItemFlags.NameOfEmployer);
        SectionTwoState = GetSectionState(ProviderTaskListSectionFlags.Two, TaskListItemFlags.ClosingAndStartDates);
        SectionThreeState = vacancy.ApprenticeshipType.GetValueOrDefault() switch
        {
            ApprenticeshipTypes.Foundation => GetSectionState(ProviderTaskListSectionFlags.Three, TaskListItemFlags.FutureProspects),
            _ => GetSectionState(ProviderTaskListSectionFlags.Three, TaskListItemFlags.Skills | TaskListItemFlags.FutureProspects),
        };
        SectionFourState = GetSectionState(ProviderTaskListSectionFlags.Four, TaskListItemFlags.NameOfEmployerOnAdvert);
        SectionFiveState = vacancy.ApplicationMethod is ApplicationMethod.ThroughExternalApplicationSite
            ? VacancyTaskListSectionState.NotRequired
            : GetSectionState(ProviderTaskListSectionFlags.Five, TaskListItemFlags.AdditionalQuestions);
        SectionSixState = vacancy.Status == VacancyStatus.Submitted
            ? VacancyTaskListSectionState.Completed
            : AllFlagsCompleted((TaskListItemFlags)ProviderTaskListSectionFlags.All)
                ? VacancyTaskListSectionState.InProgress
                : VacancyTaskListSectionState.NotStarted;
        
        // Section One
        NameOfEmployerEditable = true;
        VacancyTitleEditable = CompleteStates[TaskListItemFlags.NameOfEmployer];
        TrainingCourseEditable = CompleteStates[TaskListItemFlags.AdvertTitle];
        ApprenticeshipSummaryEditable = CompleteStates[TaskListItemFlags.TrainingCourse];
        WhatWillTheyDoAtWorkEditable = CompleteStates[TaskListItemFlags.SummaryDescription];
        HowWillTheyTrainEditable = CompleteStates[TaskListItemFlags.WhatWillTheyDoAtWork];
        
        // Section Two
        ClosingAndStartDatesEditable = SectionOneState == VacancyTaskListSectionState.Completed;
        DurationAndWorkingHoursEditable = CompleteStates[TaskListItemFlags.ClosingAndStartDates];
        PayRateAndBenefitsEditable = CompleteStates[TaskListItemFlags.DurationAndWorkHours];
        NumberOfPositionsEditable = CompleteStates[TaskListItemFlags.PayRateAndBenefits];
        LocationsEditable = CompleteStates[TaskListItemFlags.NumberOfPositions];
        
        // Section Three
        if (apprenticeshipType == ApprenticeshipTypes.Foundation)
        {
            SkillsEditable = false;
            QualificationsEditable = false;
            FutureProspectsEditable = SectionTwoState == VacancyTaskListSectionState.Completed;
        }
        else
        {
            SkillsEditable = SectionTwoState == VacancyTaskListSectionState.Completed;
            QualificationsEditable = CompleteStates[TaskListItemFlags.Skills];
            FutureProspectsEditable = CompleteStates[TaskListItemFlags.FutureProspects] || CompleteStates[TaskListItemFlags.Qualifications];
        }
        OtherThingsToConsiderEditable = CompleteStates[TaskListItemFlags.FutureProspects];
        
        // Section Four
        NameOfEmployerOnVacancyEditable = CompleteStates[TaskListItemFlags.NameOfEmployerOnAdvert] || SectionThreeState == VacancyTaskListSectionState.Completed;
        EmployerInformationEditable = CompleteStates[TaskListItemFlags.NameOfEmployerOnAdvert];
        ContactDetailsEditable = CompleteStates[TaskListItemFlags.EmployerInformation];
        WebsiteForApplicationsEditable = CompleteStates[TaskListItemFlags.EmployerInformation];
        
        // Section Five
        AdditionalQuestionsEditable = SectionFourState == VacancyTaskListSectionState.Completed;
        
        // Section Six
        CheckYourAnswersEditable = SectionSixState != VacancyTaskListSectionState.NotStarted;
    }

    private VacancyTaskListSectionState GetSectionState(ProviderTaskListSectionFlags sectionFlags, TaskListItemFlags anyItemFlag)
    {
        if (AllFlagsCompleted((TaskListItemFlags)sectionFlags))
        {
            return VacancyTaskListSectionState.Completed;
        }

        return AnyFlagsCompleted(anyItemFlag)
            ? VacancyTaskListSectionState.InProgress
            : VacancyTaskListSectionState.NotStarted;
    }

    public VacancyTaskListSectionState SectionOneState { get; }
    public VacancyTaskListSectionState SectionTwoState { get; }
    public VacancyTaskListSectionState SectionThreeState { get; }
    public VacancyTaskListSectionState SectionFourState { get; }
    public VacancyTaskListSectionState SectionFiveState { get; }
    public VacancyTaskListSectionState SectionSixState { get; }

    // Section One
    public bool NameOfEmployerEditable { get; }
    public bool VacancyTitleEditable { get; }
    public bool TrainingCourseEditable { get; }
    public bool ApprenticeshipSummaryEditable { get; }
    public bool WhatWillTheyDoAtWorkEditable { get; }
    public bool HowWillTheyTrainEditable { get; }

    // Section Two
    public bool ClosingAndStartDatesEditable { get; }
    public bool DurationAndWorkingHoursEditable { get; }
    public bool PayRateAndBenefitsEditable { get; }
    public bool NumberOfPositionsEditable { get; }
    public bool LocationsEditable { get; }

    // Section Three
    public bool SkillsEditable { get; }
    public bool QualificationsEditable { get; }
    public bool FutureProspectsEditable { get; }
    public bool OtherThingsToConsiderEditable { get; }

    // Section Four
    public bool NameOfEmployerOnVacancyEditable { get; }
    public bool EmployerInformationEditable { get; }
    public bool ContactDetailsEditable { get; }
    public bool WebsiteForApplicationsEditable { get; }
    
    // Section Five
    public bool AdditionalQuestionsEditable { get; }
    
    // Section Six
    public bool CheckYourAnswersEditable { get; }
}