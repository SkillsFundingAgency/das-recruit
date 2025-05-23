using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.EmployerTaskListViewTests;

public class WhenGettingSectionOneState
{
    public static readonly IEnumerable<object[]> SectionOneTestCases =
        new List<object[]>
        {
            new object[] { TaskListItemFlags.None, VacancyTaskListSectionState.NotStarted },
            new object[] { TaskListItemFlags.AdvertTitle, VacancyTaskListSectionState.InProgress },
            new object[] { (TaskListItemFlags)EmployerTaskListSectionFlags.One, VacancyTaskListSectionState.Completed },
        };
    
    [TestCaseSource(nameof(SectionOneTestCases))]
    public void Section_One_Should_Have_State(TaskListItemFlags flags, VacancyTaskListSectionState expectedState)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => false);
        foreach (var kvp in states.Where(kvp => flags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = true;
        }
        
        // act
        var sut = new EmployerTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionOneState.Should().Be(expectedState);
    }
    
    public static readonly IEnumerable<object[]> SectionOneFieldsTestCases =
        new List<object[]>
        {
            new object[] { (EmployerTaskListStateView v) => v.NameOfOrganisationEditable, TaskListItemFlags.AdvertTitle },
            new object[] { (EmployerTaskListStateView v) => v.TrainingCourseEditable, TaskListItemFlags.OrganisationName },
            new object[] { (EmployerTaskListStateView v) => v.TrainingProviderEditable, TaskListItemFlags.TrainingCourse },
            new object[] { (EmployerTaskListStateView v) => v.ApprenticeshipSummaryEditable, TaskListItemFlags.TrainingProvider },
            new object[] { (EmployerTaskListStateView v) => v.WhatWillTheyDoAtWorkEditable, TaskListItemFlags.SummaryDescription },
            new object[] { (EmployerTaskListStateView v) => v.HowWillTheyTrainEditable, TaskListItemFlags.WhatWillTheyDoAtWork },
        };
    
    [TestCaseSource(nameof(SectionOneFieldsTestCases))]
    public void Section_One_Fields_Require_Dependent_Fields(Func<EmployerTaskListStateView, bool> fieldFunc, TaskListItemFlags flag)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        states[flag] = false;
        
        // act
        var sut = new EmployerTaskListStateView(states, vacancy);
        bool actual = fieldFunc(sut);
        
        // assert
        actual.Should().BeFalse();
    }
}