using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Domain;

public sealed class EmployerTaskListStateView: TaskListStateViewBase
{
    private EmployerTaskListStateView()
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

    public static EmployerTaskListStateView CreateEmpty()
    {
        return new EmployerTaskListStateView();
    }

    public EmployerTaskListStateView(Dictionary<TaskListItemFlags, bool> completeStates, Vacancy vacancy)
    {
        ArgumentNullException.ThrowIfNull(completeStates);
        ArgumentNullException.ThrowIfNull(vacancy);
        
        CompleteStates = completeStates;
        var apprenticeshipType = vacancy.ApprenticeshipType.GetValueOrDefault();

        SectionOneState = GetSectionState(EmployerTaskListSectionFlags.One, TaskListItemFlags.AdvertTitle);
        SectionTwoState = GetSectionState(EmployerTaskListSectionFlags.Two, TaskListItemFlags.ClosingAndStartDates);
        SectionThreeState = apprenticeshipType switch
        {
            ApprenticeshipTypes.Foundation => GetSectionState(EmployerTaskListSectionFlags.Three, TaskListItemFlags.FutureProspects),
            _ => GetSectionState(EmployerTaskListSectionFlags.Three, TaskListItemFlags.Skills | TaskListItemFlags.FutureProspects),
        };
        SectionFourState = GetSectionState(EmployerTaskListSectionFlags.Four, TaskListItemFlags.NameOfEmployerOnAdvert);
        SectionFiveState = vacancy.ApplicationMethod is ApplicationMethod.ThroughExternalApplicationSite
            ? VacancyTaskListSectionState.NotRequired
            : GetSectionState(EmployerTaskListSectionFlags.Five, TaskListItemFlags.AdditionalQuestions);
        SectionSixState = vacancy.Status == VacancyStatus.Submitted
            ? VacancyTaskListSectionState.Completed
            : AllFlagsCompleted((TaskListItemFlags)EmployerTaskListSectionFlags.All)
                ? VacancyTaskListSectionState.InProgress
                : VacancyTaskListSectionState.NotStarted;
        
        // Section One
        NameOfOrganisationEditable = CompleteStates[TaskListItemFlags.AdvertTitle];
        TrainingCourseEditable = CompleteStates[TaskListItemFlags.OrganisationName];
        TrainingProviderEditable = CompleteStates[TaskListItemFlags.TrainingCourse];
        ApprenticeshipSummaryEditable = CompleteStates[TaskListItemFlags.TrainingProvider];
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
        NameOfEmployerOnAdvertEditable = CompleteStates[TaskListItemFlags.NameOfEmployerOnAdvert] || SectionThreeState == VacancyTaskListSectionState.Completed;
        EmployerInformationEditable = CompleteStates[TaskListItemFlags.NameOfEmployerOnAdvert];
        ContactDetailsEditable = CompleteStates[TaskListItemFlags.EmployerInformation];
        WebsiteForApplicationsEditable = CompleteStates[TaskListItemFlags.EmployerInformation];
        
        // Section Five
        AdditionalQuestionsEditable = vacancy.ApplicationMethod is not ApplicationMethod.ThroughExternalApplicationSite && SectionFourState == VacancyTaskListSectionState.Completed;
        
        // Section Six
        CheckYourAnswersEditable = SectionSixState != VacancyTaskListSectionState.NotStarted;
    }

    private VacancyTaskListSectionState GetSectionState(EmployerTaskListSectionFlags sectionFlags, TaskListItemFlags anyItemFlag)
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
    public bool NameOfOrganisationEditable { get; }
    public bool TrainingCourseEditable { get; }
    public bool TrainingProviderEditable { get; }
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
    public bool NameOfEmployerOnAdvertEditable { get; }
    public bool EmployerInformationEditable { get; }
    public bool ContactDetailsEditable { get; }
    public bool WebsiteForApplicationsEditable { get; }
    
    // Section Five
    public bool AdditionalQuestionsEditable { get; }
    
    // Section Six
    public bool CheckYourAnswersEditable { get; }
}